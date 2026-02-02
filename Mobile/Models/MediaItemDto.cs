namespace Windeck.Geschichtstour.Mobile.Models;

/// <summary>
/// Medienobjekt zu einer Station (Bild, Audio, Video).
/// </summary>
public class MediaItemDto
{
    public int Id { get; set; }

    public int StationId { get; set; }

    public string MediaType { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? Caption { get; set; }

    public int SortOrder { get; set; }

    /// <summary>
    /// Vollstaendige URL fuer die Medienanzeige in der UI.
    /// </summary>
    public string FullUrl { get; set; } = string.Empty;
}
