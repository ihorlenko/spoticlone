# DATABASE.md — SQLite клієнтська база даних

## Огляд

SQLite використовується виключно на клієнті (MAUI) як локальне сховище для:
- Кешу контенту з бекенду (треки, артисти, альбоми)
- Персональних даних користувача (плейлисти, лайки, історія)

Бібліотека: **sqlite-net-pcl** (`SQLite.NET`) — стандарт для MAUI.

---

## NuGet пакети

```xml
<!-- SpotiClone.csproj -->
<PackageReference Include="sqlite-net-pcl" Version="1.9.172" />
<PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.8" />
```

---

## Схема бази даних

### Таблиця `Track` (кеш)
```csharp
[Table("Track")]
public class TrackEntity
{
    [PrimaryKey]
    public string Id { get; set; }          // MongoDB ObjectId як рядок

    public string Title { get; set; }
    public string ArtistId { get; set; }
    public string AlbumId { get; set; }
    public int DurationMs { get; set; }
    public string AudioUrl { get; set; }    // Повний URL для стримінгу
    public string Genre { get; set; }

    // Денормалізовані поля для швидкого відображення
    public string ArtistName { get; set; }
    public string AlbumTitle { get; set; }
    public string CoverUrl { get; set; }

    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}
```

### Таблиця `Artist` (кеш)
```csharp
[Table("Artist")]
public class ArtistEntity
{
    [PrimaryKey]
    public string Id { get; set; }

    public string Name { get; set; }
    public string CoverUrl { get; set; }
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}
```

### Таблиця `Album` (кеш)
```csharp
[Table("Album")]
public class AlbumEntity
{
    [PrimaryKey]
    public string Id { get; set; }

    public string Title { get; set; }
    public string ArtistId { get; set; }
    public string ArtistName { get; set; }  // Денормалізовано
    public string CoverUrl { get; set; }
    public int Year { get; set; }
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}
```

### Таблиця `Playlist`
```csharp
[Table("Playlist")]
public class PlaylistEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

### Таблиця `PlaylistTrack`
```csharp
[Table("PlaylistTrack")]
public class PlaylistTrackEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int PlaylistId { get; set; }

    public string TrackId { get; set; }     // FK → Track.Id
    public int Position { get; set; }       // Порядок треку в плейлисті
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
```

### Таблиця `LikedTrack`
```csharp
[Table("LikedTrack")]
public class LikedTrackEntity
{
    [PrimaryKey]
    public string TrackId { get; set; }     // FK → Track.Id

    // Денормалізовано для незалежності від кешу
    public string Title { get; set; }
    public string ArtistName { get; set; }
    public string CoverUrl { get; set; }
    public string AudioUrl { get; set; }
    public int DurationMs { get; set; }

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    public bool IsSynced { get; set; } = false;  // Поле з варіанту, для майбутньої синхронізації
}
```

### Таблиця `ListeningHistory`
```csharp
[Table("ListeningHistory")]
public class ListeningHistoryEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string TrackId { get; set; }     // FK → Track.Id

    public DateTime ListenedAt { get; set; } = DateTime.UtcNow;
    public int ActualDurationMs { get; set; }   // Скільки реально прослухано

    // Денормалізовано для відображення без join
    public string TrackTitle { get; set; }
    public string ArtistName { get; set; }
    public string CoverUrl { get; set; }
}
```

### Таблиця `SearchHistory`
```csharp
[Table("SearchHistory")]
public class SearchHistoryEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Query { get; set; }
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    public int ResultsCount { get; set; }
}
```

---

## DatabaseService

Клас `DatabaseService` — єдина точка доступу до SQLite. Реєструється як singleton у DI.

### Інтерфейс

```csharp
// Services/IDatabaseService.cs
public interface IDatabaseService
{
    // --- Cache ---
    Task CacheTracksAsync(IEnumerable<TrackEntity> tracks);
    Task CacheArtistsAsync(IEnumerable<ArtistEntity> artists);
    Task CacheAlbumsAsync(IEnumerable<AlbumEntity> albums);
    Task<List<TrackEntity>> GetCachedTracksAsync();

    // --- Liked Tracks ---
    Task<bool> IsLikedAsync(string trackId);
    Task AddLikedTrackAsync(LikedTrackEntity track);
    Task RemoveLikedTrackAsync(string trackId);
    Task<List<LikedTrackEntity>> GetLikedTracksAsync();

    // --- Playlists ---
    Task<List<PlaylistEntity>> GetPlaylistsAsync();
    Task<int> CreatePlaylistAsync(string name);
    Task DeletePlaylistAsync(int playlistId);
    Task RenamePlaylistAsync(int playlistId, string newName);
    Task AddTrackToPlaylistAsync(int playlistId, string trackId, string title, string artistName, string coverUrl, string audioUrl, int durationMs);
    Task RemoveTrackFromPlaylistAsync(int playlistId, string trackId);
    Task<List<PlaylistTrackEntity>> GetPlaylistTracksAsync(int playlistId);

    // --- Listening History ---
    Task AddListeningHistoryAsync(ListeningHistoryEntity entry);
    Task<List<ListeningHistoryEntity>> GetListeningHistoryAsync(int limit = 50);
    Task ClearListeningHistoryAsync();

    // --- Search History ---
    Task AddSearchHistoryAsync(string query, int resultsCount);
    Task<List<SearchHistoryEntity>> GetSearchHistoryAsync(int limit = 20);
    Task DeleteSearchHistoryItemAsync(int id);
    Task ClearSearchHistoryAsync();
}
```

### Реалізація (DatabaseService.cs)

```csharp
// Services/DatabaseService.cs
public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection _db;
    private bool _initialized = false;

    private async Task InitAsync()
    {
        if (_initialized) return;

        var dbPath = Path.Combine(
            FileSystem.AppDataDirectory,
            "spoticlone.db3"
        );

        _db = new SQLiteAsyncConnection(dbPath);

        await _db.CreateTableAsync<TrackEntity>();
        await _db.CreateTableAsync<ArtistEntity>();
        await _db.CreateTableAsync<AlbumEntity>();
        await _db.CreateTableAsync<PlaylistEntity>();
        await _db.CreateTableAsync<PlaylistTrackEntity>();
        await _db.CreateTableAsync<LikedTrackEntity>();
        await _db.CreateTableAsync<ListeningHistoryEntity>();
        await _db.CreateTableAsync<SearchHistoryEntity>();

        _initialized = true;
    }

    // Всі методи починаються з: await InitAsync();
}
```

### Реєстрація в DI (MauiProgram.cs)
```csharp
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
```

---

## Важливі нотатки щодо схеми

### Чому денормалізація в LikedTrack і ListeningHistory?
sqlite-net-pcl не підтримує JOIN запити нативно. Для відображення списку лайків або історії нам потрібні `title`, `artist_name`, `cover_url` — без join. Тому дублюємо ці поля. Це свідоме рішення для простоти.

### Кеш vs Персональні дані
- `Track`, `Artist`, `Album` — кеш, може бути очищений та перезавантажений з бекенду
- `Playlist`, `PlaylistTrack`, `LikedTrack`, `ListeningHistory`, `SearchHistory` — персональні дані, не видаляти при очищенні кешу

### IsSynced в LikedTrack
Поле залишене з варіанту завдання. У поточній реалізації завжди `false`. Зарезервовано для майбутньої синхронізації з сервером.

### PlaylistTrack і треки
`PlaylistTrackEntity` зберігає `TrackId` але також денормалізовані поля (`Title`, `ArtistName` тощо) для відображення — додай їх при реалізації методу `AddTrackToPlaylistAsync`.
