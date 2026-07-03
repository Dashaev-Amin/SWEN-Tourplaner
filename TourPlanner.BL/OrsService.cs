using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TourPlanner.Models;

namespace TourPlanner.BL
{
    public class OrsService : IOrsService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly ILogger<OrsService> _logger;

        public OrsService(string apiKey, ILogger<OrsService> logger)
        {
            _http = new HttpClient();
            _http.BaseAddress = new Uri("https://api.openrouteservice.org/");
            _apiKey = apiKey;
            _logger = logger;
        }

        public async Task<(double lon, double lat)?> GeocodeAsync(string place)
        {
            try
            {
                var url = $"geocode/search?api_key={_apiKey}&text={Uri.EscapeDataString(place)}&size=1";
                var response = await _http.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var features = doc.RootElement.GetProperty("features");

                if (features.GetArrayLength() == 0)
                {
                    _logger.LogWarning("Geocoding: Kein Ergebnis fuer '{Place}'", place);
                    return null;
                }

                var coords = features[0].GetProperty("geometry").GetProperty("coordinates");
                var lon = coords[0].GetDouble();
                var lat = coords[1].GetDouble();

                _logger.LogInformation("Geocoding: '{Place}' -> ({Lon}, {Lat})", place, lon, lat);
                return (lon, lat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geocoding fehlgeschlagen fuer '{Place}'", place);
                return null;
            }
        }

        public async Task<OrsRouteResult?> GetDirectionsAsync(
            double lon1, double lat1, double lon2, double lat2, TransportType transportType)
        {
            try
            {
                var profile = MapProfile(transportType);
                var url = $"v2/directions/{profile}/geojson?api_key={_apiKey}";

                var body = JsonSerializer.Serialize(new
                {
                    coordinates = new[] { new[] { lon1, lat1 }, new[] { lon2, lat2 } }
                });

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await _http.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var feature = doc.RootElement.GetProperty("features")[0];
                var summary = feature.GetProperty("properties").GetProperty("summary");
                var distance = summary.GetProperty("distance").GetDouble(); // Meter
                var duration = summary.GetProperty("duration").GetDouble(); // Sekunden

                // Geometry als GeoJSON-String speichern
                var geometry = feature.GetProperty("geometry");
                var geometryJson = geometry.GetRawText();

                _logger.LogInformation(
                    "Directions: {Profile}, {Distance:F0}m, {Duration:F0}s",
                    profile, distance, duration);

                return new OrsRouteResult
                {
                    DistanceKm = Math.Round(distance / 1000.0, 2),
                    DurationSeconds = duration,
                    GeometryGeoJson = geometryJson
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Directions-Abfrage fehlgeschlagen");
                return null;
            }
        }

        private static string MapProfile(TransportType type) => type switch
        {
            TransportType.Car => "driving-car",
            TransportType.Bike => "cycling-regular",
            TransportType.Hiking => "foot-hiking",
            TransportType.Running => "foot-walking",
            _ => "driving-car"
        };
    }
}
