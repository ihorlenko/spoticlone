using SQLite;

namespace SpotiClone.Data;

[Table("ListeningHistory")]
public class ListeningHistoryEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string TrackId { get; set; } = string.Empty;

    public DateTime ListenedAt { get; set; } = DateTime.UtcNow;
    public int ActualDurationMs { get; set; }

    public string TrackTitle { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
}
