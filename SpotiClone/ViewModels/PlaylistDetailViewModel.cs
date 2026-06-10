using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpotiClone.Data;
using SpotiClone.Models;
using SpotiClone.Services;

namespace SpotiClone.ViewModels;

public partial class PlaylistDetailViewModel : BaseViewModel
{
    private readonly IDatabaseService _dbService;
    private readonly PlayerViewModel _playerViewModel;
    private int _playlistId;

    [ObservableProperty]
    private PlaylistEntity _playlist = new();

    [ObservableProperty]
    private ObservableCollection<PlaylistTrackEntity> _tracks = new();

    [ObservableProperty]
    private bool _hasNoTracks;

    public PlaylistDetailViewModel(IDatabaseService dbService, PlayerViewModel playerViewModel)
    {
        _dbService = dbService;
        _playerViewModel = playerViewModel;
    }

    public async Task InitAsync(int playlistId, string playlistName)
    {
        _playlistId = playlistId;
        Playlist = new PlaylistEntity { Id = playlistId, Name = playlistName };
        Title = playlistName;
        await LoadTracksAsync();
    }

    [RelayCommand]
    private async Task LoadTracksAsync()
    {
        var items = await _dbService.GetPlaylistTracksAsync(_playlistId);
        Tracks = new ObservableCollection<PlaylistTrackEntity>(items);
        HasNoTracks = Tracks.Count == 0;
    }

    [RelayCommand]
    private async Task PlayTrackAsync(PlaylistTrackEntity item)
    {
        if (item is null) return;
        var queue = Tracks.Select(t => new TrackDto
        {
            Id = t.TrackId, Title = t.Title, ArtistName = t.ArtistName,
            CoverUrl = t.CoverUrl, AudioUrl = t.AudioUrl, DurationMs = t.DurationMs
        }).ToList();
        var dto = queue.FirstOrDefault(t => t.Id == item.TrackId);
        if (dto is null) return;
        await _playerViewModel.PlayTrackAsync(dto, queue);
        await Shell.Current.GoToAsync("player",
            new Dictionary<string, object> { ["TrackId"] = item.TrackId });
    }

    [RelayCommand]
    private async Task RemoveTrackAsync(PlaylistTrackEntity item)
    {
        if (item is null) return;
        await _dbService.RemoveTrackFromPlaylistAsync(_playlistId, item.TrackId);
        Tracks.Remove(item);
        HasNoTracks = Tracks.Count == 0;
    }
}
