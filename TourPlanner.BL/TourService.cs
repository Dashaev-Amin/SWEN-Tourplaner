using Microsoft.Extensions.Logging;
using TourPlanner.DAL;
using TourPlanner.Models;

namespace TourPlanner.BL
{
    public class TourService : ITourService
    {
        private readonly ITourRepository _repo;
        private readonly ILogger<TourService> _logger;

        public TourService(ITourRepository repo, ILogger<TourService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<IEnumerable<Tour>> GetAllAsync()
        {
            _logger.LogInformation("Getting all tours");
            return await _repo.GetAllAsync();
        }

        public async Task<Tour?> GetByIdAsync(Guid id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<Tour> CreateAsync(Tour tour)
        {
            _logger.LogInformation("Creating tour: {Name}", tour.Name);
            return await _repo.CreateAsync(tour);
        }

        public async Task<Tour> UpdateAsync(Tour tour)
        {
            return await _repo.UpdateAsync(tour);
        }

        public async Task DeleteAsync(Guid id)
        {
            _logger.LogInformation("Deleting tour {Id}", id);
            await _repo.DeleteAsync(id);
        }
    }
}
