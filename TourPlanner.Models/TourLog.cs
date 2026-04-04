using System.ComponentModel.DataAnnotations;

namespace TourPlanner.Models
{
    public class TourLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TourId { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        public string? Comment { get; set; }

        [Required]
        [Range(1, 5)]
        public int Difficulty { get; set; }

        [Required]
        public double TotalDistance { get; set; }

        [Required]
        public double TotalTime { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public Tour Tour { get; set; } = null!;
    }
}
