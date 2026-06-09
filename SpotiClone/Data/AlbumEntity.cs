using SQLite;

namespace SpotiClone.Data;

[Table("Album")]
public class AlbumEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string ArtistId { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}
