using Microsoft.EntityFrameworkCore;
using TourPlanner.Models;

namespace TourPlanner.DAL
{
    public class TourRepository : ITourRepository
    {
        private readonly TourPlannerDbContext _ctx;

        public TourRepository(TourPlannerDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IEnumerable<Tour>> GetAllAsync()
        {
            return await _ctx.Tours.Include(t => t.TourLogs).ToListAsync();
        }

        public async Task<Tour?> GetByIdAsync(Guid id)
        {
            return await _ctx.Tours.Include(t => t.TourLogs)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Tour> CreateAsync(Tour tour)
        {
            _ctx.Tours.Add(tour);
            await _ctx.SaveChangesAsync();
            return tour;
        }

        public async Task<Tour> UpdateAsync(Tour tour)
        {
            _ctx.Tours.Update(tour);
            await _ctx.SaveChangesAsync();
            return tour;
        }

        public async Task DeleteAsync(Guid id)
        {
            var tour = await _ctx.Tours.FindAsync(id);
            if (tour != null)
            {
                _ctx.Tours.Remove(tour);
                await _ctx.SaveChangesAsync();
            }
        }
    }
}
