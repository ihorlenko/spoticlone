using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpotiClone.Data;
using SpotiClone.Models;
using SpotiClone.Services;

namespace SpotiClone.ViewModels;

public partial class HistoryViewModel : BaseViewModel
{
    private readonly IDatabaseService _dbService;
    private readonly IApiService _apiService;
    private readonly PlayerViewModel _playerViewModel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoHistory))]
    private ObservableCollection<ListeningHistoryEntity> _history = new();

    public bool HasNoHistory => History.Count == 0;

    public HistoryViewModel(IDatabaseService dbService, IApiService apiService, PlayerViewModel playerViewModel)
    {
        _dbService = dbService;
        _apiService = apiService;
        _playerViewModel = playerViewModel;
        Title = "Історія";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var items = await _dbService.GetListeningHistoryAsync();
            History = new ObservableCollection<ListeningHistoryEntity>(items);
        }
        catch (Exception) { }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        if (History.Count == 0) return;

        bool confirmed = await Shell.Current.DisplayAlertAsync(
            "Очистити історію",
            "Видалити всю історію прослуховувань?",
            "Очистити",
            "Скасувати");

        if (!confirmed) return;

        await _dbService.ClearListeningHistoryAsync();
        History.Clear();
    }

    [RelayCommand]
    private async Task PlayTrackAsync(ListeningHistoryEntity item)
    {
        if (item is null) return;

        TrackDto? trackDto = null;
        try
        {
            var cached = await _dbService.GetCachedTracksAsync();
            var cachedTrack = cached.FirstOrDefault(t => t.Id == item.TrackId);
            if (cachedTrack != null)
            {
                trackDto = new TrackDto
                {
                    Id = cachedTrack.Id,
                    Title = cachedTrack.Title,
                    ArtistName = cachedTrack.ArtistName,
                    CoverUrl = cachedTrack.CoverUrl,
                    AudioUrl = cachedTrack.AudioUrl,
                    DurationMs = cachedTrack.DurationMs
                };
            }
            else
            {
                trackDto = await _apiService.GetTrackAsync(item.TrackId);
            }
        }
        catch (Exception) { return; }

        if (trackDto is null) return;

        await _playerViewModel.PlayTrackAsync(trackDto, new List<TrackDto> { trackDto });
        await Shell.Current.GoToAsync("player", new Dictionary<string, object>
        {
            ["TrackId"] = item.TrackId
        });
    }
}
