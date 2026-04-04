using TourPlanner.Models;

namespace TourPlanner.BL
{
    public interface ITourService
    {
        Task<IEnumerable<Tour>> GetAllAsync();
        Task<Tour?> GetByIdAsync(Guid id);
        Task<Tour> CreateAsync(Tour tour);
        Task<Tour> UpdateAsync(Tour tour);
        Task DeleteAsync(Guid id);
    }
}
