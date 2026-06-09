using SQLite;

namespace SpotiClone.Data;

[Table("Track")]
public class TrackEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string ArtistId { get; set; } = string.Empty;
    public string AlbumId { get; set; } = string.Empty;
    public int DurationMs { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;

    public string ArtistName { get; set; } = string.Empty;
    public string AlbumTitle { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;

    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}
