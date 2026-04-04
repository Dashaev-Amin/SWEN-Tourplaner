using System.ComponentModel.DataAnnotations;

namespace TourPlanner.Models
{
    public class Tour
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string From { get; set; } = string.Empty;

        [Required]
        public string To { get; set; } = string.Empty;

        [Required]
        public string TransportType { get; set; } = string.Empty;

        public double Distance { get; set; }

        public double EstimatedTime { get; set; }

        public string? RouteImage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TourLog> TourLogs { get; set; } = new List<TourLog>();
    }
}
