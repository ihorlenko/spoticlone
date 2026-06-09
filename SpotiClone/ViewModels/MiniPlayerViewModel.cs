using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SpotiClone.ViewModels;

public partial class MiniPlayerViewModel : ObservableObject
{
    public PlayerViewModel Player { get; }

    [ObservableProperty]
    private bool _hasCurrentTrack;

    public MiniPlayerViewModel(PlayerViewModel player)
    {
        Player = player;
        Player.PropertyChanged += OnPlayerPropertyChanged;
    }

    private void OnPlayerPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayerViewModel.CurrentTrack))
            HasCurrentTrack = Player.CurrentTrack is not null;
    }

    [RelayCommand]
    private async Task OpenPlayerAsync()
    {
        if (Player.CurrentTrack is null) return;
        await Shell.Current.GoToAsync("player", new Dictionary<string, object>
        {
            ["TrackId"] = Player.CurrentTrack.Id
        });
    }
}
