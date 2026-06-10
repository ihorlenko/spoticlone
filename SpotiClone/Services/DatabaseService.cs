using SQLite;
using SpotiClone.Data;

namespace SpotiClone.Services;

public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection _db = null!;
    private bool _initialized = false;

    private async Task InitAsync()
    {
        if (_initialized) return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "spoticlone.db3");
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

    // --- Cache ---

    public async Task CacheTracksAsync(IEnumerable<TrackEntity> tracks)
    {
        await InitAsync();
        var list = tracks.ToList();
        await _db.RunInTransactionAsync(conn => {
            foreach (var item in list) conn.InsertOrReplace(item);
        });
    }

    public async Task CacheArtistsAsync(IEnumerable<ArtistEntity> artists)
    {
        await InitAsync();
        var list = artists.ToList();
        await _db.RunInTransactionAsync(conn => {
            foreach (var item in list) conn.InsertOrReplace(item);
        });
    }

    public async Task CacheAlbumsAsync(IEnumerable<AlbumEntity> albums)
    {
        await InitAsync();
        var list = albums.ToList();
        await _db.RunInTransactionAsync(conn => {
            foreach (var item in list) conn.InsertOrReplace(item);
        });
    }

    public async Task<List<TrackEntity>> GetCachedTracksAsync()
    {
        await InitAsync();
        return await _db.Table<TrackEntity>().ToListAsync();
    }

    public async Task<List<AlbumEntity>> GetCachedAlbumsAsync()
    {
        await InitAsync();
        return await _db.Table<AlbumEntity>().ToListAsync();
    }

    public async Task<List<ArtistEntity>> GetCachedArtistsAsync()
    {
        await InitAsync();
        return await _db.Table<ArtistEntity>().ToListAsync();
    }

    // --- Liked Tracks ---

    public async Task<bool> IsLikedAsync(string trackId)
    {
        await InitAsync();
        var count = await _db.Table<LikedTrackEntity>()
            .Where(t => t.TrackId == trackId)
            .CountAsync();
        return count > 0;
    }

    public async Task AddLikedTrackAsync(LikedTrackEntity track)
    {
        await InitAsync();
        await _db.InsertOrReplaceAsync(track);
    }

    public async Task RemoveLikedTrackAsync(string trackId)
    {
        await InitAsync();
        await _db.DeleteAsync<LikedTrackEntity>(trackId);
    }

    public async Task<List<LikedTrackEntity>> GetLikedTracksAsync()
    {
        await InitAsync();
        return await _db.Table<LikedTrackEntity>()
            .OrderByDescending(t => t.LikedAt)
            .ToListAsync();
    }

    // --- Playlists ---

    public async Task<List<PlaylistEntity>> GetPlaylistsAsync()
    {
        await InitAsync();
        return await _db.Table<PlaylistEntity>()
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();
    }

    public async Task<int> CreatePlaylistAsync(string name)
    {
        await InitAsync();
        var playlist = new PlaylistEntity { Name = name };
        await _db.InsertAsync(playlist);
        return playlist.Id;
    }

    public async Task DeletePlaylistAsync(int playlistId)
    {
        await InitAsync();
        await _db.DeleteAsync<PlaylistEntity>(playlistId);
        await _db.Table<PlaylistTrackEntity>()
            .Where(pt => pt.PlaylistId == playlistId)
            .DeleteAsync();
    }

    public async Task RenamePlaylistAsync(int playlistId, string newName)
    {
        await InitAsync();
        var playlist = await _db.GetAsync<PlaylistEntity>(playlistId);
        playlist.Name = newName;
        playlist.UpdatedAt = DateTime.UtcNow;
        await _db.UpdateAsync(playlist);
    }

    public async Task AddTrackToPlaylistAsync(int playlistId, string trackId, string title, string artistName, string coverUrl, string audioUrl, int durationMs)
    {
        await InitAsync();
        var maxPosition = await _db.Table<PlaylistTrackEntity>()
            .Where(pt => pt.PlaylistId == playlistId)
            .CountAsync();

        var entry = new PlaylistTrackEntity
        {
            PlaylistId = playlistId,
            TrackId = trackId,
            Position = maxPosition,
            Title = title,
            ArtistName = artistName,
            CoverUrl = coverUrl,
            AudioUrl = audioUrl,
            DurationMs = durationMs
        };
        await _db.InsertAsync(entry);

        var playlist = await _db.GetAsync<PlaylistEntity>(playlistId);
        playlist.UpdatedAt = DateTime.UtcNow;
        await _db.UpdateAsync(playlist);
    }

    public async Task RemoveTrackFromPlaylistAsync(int playlistId, string trackId)
    {
        await InitAsync();
        await _db.Table<PlaylistTrackEntity>()
            .Where(pt => pt.PlaylistId == playlistId && pt.TrackId == trackId)
            .DeleteAsync();
    }

    public async Task<List<PlaylistTrackEntity>> GetPlaylistTracksAsync(int playlistId)
    {
        await InitAsync();
        return await _db.Table<PlaylistTrackEntity>()
            .Where(pt => pt.PlaylistId == playlistId)
            .OrderBy(pt => pt.Position)
            .ToListAsync();
    }

    // --- Listening History ---

    public async Task AddListeningHistoryAsync(ListeningHistoryEntity entry)
    {
        await InitAsync();
        await _db.Table<ListeningHistoryEntity>()
            .Where(h => h.TrackId == entry.TrackId)
            .DeleteAsync();
        await _db.InsertAsync(entry);
    }

    public async Task<List<ListeningHistoryEntity>> GetListeningHistoryAsync(int limit = 50)
    {
        await InitAsync();
        return await _db.Table<ListeningHistoryEntity>()
            .OrderByDescending(h => h.ListenedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task ClearListeningHistoryAsync()
    {
        await InitAsync();
        await _db.DeleteAllAsync<ListeningHistoryEntity>();
    }

    // --- Search History ---

    public async Task AddSearchHistoryAsync(string query, int resultsCount)
    {
        await InitAsync();
        // Remove duplicate if exists
        await _db.Table<SearchHistoryEntity>()
            .Where(s => s.Query == query)
            .DeleteAsync();

        await _db.InsertAsync(new SearchHistoryEntity
        {
            Query = query,
            ResultsCount = resultsCount
        });
    }

    public async Task<List<SearchHistoryEntity>> GetSearchHistoryAsync(int limit = 20)
    {
        await InitAsync();
        return await _db.Table<SearchHistoryEntity>()
            .OrderByDescending(s => s.SearchedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task DeleteSearchHistoryItemAsync(int id)
    {
        await InitAsync();
        await _db.DeleteAsync<SearchHistoryEntity>(id);
    }

    public async Task ClearSearchHistoryAsync()
    {
        await InitAsync();
        await _db.DeleteAllAsync<SearchHistoryEntity>();
    }
}
