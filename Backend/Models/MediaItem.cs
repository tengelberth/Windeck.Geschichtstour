using System.ComponentModel.DataAnnotations;

namespace Windeck.Geschichtstour.Backend.Models
{
    /// <summary>
    /// Repräsentiert ein Medienobjekt (Bild, Audio, Video),
    /// das zu einer Station gehört.
    /// </summary>
    public class MediaItem
    {
        /// <summary>
        /// Eindeutige ID des Mediums (Primärschlüssel).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fremdschlüssel auf die zugehörige Station.
        /// </summary>
        [Required]
        public int StationId { get; set; }

        /// <summary>
        /// Navigationseigenschaft zur Station.
        /// </summary>
        public Station? Station { get; set; }

        /// <summary>
        /// Typ des Mediums, z. B. "Image", "Audio" oder "Video".
        /// </summary>
        [Required(ErrorMessage = "Der Medientyp ist erforderlich.")]
        [StringLength(20, ErrorMessage = "Der Medientyp darf maximal {1} Zeichen lang sein.")]

        public string MediaType { get; set; } = "Image";

        /// <summary>
        /// URL zur Datei im Webspeicher (z. B. /media/images/rathaus01.jpg
        /// oder eine vollständige URL).
        /// </summary>
        [Required(ErrorMessage = "Die Medien-URL ist erforderlich.")]
        [StringLength(500, ErrorMessage = "Die URL darf maximal {1} Zeichen lang sein.")]

        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Optionaler Untertitel/Beschriftung des Mediums.
        /// </summary>
        [Required(ErrorMessage = "Die Medien-URL ist erforderlich.")]
        [StringLength(500, ErrorMessage = "Die URL darf maximal {1} Zeichen lang sein.")]

        public string? Caption { get; set; }

        /// <summary>
        /// Sortierreihenfolge innerhalb der Medienliste.
        /// Ermöglicht z. B. die Steuerung der Anzeige-Reihenfolge in der App.
        /// </summary>
        public int SortOrder { get; set; }
    }
}
