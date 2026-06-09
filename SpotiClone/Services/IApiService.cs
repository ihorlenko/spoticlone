using SpotiClone.Models;

namespace SpotiClone.Services;

public interface IApiService
{
    Task<List<TrackDto>> GetTracksAsync(int limit = 20, int skip = 0, string? genre = null);
    Task<TrackDto?> GetTrackAsync(string id);
    Task<List<ArtistDto>> GetArtistsAsync(int limit = 20, int skip = 0);
    Task<ArtistDto?> GetArtistAsync(string id);
    Task<List<AlbumDto>> GetAlbumsAsync(int limit = 20, int skip = 0);
    Task<AlbumDto?> GetAlbumAsync(string id);
    Task<SearchResultDto> SearchAsync(string query, int limit = 10);
}
