using System.Text.Json.Serialization;
using Windeck.Geschichtstour.Mobile.Models;

namespace Windeck.Geschichtstour.Mobile.Services;

/// <summary>
/// Definiert Source-Generator-Metadaten fuer die JSON-Serialisierung der API-Daten.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
)]
[JsonSerializable(typeof(StationDto))]
[JsonSerializable(typeof(List<StationDto>))]
[JsonSerializable(typeof(MediaItemDto))]
[JsonSerializable(typeof(List<MediaItemDto>))]

[JsonSerializable(typeof(CategoryDto))]
[JsonSerializable(typeof(List<CategoryDto>))]

[JsonSerializable(typeof(TourDto))]
[JsonSerializable(typeof(TourStopDto))]
[JsonSerializable(typeof(List<TourDto>))]
[JsonSerializable(typeof(List<TourStopDto>))]
public partial class ApiJsonContext : JsonSerializerContext
{
}


