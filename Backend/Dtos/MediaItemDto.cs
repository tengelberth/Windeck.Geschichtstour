namespace Windeck.Geschichtstour.Backend.Dtos
{
    /// <summary>
    /// Datenübertragungsobjekt für Medien (Bild, Audio, Video),
    /// das von der API an die mobile App gesendet wird.
    /// </summary>
    public class MediaItemDto
    {
        public int Id { get; set; }

        /// <summary>
        /// Typ des Mediums, z. B. "Image", "Audio" oder "Video".
        /// </summary>
        public string MediaType { get; set; } = "Image";

        /// <summary>
        /// Öffentliche URL, unter der das Medium abgerufen werden kann.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Optionaler Untertitel/Beschriftung.
        /// </summary>
        public string? Caption { get; set; }

        /// <summary>
        /// Reihenfolge der Anzeige (z. B. für Bildgalerie).
        /// </summary>
        public int SortOrder { get; set; }
    }
}
