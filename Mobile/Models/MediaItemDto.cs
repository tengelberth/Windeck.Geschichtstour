namespace Windeck.Geschichtstour.Mobile.Models;

/// <summary>
/// Medienobjekt zu einer Station (Bild, Audio, Video).
/// </summary>
public class MediaItemDto
{
    public int Id { get; set; }

    public int StationId { get; set; }

    public string MediaType { get; set; } = string.Empty; // z. B. "Image", "Audio", "Video"

    public string Url { get; set; } = string.Empty;

    public string? Caption { get; set; }

    public int SortOrder { get; set; }

    // Für jetzt: dieselbe BaseUrl wie im ApiClient
    public string FullUrl => $"https://geschichtstour-backend.azurewebsites.net/{Url.TrimStart('/')}";
}
