namespace Windeck.Geschichtstour.Backend.Dtos
{
    /// <summary>
    /// Datenübertragungsobjekt für eine Tour, inklusive der
    /// enthaltenen Stationen in der vorgesehenen Reihenfolge.
    /// </summary>
    public class TourDto
    {
        public int Id { get; set; }

        /// <summary>
        /// Titel der Tour, z. B. "Altstadttour Rosbach".
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Beschreibung der Tour (Thema, Dauer, Zielgruppe, ...).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Optionaler Link, z. B. fuer Komoot oder eine fertige Tour-Route.
        /// </summary>
        public string? TourLink { get; set; }

        /// <summary>
        /// Haltepunkte der Tour in der Reihenfolge, in der sie besucht werden sollen.
        /// </summary>
        public List<TourStopDto> Stops { get; set; } = new();
    }
}
