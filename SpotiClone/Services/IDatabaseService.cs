using SpotiClone.Data;

namespace SpotiClone.Services;

public interface IDatabaseService
{
    // --- Cache ---
    Task CacheTracksAsync(IEnumerable<TrackEntity> tracks);
    Task CacheArtistsAsync(IEnumerable<ArtistEntity> artists);
    Task CacheAlbumsAsync(IEnumerable<AlbumEntity> albums);
    Task<List<TrackEntity>> GetCachedTracksAsync();
    Task<List<AlbumEntity>> GetCachedAlbumsAsync();
    Task<List<ArtistEntity>> GetCachedArtistsAsync();

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
