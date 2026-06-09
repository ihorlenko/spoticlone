using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpotiClone.Data;
using SpotiClone.Models;
using SpotiClone.Services;

namespace SpotiClone.ViewModels;

public partial class PlayerViewModel : BaseViewModel
{
    private readonly IPlayerService _playerService;
    private readonly IDatabaseService _dbService;
    private readonly IApiService _apiService;

    [ObservableProperty] private TrackDto? _currentTrack;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _isLiked;
    [ObservableProperty] private double _currentPositionMs;
    [ObservableProperty] private double _durationMs = 1;
    [ObservableProperty] private string _currentPositionFormatted = "0:00";
    [ObservableProperty] private string _durationFormatted = "0:00";

    private List<TrackDto> _queue = [];
    private int _currentIndex;
    private IDispatcherTimer? _timer;
    private bool _isSeeking;

    public PlayerViewModel(IPlayerService playerService, IDatabaseService dbService, IApiService apiService)
    {
        _playerService = playerService;
        _dbService = dbService;
        _apiService = apiService;
        Title = "Плеєр";
    }

    public async Task PlayTrackAsync(TrackDto track, List<TrackDto>? queue = null)
    {
        if (CurrentTrack is not null && CurrentTrack.Id != track.Id)
            await SaveListeningHistoryAsync();

        if (queue is not null)
        {
            _queue = queue;
            _currentIndex = _queue.FindIndex(t => t.Id == track.Id);
            if (_currentIndex < 0) _currentIndex = 0;
        }
        else
        {
            _queue = [track];
            _currentIndex = 0;
        }

        CurrentTrack = track;
        IsLiked = await _dbService.IsLikedAsync(track.Id);

        try
        {
            await _playerService.PlayAsync(track.AudioUrl);
        }
        catch (Exception)
        {
            IsPlaying = false;
            await Shell.Current.DisplayAlertAsync(
                "Помилка відтворення",
                "Не вдалося завантажити аудіо. Перевірте підключення до сервера.",
                "OK");
            return;
        }

        IsPlaying = true;

        var dur = _playerService.Duration * 1000;
        DurationMs = dur > 0 ? dur : 1;
        DurationFormatted = FormatTime(dur);

        StartTimer();
    }

    // Called from PlayerPage when navigated with TrackId only (audio already playing)
    public async Task EnsureTrackLoadedAsync(string trackId)
    {
        if (CurrentTrack?.Id == trackId) return;

        try
        {
            var track = await _apiService.GetTrackAsync(trackId);
            if (track is null) return;
            CurrentTrack = track;
            IsLiked = await _dbService.IsLikedAsync(trackId);
            IsPlaying = _playerService.IsPlaying;
            var dur = _playerService.Duration * 1000;
            DurationMs = dur > 0 ? dur : 1;
            DurationFormatted = FormatTime(dur);
            StartTimer();
        }
        catch { /* ignore */ }
    }

    [RelayCommand]
    private void PlayPause()
    {
        if (IsPlaying)
        {
            _playerService.Pause();
            IsPlaying = false;
        }
        else
        {
            _playerService.Resume();
            IsPlaying = true;
        }
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        if (_queue.Count == 0) return;
        _currentIndex = (_currentIndex + 1) % _queue.Count;
        await PlayTrackAsync(_queue[_currentIndex]);
    }

    [RelayCommand]
    private async Task PreviousAsync()
    {
        if (_queue.Count == 0) return;
        if (_playerService.CurrentPosition > 3)
        {
            _playerService.Seek(0);
            CurrentPositionMs = 0;
            CurrentPositionFormatted = "0:00";
        }
        else
        {
            _currentIndex = (_currentIndex - 1 + _queue.Count) % _queue.Count;
            await PlayTrackAsync(_queue[_currentIndex]);
        }
    }

    [RelayCommand]
    private async Task ToggleLikeAsync()
    {
        if (CurrentTrack is null) return;
        if (IsLiked)
        {
            await _dbService.RemoveLikedTrackAsync(CurrentTrack.Id);
            IsLiked = false;
        }
        else
        {
            await _dbService.AddLikedTrackAsync(new LikedTrackEntity
            {
                TrackId = CurrentTrack.Id,
                Title = CurrentTrack.Title,
                ArtistName = CurrentTrack.ArtistName ?? string.Empty,
                CoverUrl = CurrentTrack.CoverUrl ?? string.Empty,
                AudioUrl = CurrentTrack.AudioUrl,
                DurationMs = CurrentTrack.DurationMs,
                LikedAt = DateTime.UtcNow
            });
            IsLiked = true;
        }
    }

    [RelayCommand]
    private static async Task CloseAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    public void BeginSeek() => _isSeeking = true;

    public void CompleteSeek(double positionMs)
    {
        _playerService.Seek(positionMs / 1000.0);
        CurrentPositionMs = positionMs;
        CurrentPositionFormatted = FormatTime(positionMs);
        _isSeeking = false;
    }

    private void StartTimer()
    {
        _timer?.Stop();
        _timer = Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_isSeeking) return;
        var posMs = _playerService.CurrentPosition * 1000;
        CurrentPositionMs = posMs;
        CurrentPositionFormatted = FormatTime(posMs);
        IsPlaying = _playerService.IsPlaying;

        if (_playerService.Duration > 0 && DurationMs <= 1)
        {
            var dur = _playerService.Duration * 1000;
            DurationMs = dur;
            DurationFormatted = FormatTime(dur);
        }
    }

    private async Task SaveListeningHistoryAsync()
    {
        if (CurrentTrack is null) return;
        await _dbService.AddListeningHistoryAsync(new ListeningHistoryEntity
        {
            TrackId = CurrentTrack.Id,
            TrackTitle = CurrentTrack.Title,
            ArtistName = CurrentTrack.ArtistName ?? string.Empty,
            CoverUrl = CurrentTrack.CoverUrl ?? string.Empty,
            ListenedAt = DateTime.UtcNow,
            ActualDurationMs = (int)CurrentPositionMs
        });
    }

    private static string FormatTime(double ms)
    {
        if (ms <= 0) return "0:00";
        var ts = TimeSpan.FromMilliseconds(ms);
        return ts.Hours > 0
            ? $"{ts.Hours}:{ts.Minutes:D2}:{ts.Seconds:D2}"
            : $"{ts.Minutes}:{ts.Seconds:D2}";
    }
}
