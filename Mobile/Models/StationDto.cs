using System.Collections.Generic;

namespace Windeck.Geschichtstour.Mobile.Models;

/// <summary>
/// Station, wie sie von der Web-API geliefert wird.
/// </summary>
public class StationDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string ShortDescription { get; set; } = string.Empty;

    public string LongDescription { get; set; } = string.Empty;

    public string? Street { get; set; }

    public string? HouseNumber { get; set; }

    public string? ZipCode { get; set; }

    public string? City { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int? CategoryId { get; set; }

    public CategoryDto? Category { get; set; }

    public List<MediaItemDto> MediaItems { get; set; } = new();
}
