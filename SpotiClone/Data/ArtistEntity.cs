using SQLite;

namespace SpotiClone.Data;

[Table("Artist")]
public class ArtistEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}
