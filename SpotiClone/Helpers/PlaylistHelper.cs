using SpotiClone.Models;
using SpotiClone.Services;

namespace SpotiClone.Helpers;

public static class PlaylistHelper
{
    public static async Task AddTrackToPlaylistAsync(TrackDto track, IDatabaseService dbService)
    {
        var playlists = await dbService.GetPlaylistsAsync();
        if (playlists.Count == 0)
        {
            await Shell.Current.DisplayAlertAsync(
                "Немає плейлистів",
                "Спочатку створіть плейліст у розділі «Бібліотека».",
                "OK");
            return;
        }

        var options = playlists.Select(p => p.Name).ToArray();
        var chosen = await Shell.Current.DisplayActionSheetAsync(
            "Додати в плейліст", "Скасувати", null, options);

        if (chosen is null || chosen == "Скасувати") return;

        var playlist = playlists.FirstOrDefault(p => p.Name == chosen);
        if (playlist is null) return;

        await dbService.AddTrackToPlaylistAsync(
            playlist.Id, track.Id, track.Title,
            track.ArtistName ?? string.Empty,
            track.CoverUrl ?? string.Empty,
            track.AudioUrl, track.DurationMs);

        await Shell.Current.DisplayAlertAsync(
            "Додано",
            $"«{track.Title}» додано в «{playlist.Name}»",
            "OK");
    }
}
