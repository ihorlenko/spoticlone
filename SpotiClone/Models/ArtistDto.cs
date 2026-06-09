using System.Text.Json.Serialization;

namespace SpotiClone.Models;

public class ArtistDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("cover_url")]
    public string CoverUrl { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("tracks")]
    public List<TrackDto>? Tracks { get; set; }

    [JsonPropertyName("albums")]
    public List<AlbumDto>? Albums { get; set; }
}
