using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourPlanner.Models
{
    public class Tour
    {
        // Berechnete Attribute (nicht in DB gespeichert, on-the-fly im BL berechnet)
        [NotMapped]
        public int PopularityCount { get; set; }

        [NotMapped]
        public string PopularityLevel { get; set; } = "Unbeliebt";

        [NotMapped]
        public string ChildFriendliness { get; set; } = "Unbekannt";

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
        public TransportType TransportType { get; set; } = TransportType.Car;

        public double Distance { get; set; }

        public double EstimatedTime { get; set; }

        public string? RouteImage { get; set; }

        public string? RouteGeometry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TourLog> TourLogs { get; set; } = new List<TourLog>();
    }
}
