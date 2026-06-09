using SQLite;

namespace SpotiClone.Data;

[Table("SearchHistory")]
public class SearchHistoryEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Query { get; set; } = string.Empty;
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    public int ResultsCount { get; set; }
}
