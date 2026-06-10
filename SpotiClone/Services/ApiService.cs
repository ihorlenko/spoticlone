using System.Net.Http.Json;
using SpotiClone.Models;

namespace SpotiClone.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:8000");
    }

    public async Task<List<TrackDto>> GetTracksAsync(int limit = 20, int skip = 0, string? genre = null)
    {
        var url = $"/tracks?limit={limit}&skip={skip}";
        if (!string.IsNullOrEmpty(genre))
            url += $"&genre={Uri.EscapeDataString(genre)}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TrackDto>>() ?? new();
    }

    public async Task<TrackDto?> GetTrackAsync(string id)
    {
        var response = await _httpClient.GetAsync($"/tracks/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TrackDto>();
    }

    public async Task<List<ArtistDto>> GetArtistsAsync(int limit = 20, int skip = 0)
    {
        var response = await _httpClient.GetAsync($"/artists?limit={limit}&skip={skip}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ArtistDto>>() ?? new();
    }

    public async Task<ArtistDto?> GetArtistAsync(string id)
    {
        var response = await _httpClient.GetAsync($"/artists/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ArtistDto>();
    }

    public async Task<List<AlbumDto>> GetAlbumsAsync(int limit = 20, int skip = 0)
    {
        var response = await _httpClient.GetAsync($"/albums?limit={limit}&skip={skip}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<AlbumDto>>() ?? new();
    }

    public async Task<AlbumDto?> GetAlbumAsync(string id)
    {
        var response = await _httpClient.GetAsync($"/albums/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AlbumDto>();
    }

    public async Task<SearchResultDto> SearchAsync(string query, int limit = 10)
    {
        var encoded = Uri.EscapeDataString(query);
        var response = await _httpClient.GetAsync($"/search?q={encoded}&limit={limit}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SearchResultDto>() ?? new();
    }

    public async Task<string?> GetLyricsAsync(string trackId)
    {
        var response = await _httpClient.GetAsync($"/tracks/{trackId}/lyrics");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
