using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpotiClone.Data;
using SpotiClone.Helpers;
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
    [ObservableProperty] private bool _hasLyrics;
    [ObservableProperty] private int _currentLyricIndex = -1;

    public ObservableCollection<LyricLine> LyricsLines { get; } = [];

    private List<TrackDto> _queue = [];
    private int _currentIndex;
    private IDispatcherTimer? _timer;
    private bool _isSeeking;
    public bool IsSeeking => _isSeeking;

    public PlayerViewModel(IPlayerService playerService, IDatabaseService dbService, IApiService apiService)
    {
        _playerService = playerService;
        _dbService = dbService;
        _apiService = apiService;
        Title = "Плеєр";
        _playerService.PlaybackEnded += OnPlaybackEnded;
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
        _ = LoadLyricsAsync(track.Id);

        await _dbService.AddListeningHistoryAsync(new ListeningHistoryEntity
        {
            TrackId = track.Id,
            TrackTitle = track.Title,
            ArtistName = track.ArtistName ?? string.Empty,
            CoverUrl = track.CoverUrl ?? string.Empty,
            ListenedAt = DateTime.UtcNow,
            ActualDurationMs = 0
        });

        // Reset position immediately so slider doesn't show stale value during download
        CurrentPositionMs = 0;
        CurrentPositionFormatted = "0:00";
        DurationMs = 1;
        DurationFormatted = "0:00";

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
        System.Diagnostics.Debug.WriteLine("[PlayerViewModel] Timer started");
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_isSeeking) return;

        var posMs = _playerService.CurrentPosition * 1000;
        System.Diagnostics.Debug.WriteLine($"[PlayerViewModel] Tick: pos={posMs:F0}ms dur={DurationMs:F0}ms isPlaying={IsPlaying} serviceIsPlaying={_playerService.IsPlaying}");
        CurrentPositionMs = posMs;
        CurrentPositionFormatted = FormatTime(posMs);

        // Update duration if it wasn't available when the track started
        if (_playerService.Duration > 0 && DurationMs <= 1)
        {
            var dur = _playerService.Duration * 1000;
            System.Diagnostics.Debug.WriteLine($"[PlayerViewModel] Duration updated: {dur:F0}ms");
            DurationMs = dur;
            DurationFormatted = FormatTime(dur);
        }

        UpdateCurrentLyric((int)posMs);

        // Fallback auto-advance: don't rely on IsPlaying from the audio library
        // (Plugin.Maui.Audio can return false even while audio is playing)
        if (IsPlaying && DurationMs > 1 && posMs >= DurationMs - 500)
        {
            System.Diagnostics.Debug.WriteLine($"[PlayerViewModel] Auto-advance triggered at pos={posMs:F0}ms dur={DurationMs:F0}ms");
            _timer?.Stop();
            _ = NextAsync();
        }
    }

    private async void OnPlaybackEnded(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[PlayerViewModel] OnPlaybackEnded. CurrentPositionMs={CurrentPositionMs:F0} DurationMs={DurationMs:F0}");
        _timer?.Stop();
        await NextAsync();
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

    private void UpdateCurrentLyric(int posMs)
    {
        if (!HasLyrics || LyricsLines.Count == 0) return;

        var newIndex = -1;
        for (var i = LyricsLines.Count - 1; i >= 0; i--)
        {
            if (LyricsLines[i].TimeMs <= posMs)
            {
                newIndex = i;
                break;
            }
        }

        if (newIndex == CurrentLyricIndex) return;

        if (CurrentLyricIndex >= 0 && CurrentLyricIndex < LyricsLines.Count)
            LyricsLines[CurrentLyricIndex].IsActive = false;
        if (newIndex >= 0 && newIndex < LyricsLines.Count)
            LyricsLines[newIndex].IsActive = true;

        CurrentLyricIndex = newIndex;
    }

    private async Task LoadLyricsAsync(string trackId)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            LyricsLines.Clear();
            HasLyrics = false;
            CurrentLyricIndex = -1;
        });

        try
        {
            var lrc = await _apiService.GetLyricsAsync(trackId);
            if (lrc is null) return;

            var parsed = LrcParser.Parse(lrc);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var line in parsed)
                    LyricsLines.Add(line);
                HasLyrics = parsed.Count > 0;
            });
        }
        catch { /* lyrics are optional */ }
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
