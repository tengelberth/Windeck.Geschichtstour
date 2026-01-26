using System.Collections.Generic;

namespace Windeck.Geschichtstour.Mobile.Models
{
    /// <summary>
    /// Datenübertragungsobjekt für eine Tour, inklusive der
    /// enthaltenen Stationen in der vorgesehenen Reihenfolge.
    /// </summary>
    public class TourDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public List<TourStopDto> Stops { get; set; } = new();
    }
}
