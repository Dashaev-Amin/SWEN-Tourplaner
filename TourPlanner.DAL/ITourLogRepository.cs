using TourPlanner.Models;

namespace TourPlanner.DAL
{
    public interface ITourLogRepository
    {
        Task<IEnumerable<TourLog>> GetAllAsync();
        Task<TourLog?> GetByIdAsync(Guid id);
        Task<IEnumerable<TourLog>> GetByTourIdAsync(Guid tourId);
        Task<TourLog> CreateAsync(TourLog log);
        Task<TourLog> UpdateAsync(TourLog log);
        Task DeleteAsync(Guid id);
    }
}
