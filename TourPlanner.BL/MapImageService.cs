using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TourPlanner.BL
{
    /// <summary>
    /// Erzeugt statische Kartenbilder via staticmap.openstreetmap.de.
    /// Der Dienst unterstuetzt keine Polyline-Zeichnung, daher werden
    /// Start- und Endpunkt als Marker angezeigt. Die volle Route ist
    /// in der interaktiven Leaflet-Karte sichtbar.
    /// </summary>
    public class MapImageService : IMapImageService
    {
        private readonly HttpClient _http;
        private readonly ILogger<MapImageService> _logger;

        public MapImageService(HttpClient http, ILogger<MapImageService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<byte[]?> GenerateMapImageAsync(string routeGeometryGeoJson)
        {
            try
            {
                var coords = ParseCoordinates(routeGeometryGeoJson);
                if (coords.Count < 2)
                {
                    _logger.LogWarning("RouteGeometry hat weniger als 2 Koordinaten, kein Bild generiert");
                    return null;
                }

                var (startLat, startLon) = coords.First();
                var (endLat, endLon) = coords.Last();

                // Bounding-Box berechnen fuer Center + Zoom
                var minLat = coords.Min(c => c.lat);
                var maxLat = coords.Max(c => c.lat);
                var minLon = coords.Min(c => c.lon);
                var maxLon = coords.Max(c => c.lon);

                var centerLat = (minLat + maxLat) / 2.0;
                var centerLon = (minLon + maxLon) / 2.0;
                var zoom = CalculateZoom(maxLat - minLat, maxLon - minLon);

                // URL bauen: staticmap.openstreetmap.de unterstuetzt markers im Format lat,lon,icon
                var center = $"{Fmt(centerLat)},{Fmt(centerLon)}";
                var markers = $"{Fmt(startLat)},{Fmt(startLon)},ol-marker" +
                              $"|{Fmt(endLat)},{Fmt(endLon)},ol-marker";

                var url = $"https://staticmap.openstreetmap.de/staticmap.php" +
                          $"?center={center}&zoom={zoom}&size=600x400&markers={markers}";

                _logger.LogInformation("Static-Map-Request: {Url}", url);

                var response = await _http.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var bytes = await response.Content.ReadAsByteArrayAsync();

                // PNG-Magic-Bytes pruefen (89 50 4E 47)
                if (bytes.Length < 8 || bytes[0] != 0x89 || bytes[1] != 0x50 ||
                    bytes[2] != 0x4E || bytes[3] != 0x47)
                {
                    _logger.LogWarning("Static-Map-Antwort ist kein gueltiges PNG ({Length} Bytes)", bytes.Length);
                    return null;
                }

                _logger.LogInformation("Static-Map-Bild generiert: {Length} Bytes", bytes.Length);
                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Generieren des Kartenbilds");
                return null;
            }
        }

        /// <summary>
        /// Parst GeoJSON-LineString-Koordinaten [[lon,lat], ...] in (lat,lon)-Tupel.
        /// Duennt auf max. 40 Punkte aus (gleichmaessig), damit URLs nicht zu lang werden.
        /// </summary>
        private static List<(double lat, double lon)> ParseCoordinates(string geoJson)
        {
            using var doc = JsonDocument.Parse(geoJson);
            var coordsArray = doc.RootElement.GetProperty("coordinates");

            var all = coordsArray.EnumerateArray()
                .Select(c => (lat: c[1].GetDouble(), lon: c[0].GetDouble()))
                .ToList();

            if (all.Count <= 40)
                return all;

            // Gleichmaessig auf ~40 Punkte ausduennen, Start und Ende immer behalten
            var result = new List<(double lat, double lon)> { all.First() };
            var step = (double)(all.Count - 1) / 39;
            for (int i = 1; i < 39; i++)
            {
                var idx = (int)Math.Round(i * step);
                result.Add(all[idx]);
            }
            result.Add(all.Last());
            return result;
        }

        /// <summary>
        /// Berechnet ein sinnvolles Zoom-Level aus der Bounding-Box-Groesse.
        /// </summary>
        private static int CalculateZoom(double latDiff, double lonDiff)
        {
            var maxDiff = Math.Max(latDiff, lonDiff);
            if (maxDiff <= 0) return 12;
            var zoom = (int)Math.Floor(Math.Log2(360.0 / maxDiff)) - 1;
            return Math.Clamp(zoom, 2, 16);
        }

        private static string Fmt(double val) =>
            val.ToString("F6", CultureInfo.InvariantCulture);
    }
}
