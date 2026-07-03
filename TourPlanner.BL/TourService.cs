using Microsoft.Extensions.Logging;
using TourPlanner.BL.Exceptions;
using TourPlanner.DAL;
using TourPlanner.Models;

namespace TourPlanner.BL
{
    public class TourService : ITourService
    {
        private readonly ITourRepository _repo;
        private readonly IOrsService _ors;
        private readonly IMapImageService _mapImage;
        private readonly string _imageDirectory;
        private readonly ILogger<TourService> _logger;

        public TourService(ITourRepository repo, IOrsService ors, IMapImageService mapImage,
            string imageDirectory, ILogger<TourService> logger)
        {
            _repo = repo;
            _ors = ors;
            _mapImage = mapImage;
            _imageDirectory = imageDirectory;
            _logger = logger;
        }

        public async Task<IEnumerable<Tour>> GetAllAsync()
        {
            _logger.LogInformation("Getting all tours");
            var tours = (await _repo.GetAllAsync()).ToList();
            tours.ForEach(ComputeAttributes);
            return tours;
        }

        public async Task<Tour?> GetByIdAsync(Guid id)
        {
            var tour = await _repo.GetByIdAsync(id);
            if (tour != null)
                ComputeAttributes(tour);
            return tour;
        }

        public async Task<Tour> CreateAsync(Tour tour)
        {
            _logger.LogInformation("Creating tour: {Name}", tour.Name);
            await FillRouteDataAsync(tour);
            await GenerateMapImageAsync(tour);

            try
            {
                var created = await _repo.CreateAsync(tour);
                ComputeAttributes(created);
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Erstellen der Tour '{Name}'", tour.Name);
                throw new TourValidationException($"Tour konnte nicht erstellt werden: {ex.Message}", ex);
            }
        }

