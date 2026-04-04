using Microsoft.EntityFrameworkCore;
using TourPlanner.Models;

namespace TourPlanner.DAL
{
    public class TourLogRepository : ITourLogRepository
    {
        private readonly TourPlannerDbContext _ctx;

        public TourLogRepository(TourPlannerDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IEnumerable<TourLog>> GetAllAsync()
        {
            return await _ctx.TourLogs.ToListAsync();
        }

        public async Task<TourLog?> GetByIdAsync(Guid id)
        {
            return await _ctx.TourLogs.FindAsync(id);
        }

        public async Task<IEnumerable<TourLog>> GetByTourIdAsync(Guid tourId)
        {
            return await _ctx.TourLogs
                .Where(l => l.TourId == tourId)
                .ToListAsync();
        }

        public async Task<TourLog> CreateAsync(TourLog log)
        {
            _ctx.TourLogs.Add(log);
            await _ctx.SaveChangesAsync();
            return log;
        }

        public async Task<TourLog> UpdateAsync(TourLog log)
        {
            _ctx.TourLogs.Update(log);
            await _ctx.SaveChangesAsync();
            return log;
        }

        public async Task DeleteAsync(Guid id)
        {
            var log = await _ctx.TourLogs.FindAsync(id);
            if (log != null)
            {
                _ctx.TourLogs.Remove(log);
                await _ctx.SaveChangesAsync();
            }
        }
    }
}
