using System.Text.Json.Serialization;

namespace SpotiClone.Models;

public class SearchResultDto
{
    [JsonPropertyName("tracks")]
    public List<TrackDto> Tracks { get; set; } = new();

    [JsonPropertyName("artists")]
    public List<ArtistDto> Artists { get; set; } = new();

    [JsonPropertyName("albums")]
    public List<AlbumDto> Albums { get; set; } = new();

    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("total_results")]
    public int TotalResults { get; set; }
}
