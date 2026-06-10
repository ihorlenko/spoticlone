using Plugin.Maui.Audio;

namespace SpotiClone.Services;

public class PlayerService : IPlayerService
{
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _player;
    private string? _tempFilePath;

    // Plugin.Maui.Audio does not reliably report CurrentPosition on iOS/macOS when
    // the player is created from a Stream (AVAudioPlayer sees it as NSData, not a URL).
    // We track position ourselves using wall-clock time instead.
    private DateTime? _playbackStartedAt;
    private double _seekOffsetSeconds;

    public event EventHandler? PlaybackEnded;

    public PlayerService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public double CurrentPosition =>
        _playbackStartedAt.HasValue
            ? _seekOffsetSeconds + (DateTime.UtcNow - _playbackStartedAt.Value).TotalSeconds
            : _seekOffsetSeconds;

    public double Duration => _player?.Duration ?? 0;
    public bool IsPlaying => _player?.IsPlaying ?? false;

    public async Task PlayAsync(string audioUrl)
    {
        if (_player is not null)
        {
            _player.PlaybackEnded -= OnPlaybackEnded;
            _player.Stop();
            _player.Dispose();
            _player = null;
        }

        if (_tempFilePath is not null && File.Exists(_tempFilePath))
        {
            try { File.Delete(_tempFilePath); } catch { /* ignore cleanup errors */ }
        }

        var tempPath = Path.GetTempFileName() + ".mp3";
        _tempFilePath = tempPath;

        using var client = new HttpClient();
        var bytes = await client.GetByteArrayAsync(audioUrl);
        await File.WriteAllBytesAsync(tempPath, bytes);

        var stream = File.OpenRead(tempPath);
        _player = _audioManager.CreatePlayer(stream);
        _player.PlaybackEnded += OnPlaybackEnded;

        _seekOffsetSeconds = 0;
        _playbackStartedAt = DateTime.UtcNow;
        _player.Play();

        await Task.Delay(200);
    }

    public void Pause()
    {
        if (_playbackStartedAt.HasValue)
        {
            _seekOffsetSeconds += (DateTime.UtcNow - _playbackStartedAt.Value).TotalSeconds;
            _playbackStartedAt = null;
        }
        _player?.Pause();
    }

    public void Resume()
    {
        _playbackStartedAt = DateTime.UtcNow;
        _player?.Play();
    }

    public void Stop()
    {
        _seekOffsetSeconds = 0;
        _playbackStartedAt = null;
        _player?.Stop();
    }

    public void Seek(double positionSeconds)
    {
        _seekOffsetSeconds = positionSeconds;
        if (_playbackStartedAt.HasValue)
            _playbackStartedAt = DateTime.UtcNow;
        _player?.Seek(positionSeconds);
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[PlayerService] PlaybackEnded fired. Position={CurrentPosition:F2}s Duration={Duration:F2}s");
        _playbackStartedAt = null;
        PlaybackEnded?.Invoke(this, EventArgs.Empty);
    }
}
