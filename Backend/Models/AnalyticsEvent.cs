using System.ComponentModel.DataAnnotations;

namespace Windeck.Geschichtstour.Backend.Models
{
    /// <summary>
    /// Speichert ein einzelnes Analytics-Ereignis fuer API-Nutzung und Inhaltsaufrufe.
    /// </summary>
    public class AnalyticsEvent
    {
        public long Id { get; set; }

        [Required]
        [StringLength(40)]
        public string EventType { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Endpoint { get; set; } = string.Empty;

        public int? StationId { get; set; }

        [StringLength(100)]
        public string? StationCode { get; set; }

        [StringLength(200)]
        public string? StationTitle { get; set; }

        public int? TourId { get; set; }

        public int StatusCode { get; set; }

        [StringLength(300)]
        public string? UserAgent { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
