namespace SpotiClone.Services;

public interface IPlayerService
{
    double CurrentPosition { get; }
    double Duration { get; }
    bool IsPlaying { get; }

    event EventHandler? PlaybackEnded;

    Task PlayAsync(string audioUrl);
    void Pause();
    void Resume();
    void Stop();
    void Seek(double positionSeconds);
}
