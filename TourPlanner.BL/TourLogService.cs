using Microsoft.Extensions.Logging;
using TourPlanner.DAL;
using TourPlanner.Models;

namespace TourPlanner.BL
{
    public class TourLogService : ITourLogService
    {
        private readonly ITourLogRepository _repo;
        private readonly ILogger<TourLogService> _logger;

        public TourLogService(ITourLogRepository repo, ILogger<TourLogService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<IEnumerable<TourLog>> GetByTourIdAsync(Guid tourId)
        {
            return await _repo.GetByTourIdAsync(tourId);
        }

        public async Task<TourLog?> GetByIdAsync(Guid id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<TourLog> CreateAsync(TourLog log)
        {
            _logger.LogInformation("Creating log for tour {TourId}", log.TourId);
            return await _repo.CreateAsync(log);
        }

        public async Task<TourLog> UpdateAsync(TourLog log)
        {
            return await _repo.UpdateAsync(log);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
