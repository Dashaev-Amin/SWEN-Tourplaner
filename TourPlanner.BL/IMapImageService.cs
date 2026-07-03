namespace TourPlanner.BL
{
    public interface IMapImageService
    {
        Task<byte[]?> GenerateMapImageAsync(string routeGeometryGeoJson);
    }
}
