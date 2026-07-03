using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.BL;
using TourPlanner.BL.Exceptions;
using TourPlanner.Models;

namespace TourPlanner.API.Controllers
{
    [ApiController]
    [Route("api/tours")]
    public class TourController : ControllerBase
    {
        private readonly ITourService _tourService;
        private readonly ILogger<TourController> _logger;

        public TourController(ITourService tourService, ILogger<TourController> logger)
        {
            _tourService = tourService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tour>>> GetAll()
        {
            var tours = await _tourService.GetAllAsync();
            return Ok(tours);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Tour>>> Search([FromQuery] string q = "")
        {
            var tours = await _tourService.SearchAsync(q);
            return Ok(tours);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tour>> GetById(Guid id)
        {
            var tour = await _tourService.GetByIdAsync(id);
            if (tour == null)
                return NotFound(new { error = $"Tour mit ID '{id}' wurde nicht gefunden." });
            return Ok(tour);
        }

        [HttpPost]
        public async Task<ActionResult<Tour>> Create(Tour tour)
        {
            try
            {
                var created = await _tourService.CreateAsync(tour);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (TourPlannerException ex)
            {
                return HandleBlException(ex, "Error creating tour");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Tour>> Update(Guid id, Tour tour)
        {
            var existing = await _tourService.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { error = $"Tour mit ID '{id}' wurde nicht gefunden." });

            tour.Id = id;
            try
            {
                var updated = await _tourService.UpdateAsync(tour);
                return Ok(updated);
            }
            catch (TourPlannerException ex)
            {
                return HandleBlException(ex, $"Error updating tour {id}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _tourService.DeleteAsync(id);
                return NoContent();
            }
            catch (TourNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (TourPlannerException ex)
            {
                return HandleBlException(ex, $"Error deleting tour {id}");
            }
        }

        [HttpPost("{id}/image")]
        public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
        {
            var tour = await _tourService.GetByIdAsync(id);
            if (tour == null)
                return NotFound(new { error = $"Tour mit ID '{id}' wurde nicht gefunden." });

            var imageDir = Path.Combine(Directory.GetCurrentDirectory(),
                HttpContext.RequestServices.GetRequiredService<IConfiguration>()["ImageDirectory"] ?? "Images");

            if (!Directory.Exists(imageDir))
                Directory.CreateDirectory(imageDir);

            var fileName = $"{id}.png";
            var filePath = Path.Combine(imageDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            tour.RouteImage = $"/images/{fileName}";
            await _tourService.UpdateAsync(tour);

            _logger.LogInformation("Image gespeichert fuer Tour {Id}: {Path}", id, filePath);
            return Ok(new { path = tour.RouteImage });
        }

        // ===== Export / Import =====

        [HttpGet("export")]
        public async Task<IActionResult> Export()
        {
            var tours = await _tourService.GetAllAsync();

            // Nur persistente Daten exportieren, keine [NotMapped]-Attribute
            var exportData = tours.Select(t => new
            {
                t.Name, t.Description, t.From, t.To, t.TransportType,
                t.Distance, t.EstimatedTime, t.RouteImage, t.RouteGeometry, t.CreatedAt,
                TourLogs = (t.TourLogs ?? new List<TourLog>()).Select(l => new
                {
                    l.DateTime, l.Comment, l.Difficulty, l.TotalDistance, l.TotalTime, l.Rating
                })
            });

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            });

            var bytes = Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", "tours-export.json");
        }

        [HttpPost("import")]
        public async Task<ActionResult<IEnumerable<Tour>>> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Keine Datei hochgeladen." });

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var json = await reader.ReadToEndAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };

                var tours = JsonSerializer.Deserialize<List<Tour>>(json, options);
                if (tours == null || tours.Count == 0)
                    return BadRequest(new { error = "Keine Touren in der Datei gefunden." });

                var imported = await _tourService.ImportAsync(tours);
                _logger.LogInformation("{Count} Touren importiert", tours.Count);
                return Ok(imported);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Import: Ungueltiges JSON");
                return BadRequest(new { error = $"Ungueltiges JSON-Format: {ex.Message}" });
            }
            catch (TourPlannerException ex)
            {
                return HandleBlException(ex, "Error importing tours");
            }
        }

        // ===== Unique Feature: GPX-Export =====

        /// <summary>
        /// UNIQUE FEATURE: GPX-Export einer einzelnen Tour.
        /// Konvertiert die gespeicherte RouteGeometry (GeoJSON LineString) in eine
        /// valide GPX-1.1-Datei mit Track (trk -> trkseg -> trkpt lat/lon).
        /// </summary>
        [HttpGet("{id}/gpx")]
        public async Task<IActionResult> ExportGpx(Guid id)
        {
            var tour = await _tourService.GetByIdAsync(id);
            if (tour == null)
                return NotFound(new { error = $"Tour mit ID '{id}' wurde nicht gefunden." });

            if (string.IsNullOrEmpty(tour.RouteGeometry))
                return BadRequest(new { error = "Diese Tour hat keine Routendaten. Erstelle die Tour mit gueltigen Orten, damit eine Route berechnet wird." });

            try
            {
                var gpxXml = ConvertGeoJsonToGpx(tour);

                var bytes = Encoding.UTF8.GetBytes(gpxXml);
                var fileName = $"{SanitizeFileName(tour.Name)}.gpx";
                return File(bytes, "application/gpx+xml", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim GPX-Export fuer Tour {Id}", id);
                return StatusCode(422, new { error = $"RouteGeometry konnte nicht in GPX konvertiert werden: {ex.Message}" });
            }
        }

        // ===== Hilfsmethoden =====

        private ActionResult HandleBlException(TourPlannerException ex, string logContext)
        {
            _logger.LogError(ex, logContext);
            return ex switch
            {
                TourNotFoundException => NotFound(new { error = ex.Message }),
                TourValidationException => BadRequest(new { error = ex.Message }),
                RouteComputationException => StatusCode(422, new { error = ex.Message }),
                _ => StatusCode(500, new { error = "Ein interner Fehler ist aufgetreten." })
            };
        }

        internal static string ConvertGeoJsonToGpx(Tour tour)
        {
            using var doc = JsonDocument.Parse(tour.RouteGeometry!);
            var root = doc.RootElement;

            // GeoJSON-Koordinaten lesen: [[lon, lat], [lon, lat], ...]
            var coordinates = root.GetProperty("coordinates");

            XNamespace gpxNs = "http://www.topografix.com/GPX/1/1";
            var gpx = new XElement(gpxNs + "gpx",
                new XAttribute("version", "1.1"),
                new XAttribute("creator", "TourPlanner"),
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                new XElement(gpxNs + "trk",
                    new XElement(gpxNs + "name", tour.Name),
                    new XElement(gpxNs + "trkseg",
                        coordinates.EnumerateArray().Select(coord =>
                            new XElement(gpxNs + "trkpt",
                                new XAttribute("lat", coord[1].GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture)),
                                new XAttribute("lon", coord[0].GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture))
                            )
                        )
                    )
                )
            );

            var xmlDoc = new XDocument(new XDeclaration("1.0", "UTF-8", null), gpx);
            return xmlDoc.Declaration + Environment.NewLine + xmlDoc.Root;
        }

        internal static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var clean = new string(name.Where(c => !invalid.Contains(c)).ToArray());
            return string.IsNullOrWhiteSpace(clean) ? "tour" : clean;
        }
    }
}
