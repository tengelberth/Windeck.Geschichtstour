namespace Windeck.Geschichtstour.Mobile.Models
{
    /// <summary>
    /// Repräsentiert einen Haltepunkt innerhalb einer Tour
    /// aus Sicht der API / mobilen App.
    /// </summary>
    public class TourStopDto
    {
        public int Order { get; set; }
        public int StationId { get; set; }
        public string? StationTitle { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
