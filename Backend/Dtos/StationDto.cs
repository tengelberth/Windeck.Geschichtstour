using System.Collections.Generic;

namespace Windeck.Geschichtstour.Backend.Dtos
{
    /// <summary>
    /// Datenübertragungsobjekt für Stationen, das an die mobile App
    /// gesendet wird. Enthält nur die Felder, die die App wirklich benötigt.
    /// </summary>
    public class StationDto
    {
        public int Id { get; set; }

        /// <summary>
        /// Eindeutiger Code der Station, z. B. zur Verknüpfung mit einem QR-Code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Anzeigename der Station.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }

        /// <summary>
        /// Optional: Adresse, falls vorhanden.
        /// </summary>
        public string? Street { get; set; }
        public string? HouseNumber { get; set; }
        public string? ZipCode { get; set; }
        public string? City { get; set; }

        /// <summary>
        /// Geokoordinaten für Karte und Navigation.
        /// </summary>
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        /// <summary>
        /// Name der Kategorie (falls vorhanden).
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// Zugeordnete Medienobjekte (Bilder, Audio, Video).
        /// </summary>
        public List<MediaItemDto> MediaItems { get; set; } = new();
    }
}
