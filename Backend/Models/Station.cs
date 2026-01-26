using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Windeck.Geschichtstour.Backend.Models
{
    /// <summary>
    /// Repräsentiert einen historischen Ort / eine Station in der Geschichtstour.
    /// Eine Station kann z. B. das Rathaus, ein Denkmal im Wald oder ein Aussichtspunkt sein.
    /// </summary>
    public class Station
    {
        /// <summary>
        /// Eindeutige technische ID der Station (Primärschlüssel in der Datenbank).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Eindeutiger Code, der mit einem QR-Code verknüpft werden kann.
        /// Beispiel: "RATHAUS_ROSBACH_01".
        /// </summary>
        [Required(ErrorMessage = "Der Code ist erforderlich.")]
        [StringLength(100, ErrorMessage = "Der Code darf maximal {1} Zeichen lang sein.")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Titel der Station, der in der App angezeigt wird.
        /// Beispiel: "Rathaus Rosbach".
        /// </summary>
        [Required(ErrorMessage = "Der Titel ist erforderlich.")]
        [StringLength(200, ErrorMessage = "Der Titel darf maximal {1} Zeichen lang sein.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Kurzer Teaser-Text (z. B. 1–3 Sätze) als Einstieg für Nutzer.
        /// </summary>
        [Required(ErrorMessage = "Die Kurzbeschreibung ist erforderlich.")]
        [StringLength(500, ErrorMessage = "Die Kurzbeschreibung darf maximal {1} Zeichen lang sein.")]
        public string ShortDescription { get; set; }

        /// <summary>
        /// Ausführlicher Beschreibungstext zur Station.
        /// </summary>
        [Required(ErrorMessage = "Der Text ist erforderlich.")]
        public string? LongDescription { get; set; }

        /// <summary>
        /// Optionale Straßenangabe. Kann leer bleiben, z. B. bei Denkmälern im Wald.
        /// </summary>
        public string? Street { get; set; }

        /// <summary>
        /// Optionale Hausnummer.
        /// </summary>
        public string? HouseNumber { get; set; }

        /// <summary>
        /// Optionale Postleitzahl.
        /// </summary>
        public string? ZipCode { get; set; }

        /// <summary>
        /// Optionale Ortsangabe, z. B. "Windeck-Rosbach".
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Geografische Breite der Station. Wird für Karte und Navigation verwendet.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Geografische Länge der Station. Wird für Karte und Navigation verwendet.
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Fremdschlüssel auf die Kategorie (z. B. "Ortsgeschichte", "Industrie").
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Navigationseigenschaft zur Kategorie.
        /// </summary>
        public Category? Category { get; set; }

        /// <summary>
        /// Alle Medien (Bilder, Audio, Video), die zu dieser Station gehören.
        /// </summary>
        public List<MediaItem> MediaItems { get; set; } = new();

        /// <summary>
        /// TourStops, in denen diese Station als Teil einer Tour vorkommt.
        /// Eine Station kann in mehreren Touren vorkommen.
        /// </summary>
        public List<TourStop> TourStops { get; set; } = new();
    }
}
