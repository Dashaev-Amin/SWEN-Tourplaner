using Microsoft.AspNetCore.Mvc;
using TourPlanner.BL;
using TourPlanner.Models;

namespace TourPlanner.API.Controllers
{
    [ApiController]
    [Route("api/tours/{tourId}/logs")]
    public class TourLogController : ControllerBase
    {
        private readonly ITourLogService _logService;
        private readonly ITourService _tourService;
        private readonly ILogger<TourLogController> _logger;

        public TourLogController(ITourLogService logService, ITourService tourService, ILogger<TourLogController> logger)
        {
            _logService = logService;
            _tourService = tourService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TourLog>>> GetAll(Guid tourId)
        {
            // check if tour exists
            var tour = await _tourService.GetByIdAsync(tourId);
            if (tour == null)
                return NotFound();

            var logs = await _logService.GetByTourIdAsync(tourId);
            return Ok(logs);
        }

        [HttpPost]
        public async Task<ActionResult<TourLog>> Create(Guid tourId, TourLog log)
        {
            var tour = await _tourService.GetByIdAsync(tourId);
            if (tour == null)
                return NotFound();

            log.TourId = tourId;
            try
            {
                var created = await _logService.CreateAsync(log);
                return CreatedAtAction(nameof(GetAll), new { tourId }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating log for tour {TourId}", tourId);
                return BadRequest();
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TourLog>> Update(Guid tourId, Guid id, TourLog log)
        {
            var existing = await _logService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            log.Id = id;
            log.TourId = tourId;
            try
            {
                var updated = await _logService.UpdateAsync(log);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating log {Id}", id);
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid tourId, Guid id)
        {
            var existing = await _logService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            await _logService.DeleteAsync(id);
            return NoContent();
        }
    }
}