        public async Task<Tour> UpdateAsync(Tour tour)
        {
            _logger.LogInformation("Updating tour: {Name}", tour.Name);
            await FillRouteDataAsync(tour);
            await GenerateMapImageAsync(tour);

            try
            {
                var updated = await _repo.UpdateAsync(tour);
                ComputeAttributes(updated);
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Aktualisieren der Tour '{Name}'", tour.Name);
                throw new TourValidationException($"Tour konnte nicht aktualisiert werden: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting tour {Id}", id);
            var tour = await _repo.GetByIdAsync(id);
            if (tour == null)
                throw new TourNotFoundException(id);

            try
            {
                await _repo.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Loeschen der Tour {Id}", id);
                throw new TourValidationException($"Tour konnte nicht geloescht werden: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Tour>> SearchAsync(string query)
        {
            _logger.LogInformation("Searching tours: '{Query}'", query);
            var tours = (await _repo.GetAllAsync()).ToList();
            tours.ForEach(ComputeAttributes);

            if (string.IsNullOrWhiteSpace(query))
                return tours;

            var q = query.Trim();

            return tours.Where(t =>
                Contains(t.Name, q) ||
                Contains(t.Description, q) ||
                Contains(t.From, q) ||
                Contains(t.To, q) ||
                Contains(t.TransportType.ToString(), q) ||
                Contains(t.PopularityLevel, q) ||
                Contains(t.ChildFriendliness, q) ||
                (t.TourLogs != null && t.TourLogs.Any(log => Contains(log.Comment, q)))
            ).ToList();
        }

        public async Task<IEnumerable<Tour>> ImportAsync(IEnumerable<Tour> tours)
        {
            _logger.LogInformation("Importing {Count} tours", tours.Count());
            var results = new List<Tour>();

            foreach (var tour in tours)
            {
                // Neue IDs vergeben damit es keine Kollisionen gibt
                tour.Id = Guid.NewGuid();
                tour.CreatedAt = DateTime.UtcNow;
                foreach (var log in tour.TourLogs ?? new List<TourLog>())
                {
                    log.Id = Guid.NewGuid();
                    log.TourId = tour.Id;
                }

                try
                {
                    var created = await _repo.CreateAsync(tour);
                    ComputeAttributes(created);
                    results.Add(created);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler beim Import von Tour '{Name}'", tour.Name);
                    throw new TourValidationException($"Import fehlgeschlagen bei Tour '{tour.Name}': {ex.Message}", ex);
                }
            }

            return results;
        }

        // ===== Berechnete Attribute =====

        /// <summary>
        /// Berechnet Popularity und Child-Friendliness on-the-fly aus den TourLogs.
        /// Wird bei jedem Zurückgeben einer Tour aufgerufen, damit die Werte
        /// immer aktuell sind wenn sich Logs ändern.
        /// </summary>
        internal static void ComputeAttributes(Tour tour)
        {
            ComputePopularity(tour);
            ComputeChildFriendliness(tour);
        }

        /// <summary>
        /// Popularity: abgeleitet aus der Anzahl der TourLogs.
        ///   0 Logs   -> "Unbeliebt"
        ///   1-3 Logs -> "Bekannt"
        ///   4+ Logs  -> "Beliebt"
        /// </summary>
        internal static void ComputePopularity(Tour tour)
        {
            var count = tour.TourLogs?.Count ?? 0;
            tour.PopularityCount = count;
            tour.PopularityLevel = count switch
            {
                0 => "Unbeliebt",
                <= 3 => "Bekannt",
                _ => "Beliebt"
            };
        }

        /// <summary>
        /// Child-Friendliness: Heuristik aus den Durchschnittswerten der TourLogs.
        ///
        /// Formel:
        ///   1. Normalisierung auf 0..1:
        ///      - difficultyNorm = (avgDifficulty - 1) / 4     (1=einfach -> 0, 5=schwer -> 1)
        ///      - timeNorm       = min(avgTotalTime / 4, 1)    (0h -> 0, 4h+ -> 1)
        ///      - distanceNorm   = min(avgTotalDistance / 30, 1)(0km -> 0, 30km+ -> 1)
        ///
        ///   2. Score = (difficultyNorm + timeNorm + distanceNorm) / 3   (0..1)
        ///
        ///   3. Schwellen:
        ///      - Score <= 0.33 -> "Kinderfreundlich"
        ///      - Score <= 0.66 -> "Bedingt geeignet"
        ///      - Score >  0.66 -> "Nicht kinderfreundlich"
        ///      - Keine Logs    -> "Unbekannt"
        /// </summary>
        internal static void ComputeChildFriendliness(Tour tour)
        {
            if (tour.TourLogs == null || tour.TourLogs.Count == 0)
            {
                tour.ChildFriendliness = "Unbekannt";
                return;
            }

            var logs = tour.TourLogs.ToList();
            var avgDifficulty = logs.Average(l => l.Difficulty);
            var avgTime = logs.Average(l => l.TotalTime);
            var avgDistance = logs.Average(l => l.TotalDistance);

            var difficultyNorm = (avgDifficulty - 1.0) / 4.0;
            var timeNorm = Math.Min(avgTime / 4.0, 1.0);
            var distanceNorm = Math.Min(avgDistance / 30.0, 1.0);

            var score = (difficultyNorm + timeNorm + distanceNorm) / 3.0;

            tour.ChildFriendliness = score switch
            {
                <= 0.33 => "Kinderfreundlich",
                <= 0.66 => "Bedingt geeignet",
                _ => "Nicht kinderfreundlich"
            };
        }

        // ===== Hilfsmethoden =====

        private static bool Contains(string? text, string query)
        {
            return !string.IsNullOrEmpty(text) &&
                   text.Contains(query, StringComparison.OrdinalIgnoreCase);
        }

        private async Task GenerateMapImageAsync(Tour tour)
        {
            if (string.IsNullOrEmpty(tour.RouteGeometry))
                return;

            var imageBytes = await _mapImage.GenerateMapImageAsync(tour.RouteGeometry);
            if (imageBytes == null)
            {
                _logger.LogWarning("Kartenbild konnte nicht generiert werden fuer Tour '{Name}'", tour.Name);
                return;
            }

            if (!Directory.Exists(_imageDirectory))
                Directory.CreateDirectory(_imageDirectory);

            var fileName = $"{tour.Id}.png";
            var filePath = Path.Combine(_imageDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, imageBytes);

            tour.RouteImage = $"/images/{fileName}";
            _logger.LogInformation("Kartenbild gespeichert: {Path}", filePath);
        }

        private async Task FillRouteDataAsync(Tour tour)
        {
            var fromCoords = await _ors.GeocodeAsync(tour.From);
            if (fromCoords == null)
            {
                _logger.LogWarning("Geocoding fuer '{From}' fehlgeschlagen, Route wird nicht berechnet", tour.From);
                return;
            }

            var toCoords = await _ors.GeocodeAsync(tour.To);
            if (toCoords == null)
            {
                _logger.LogWarning("Geocoding fuer '{To}' fehlgeschlagen, Route wird nicht berechnet", tour.To);
                return;
            }

            var route = await _ors.GetDirectionsAsync(
                fromCoords.Value.lon, fromCoords.Value.lat,
                toCoords.Value.lon, toCoords.Value.lat,
                tour.TransportType);

            if (route == null)
            {
                _logger.LogWarning("Directions-Abfrage fehlgeschlagen, Route wird nicht berechnet");
                return;
            }

            tour.Distance = route.DistanceKm;
            tour.EstimatedTime = Math.Round(route.DurationSeconds / 3600.0, 2);
            tour.RouteGeometry = route.GeometryGeoJson;

            _logger.LogInformation(
                "Route berechnet: {Distance} km, {Time} h",
                tour.Distance, tour.EstimatedTime);
        }
    }
}
