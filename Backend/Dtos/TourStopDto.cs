namespace Windeck.Geschichtstour.Backend.Dtos
{
    /// <summary>
    /// Repräsentiert einen Haltepunkt innerhalb einer Tour
    /// aus Sicht der API / mobilen App.
    /// </summary>
    public class TourStopDto
    {
        public int Order { get; set; }

        /// <summary>
        /// ID der Station, die zu diesem Stop gehört.
        /// </summary>
        public int StationId { get; set; }

        public string StationTitle { get; set; } = string.Empty;

        /// <summary>
        /// Koordinaten der Station. Werden benötigt,
        /// um z. B. eine Route in Google Maps aufzubauen.
        /// </summary>
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
