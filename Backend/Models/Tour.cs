using System.ComponentModel.DataAnnotations;

namespace Windeck.Geschichtstour.Backend.Models
{
    /// <summary>
    /// Repräsentiert eine geführte Tour, die aus mehreren Stationen
    /// in einer bestimmten Reihenfolge besteht.
    /// </summary>
    public class Tour
    {
        /// <summary>
        /// Eindeutige ID der Tour (Primärschlüssel).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Titel der Tour, z. B. "Altstadttour Rosbach".
        /// </summary>
        [Required(ErrorMessage = "Der Titel der Tour ist erforderlich.")]
        [StringLength(200, ErrorMessage = "Der Tourtitel darf maximal {1} Zeichen lang sein.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Beschreibung der Tour (z. B. Dauer, Thema, Zielgruppe).
        /// </summary>
        [Required(ErrorMessage = "Die Tourbeschreibung ist erforderlich.")]
        public string? Description { get; set; }

        /// <summary>
        /// Alle Haltepunkte (Stops) der Tour in der vorgesehenen Reihenfolge.
        /// </summary>
        public List<TourStop> Stops { get; set; } = new();
    }
}
