using System.Text.Json.Serialization;

namespace SpotiClone.Models;

public class AlbumDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("artist_id")]
    public string ArtistId { get; set; } = string.Empty;

    [JsonPropertyName("artist_name")]
    public string? ArtistName { get; set; }

    [JsonPropertyName("cover_url")]
    public string CoverUrl { get; set; } = string.Empty;

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("tracks")]
    public List<TrackDto>? Tracks { get; set; }
}
