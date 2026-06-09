using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpotiClone.Data;
using SpotiClone.Models;
using SpotiClone.Services;

namespace SpotiClone.ViewModels;

public partial class SearchViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly IDatabaseService _dbService;
    private readonly PlayerViewModel _playerViewModel;
    private CancellationTokenSource _debounce = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TrackDto> _trackResults = new();

    [ObservableProperty]
    private ObservableCollection<ArtistDto> _artistResults = new();

    [ObservableProperty]
    private ObservableCollection<AlbumDto> _albumResults = new();

    [ObservableProperty]
    private ObservableCollection<SearchHistoryEntity> _searchHistory = new();

    [ObservableProperty]
    private bool _hasResults;

    [ObservableProperty]
    private bool _showHistory;

    [ObservableProperty]
    private bool _showNoResults;

    public bool HasTrackResults => TrackResults.Count > 0;
    public bool HasArtistResults => ArtistResults.Count > 0;
    public bool HasAlbumResults => AlbumResults.Count > 0;

    public SearchViewModel(IApiService apiService, IDatabaseService dbService, PlayerViewModel playerViewModel)
    {
        _apiService = apiService;
        _dbService = dbService;
        _playerViewModel = playerViewModel;
        Title = "Пошук";
    }

    partial void OnSearchQueryChanged(string value)
    {
        _debounce.Cancel();
        _debounce = new CancellationTokenSource();
        var token = _debounce.Token;

        if (string.IsNullOrWhiteSpace(value))
        {
            ClearResults();
            IsBusy = false;
            ShowNoResults = false;
            ShowHistory = SearchHistory.Count > 0;
            return;
        }

        if (value.Length < 2)
        {
            ClearResults();
            IsBusy = false;
            ShowNoResults = false;
            ShowHistory = false;
            return;
        }

        ShowHistory = false;
        ShowNoResults = false;
        IsBusy = true;
        _ = RunDebouncedSearchAsync(value, token);
    }

    private void ClearResults()
    {
        TrackResults.Clear();
        ArtistResults.Clear();
        AlbumResults.Clear();
        HasResults = false;
        OnPropertyChanged(nameof(HasTrackResults));
        OnPropertyChanged(nameof(HasArtistResults));
        OnPropertyChanged(nameof(HasAlbumResults));
    }

    private async Task RunDebouncedSearchAsync(string query, CancellationToken token)
    {
        try
        {
            await Task.Delay(500, token);
            token.ThrowIfCancellationRequested();
            await PerformSearchAsync(query, token);
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            if (!token.IsCancellationRequested)
                IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Length < 2) return;
        _debounce.Cancel();
        _debounce = new CancellationTokenSource();
        IsBusy = true;
        try
        {
            await PerformSearchAsync(SearchQuery, _debounce.Token);
        }
        catch (OperationCanceledException) { }
    }

    private async Task PerformSearchAsync(string query, CancellationToken token)
    {
        try
        {
            var result = await _apiService.SearchAsync(query);
            token.ThrowIfCancellationRequested();

            TrackResults = new ObservableCollection<TrackDto>(result.Tracks ?? new());
            ArtistResults = new ObservableCollection<ArtistDto>(result.Artists ?? new());
            AlbumResults = new ObservableCollection<AlbumDto>(result.Albums ?? new());
            HasResults = TrackResults.Count > 0 || ArtistResults.Count > 0 || AlbumResults.Count > 0;
            ShowNoResults = !HasResults;
            OnPropertyChanged(nameof(HasTrackResults));
            OnPropertyChanged(nameof(HasArtistResults));
            OnPropertyChanged(nameof(HasAlbumResults));

            if (HasResults)
                await _dbService.AddSearchHistoryAsync(query, result.TotalResults);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception) { }
        finally
        {
            if (!token.IsCancellationRequested)
                IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        var history = await _dbService.GetSearchHistoryAsync(limit: 20);
        SearchHistory = new ObservableCollection<SearchHistoryEntity>(history);
        ShowHistory = SearchHistory.Count > 0 && string.IsNullOrWhiteSpace(SearchQuery);
    }

    [RelayCommand]
    private async Task PlayTrackAsync(TrackDto track)
    {
        if (track is null) return;
        var queue = TrackResults.ToList();
        await _playerViewModel.PlayTrackAsync(track, queue);
        await Shell.Current.GoToAsync("player", new Dictionary<string, object>
        {
            ["TrackId"] = track.Id
        });
    }

    [RelayCommand]
    private void SelectHistoryItem(SearchHistoryEntity item)
    {
        if (item is null) return;
        SearchQuery = item.Query;
    }

    [RelayCommand]
    private async Task DeleteHistoryItemAsync(SearchHistoryEntity item)
    {
        if (item is null) return;
        await _dbService.DeleteSearchHistoryItemAsync(item.Id);
        SearchHistory.Remove(item);
        ShowHistory = SearchHistory.Count > 0 && string.IsNullOrWhiteSpace(SearchQuery);
    }

    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        await _dbService.ClearSearchHistoryAsync();
        SearchHistory.Clear();
        ShowHistory = false;
    }
}
