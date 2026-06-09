using SQLite;

namespace SpotiClone.Data;

[Table("PlaylistTrack")]
public class PlaylistTrackEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int PlaylistId { get; set; }

    public string TrackId { get; set; } = string.Empty;
    public int Position { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Denormalized for display without joins
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public int DurationMs { get; set; }
}
