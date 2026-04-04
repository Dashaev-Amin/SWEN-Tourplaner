using TourPlanner.Models;

namespace TourPlanner.BL
{
    public interface ITourLogService
    {
        Task<IEnumerable<TourLog>> GetByTourIdAsync(Guid tourId);
        Task<TourLog?> GetByIdAsync(Guid id);
        Task<TourLog> CreateAsync(TourLog log);
        Task<TourLog> UpdateAsync(TourLog log);
        Task DeleteAsync(Guid id);
    }
}
