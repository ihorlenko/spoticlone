using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpotiClone.Data;
using SpotiClone.Helpers;
using SpotiClone.Models;
using SpotiClone.Services;

namespace SpotiClone.ViewModels;

public partial class LibraryViewModel : BaseViewModel
{
    private readonly IDatabaseService _dbService;
    private readonly PlayerViewModel _playerViewModel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowTracks))]
    [NotifyPropertyChangedFor(nameof(ShowPlaylists))]
    private int _selectedTabIndex;

    [ObservableProperty]
    private ObservableCollection<LikedTrackEntity> _likedTracks = new();

    [ObservableProperty]
    private ObservableCollection<PlaylistEntity> _playlists = new();

    public bool ShowTracks => SelectedTabIndex == 0;
    public bool ShowPlaylists => SelectedTabIndex == 1;
    public bool HasNoLikedTracks => LikedTracks.Count == 0;
    public bool HasNoPlaylists => Playlists.Count == 0;

    public LibraryViewModel(IDatabaseService dbService, PlayerViewModel playerViewModel)
    {
        _dbService = dbService;
        _playerViewModel = playerViewModel;
        Title = "Бібліотека";
    }

    [RelayCommand]
    private void SelectTracksTab() => SelectedTabIndex = 0;

    [RelayCommand]
    private void SelectPlaylistsTab() => SelectedTabIndex = 1;

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var liked = await _dbService.GetLikedTracksAsync();
            LikedTracks = new ObservableCollection<LikedTrackEntity>(liked);

            var playlists = await _dbService.GetPlaylistsAsync();
            Playlists = new ObservableCollection<PlaylistEntity>(playlists);

            OnPropertyChanged(nameof(HasNoLikedTracks));
            OnPropertyChanged(nameof(HasNoPlaylists));
        }
        catch (Exception) { }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task CreatePlaylistAsync()
    {
        var name = await Shell.Current.DisplayPromptAsync(
            "Новий плейлист",
            "Введіть назву плейлиста:",
            "Створити",
            "Скасувати",
            placeholder: "Назва плейлиста",
            maxLength: 100);

        if (string.IsNullOrWhiteSpace(name)) return;

        var id = await _dbService.CreatePlaylistAsync(name.Trim());
        Playlists.Add(new PlaylistEntity
        {
            Id = id,
            Name = name.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        OnPropertyChanged(nameof(HasNoPlaylists));
    }

    [RelayCommand]
    private async Task DeletePlaylistAsync(PlaylistEntity playlist)
    {
        if (playlist is null) return;

        bool confirmed = await Shell.Current.DisplayAlertAsync(
            "Видалити плейлист",
            $"Видалити «{playlist.Name}»?",
            "Видалити",
            "Скасувати");

        if (!confirmed) return;

        await _dbService.DeletePlaylistAsync(playlist.Id);
        Playlists.Remove(playlist);
        OnPropertyChanged(nameof(HasNoPlaylists));
    }

    [RelayCommand]
    private async Task PlayLikedTrackAsync(LikedTrackEntity track)
    {
        if (track is null) return;

        var queue = LikedTracks.Select(t => new TrackDto
        {
            Id = t.TrackId,
            Title = t.Title,
            ArtistName = t.ArtistName,
            CoverUrl = t.CoverUrl,
            AudioUrl = t.AudioUrl,
            DurationMs = t.DurationMs
        }).ToList();

        var trackDto = queue.FirstOrDefault(t => t.Id == track.TrackId);
        if (trackDto is null) return;

        await _playerViewModel.PlayTrackAsync(trackDto, queue);
        await Shell.Current.GoToAsync("player", new Dictionary<string, object>
        {
            ["TrackId"] = track.TrackId
        });
    }

    [RelayCommand]
    private async Task OpenPlaylistAsync(PlaylistEntity playlist)
    {
        if (playlist is null) return;
        await Shell.Current.GoToAsync("playlist-detail",
            new Dictionary<string, object>
            {
                ["PlaylistId"] = playlist.Id.ToString(),
                ["PlaylistName"] = playlist.Name
            });
    }

    [RelayCommand]
    private async Task AddToPlaylistAsync(LikedTrackEntity track)
    {
        if (track is null) return;
        var dto = new TrackDto
        {
            Id = track.TrackId, Title = track.Title, ArtistName = track.ArtistName,
            CoverUrl = track.CoverUrl, AudioUrl = track.AudioUrl, DurationMs = track.DurationMs
        };
        await PlaylistHelper.AddTrackToPlaylistAsync(dto, _dbService);
    }

    [RelayCommand]
    private async Task UnlikeTrackAsync(LikedTrackEntity track)
    {
        if (track is null) return;
        await _dbService.RemoveLikedTrackAsync(track.TrackId);
        LikedTracks.Remove(track);
        OnPropertyChanged(nameof(HasNoLikedTracks));
    }
}
