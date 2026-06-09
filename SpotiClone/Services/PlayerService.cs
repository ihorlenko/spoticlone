using Plugin.Maui.Audio;

namespace SpotiClone.Services;

public class PlayerService : IPlayerService
{
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _player;

    public PlayerService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public double CurrentPosition => _player?.CurrentPosition ?? 0;
    public double Duration => _player?.Duration ?? 0;
    public bool IsPlaying => _player?.IsPlaying ?? false;

    public async Task PlayAsync(string audioUrl)
    {
        _player?.Stop();
        _player?.Dispose();

        var stream = await GetStreamFromUrlAsync(audioUrl);
        _player = _audioManager.CreatePlayer(stream);
        _player.Play();
    }

    public void Pause() => _player?.Pause();
    public void Resume() => _player?.Play();
    public void Stop() => _player?.Stop();
    public void Seek(double positionSeconds) => _player?.Seek(positionSeconds);

    private static async Task<Stream> GetStreamFromUrlAsync(string url)
    {
        using var client = new HttpClient();
        var bytes = await client.GetByteArrayAsync(url);
        return new MemoryStream(bytes);
    }
}
