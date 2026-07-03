using TourPlanner.Models;

namespace TourPlanner.BL
{
    public class OrsRouteResult
    {
        public double DistanceKm { get; set; }
        public double DurationSeconds { get; set; }
        public string GeometryGeoJson { get; set; } = string.Empty;
    }

    public interface IOrsService
    {
        Task<(double lon, double lat)?> GeocodeAsync(string place);
        Task<OrsRouteResult?> GetDirectionsAsync(double lon1, double lat1, double lon2, double lat2, TransportType transportType);
    }
}
