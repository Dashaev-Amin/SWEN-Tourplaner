using Microsoft.AspNetCore.Mvc;
using TourPlanner.BL;
using TourPlanner.Models;

namespace TourPlanner.API.Controllers
{
    [ApiController]
    [Route("api/tours")]
    public class TourController : ControllerBase
    {
        private readonly ITourService _tourService;
        private readonly ILogger<TourController> _logger;

        public TourController(ITourService tourService, ILogger<TourController> logger)
        {
            _tourService = tourService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tour>>> GetAll()
        {
            var tours = await _tourService.GetAllAsync();
            return Ok(tours);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tour>> GetById(Guid id)
        {
            var tour = await _tourService.GetByIdAsync(id);
            if (tour == null)
                return NotFound();
            return Ok(tour);
        }

        [HttpPost]
        public async Task<ActionResult<Tour>> Create(Tour tour)
        {
            try
            {
                var created = await _tourService.CreateAsync(tour);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tour");
                return BadRequest();
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Tour>> Update(Guid id, Tour tour)
        {
            var existing = await _tourService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            tour.Id = id;
            try
            {
                var updated = await _tourService.UpdateAsync(tour);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tour {Id}", id);
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _tourService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            await _tourService.DeleteAsync(id);
            return NoContent();
        }
    }
}
