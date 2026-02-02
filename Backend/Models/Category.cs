using System.ComponentModel.DataAnnotations;


namespace Windeck.Geschichtstour.Backend.Models
{
    /// <summary>
    /// Kategorisiert Stationen, z. B. nach Thema ("Ortsgeschichte",
    /// "Industriegeschichte", "Bildung", ...).
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Eindeutige ID der Kategorie (Primärschlüssel).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Anzeigename der Kategorie.
        /// </summary>
        [Required(ErrorMessage = "Der Kategoriename ist erforderlich.")]
        [StringLength(100, ErrorMessage = "Der Kategoriename darf maximal {1} Zeichen lang sein.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optionale Beschreibung der Kategorie für das Admin-Windeck.Geschichtstour.Backend.
        /// </summary>
        [Required(ErrorMessage = "Der Kategoriename ist erforderlich.")]
        [StringLength(100, ErrorMessage = "Der Kategoriename darf maximal {1} Zeichen lang sein.")]
        public string? Description { get; set; }

        /// <summary>
        /// Alle Stationen, die dieser Kategorie zugeordnet sind.
        /// </summary>
        public List<Station> Stations { get; set; } = new();
    }
}
