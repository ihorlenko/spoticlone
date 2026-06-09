using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpotiClone.Data;
using SpotiClone.Models;
using SpotiClone.Services;

namespace SpotiClone.ViewModels;

public partial class TrackLikeItem : ObservableObject
{
    public TrackDto Track { get; init; } = null!;

    [ObservableProperty]
    private bool _isLiked;
}

public partial class HomeViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly IDatabaseService _dbService;
    private readonly PlayerViewModel _playerViewModel;

    [ObservableProperty]
    private ObservableCollection<TrackLikeItem> _recentTracks = new();

    [ObservableProperty]
    private ObservableCollection<AlbumDto> _featuredAlbums = new();

    [ObservableProperty]
    private ObservableCollection<ArtistDto> _artists = new();

    public HomeViewModel(IApiService apiService, IDatabaseService dbService, PlayerViewModel playerViewModel)
    {
        _apiService = apiService;
        _dbService = dbService;
        _playerViewModel = playerViewModel;
        Title = "Головна";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var tracks = await _apiService.GetTracksAsync(limit: 20);
            var albums = await _apiService.GetAlbumsAsync(limit: 20);
            var artists = await _apiService.GetArtistsAsync(limit: 20);
            var likedIds = (await _dbService.GetLikedTracksAsync())
                .Select(l => l.TrackId)
                .ToHashSet();

            RecentTracks = new ObservableCollection<TrackLikeItem>(
                tracks.Select(t => new TrackLikeItem
                {
                    Track = t,
                    IsLiked = likedIds.Contains(t.Id)
                })
            );
            FeaturedAlbums = new ObservableCollection<AlbumDto>(albums);
            Artists = new ObservableCollection<ArtistDto>(artists);

            await _dbService.CacheTracksAsync(tracks.Select(t => new TrackEntity
            {
                Id = t.Id, Title = t.Title, ArtistId = t.ArtistId, AlbumId = t.AlbumId,
                DurationMs = t.DurationMs, AudioUrl = t.AudioUrl, Genre = t.Genre,
                ArtistName = t.ArtistName ?? string.Empty,
                AlbumTitle = t.AlbumTitle ?? string.Empty,
                CoverUrl = t.CoverUrl ?? string.Empty,
                CachedAt = DateTime.UtcNow
            }));
            await _dbService.CacheAlbumsAsync(albums.Select(a => new AlbumEntity
            {
                Id = a.Id, Title = a.Title, ArtistId = a.ArtistId,
                ArtistName = a.ArtistName ?? string.Empty, CoverUrl = a.CoverUrl, Year = a.Year
            }));
            await _dbService.CacheArtistsAsync(artists.Select(a => new ArtistEntity
            {
                Id = a.Id, Name = a.Name, CoverUrl = a.CoverUrl
            }));
        }
        catch (Exception)
        {
            var cached = await _dbService.GetCachedTracksAsync();
            var cachedAlbums = await _dbService.GetCachedAlbumsAsync();
            var cachedArtists = await _dbService.GetCachedArtistsAsync();
            var likedIds = (await _dbService.GetLikedTracksAsync())
                .Select(l => l.TrackId)
                .ToHashSet();
            RecentTracks = new ObservableCollection<TrackLikeItem>(
                cached.Select(c => new TrackLikeItem
                {
                    Track = new TrackDto
                    {
                        Id = c.Id, Title = c.Title, ArtistId = c.ArtistId, AlbumId = c.AlbumId,
                        DurationMs = c.DurationMs, AudioUrl = c.AudioUrl, Genre = c.Genre,
                        ArtistName = c.ArtistName, AlbumTitle = c.AlbumTitle, CoverUrl = c.CoverUrl
                    },
                    IsLiked = likedIds.Contains(c.Id)
                })
            );
            FeaturedAlbums = new ObservableCollection<AlbumDto>(
                cachedAlbums.Select(a => new AlbumDto
                {
                    Id = a.Id, Title = a.Title, ArtistId = a.ArtistId,
                    ArtistName = a.ArtistName, CoverUrl = a.CoverUrl, Year = a.Year
                })
            );
            Artists = new ObservableCollection<ArtistDto>(
                cachedArtists.Select(a => new ArtistDto
                {
                    Id = a.Id, Name = a.Name, CoverUrl = a.CoverUrl
                })
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PlayTrackAsync(TrackLikeItem item)
    {
        if (item is null) return;
        var queue = RecentTracks.Select(t => t.Track).ToList();
        await _playerViewModel.PlayTrackAsync(item.Track, queue);
        await Shell.Current.GoToAsync("player", new Dictionary<string, object>
        {
            ["TrackId"] = item.Track.Id
        });
    }

    [RelayCommand]
    private async Task ToggleLikeAsync(TrackLikeItem item)
    {
        if (item is null) return;
        if (item.IsLiked)
        {
            await _dbService.RemoveLikedTrackAsync(item.Track.Id);
            item.IsLiked = false;
        }
        else
        {
            await _dbService.AddLikedTrackAsync(new LikedTrackEntity
            {
                TrackId = item.Track.Id,
                Title = item.Track.Title,
                ArtistName = item.Track.ArtistName ?? string.Empty,
                CoverUrl = item.Track.CoverUrl ?? string.Empty,
                AudioUrl = item.Track.AudioUrl,
                DurationMs = item.Track.DurationMs,
                LikedAt = DateTime.UtcNow
            });
            item.IsLiked = true;
        }
    }
}
