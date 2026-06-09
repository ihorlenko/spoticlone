using SpotiClone.ViewModels;

namespace SpotiClone.Views;

[QueryProperty(nameof(TrackId), "TrackId")]
public partial class PlayerPage : ContentPage
{
    private readonly PlayerViewModel _viewModel;

    public string TrackId
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
                _ = _viewModel.EnsureTrackLoadedAsync(value);
        }
    }

    public PlayerPage(PlayerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private void OnSliderDragStarted(object? sender, EventArgs e)
    {
        _viewModel.BeginSeek();
    }

    private void OnSliderDragCompleted(object? sender, EventArgs e)
    {
        if (sender is Slider slider)
            _viewModel.CompleteSeek(slider.Value);
    }
}
