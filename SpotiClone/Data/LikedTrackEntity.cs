using SQLite;

namespace SpotiClone.Data;

[Table("LikedTrack")]
public class LikedTrackEntity
{
    [PrimaryKey]
    public string TrackId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public int DurationMs { get; set; }

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = false;
}
