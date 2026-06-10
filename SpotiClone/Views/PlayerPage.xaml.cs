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
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayerViewModel.CurrentPositionMs) && !_viewModel.IsSeeking)
            ProgressSlider.Value = _viewModel.CurrentPositionMs;

        if (e.PropertyName == nameof(PlayerViewModel.CurrentLyricIndex))
            ScrollToCurrentLyric(_viewModel.CurrentLyricIndex);
    }

    private void ScrollToCurrentLyric(int index)
    {
        if (index < 0 || index >= LyricsContainer.Children.Count) return;
        _ = MainScrollView.ScrollToAsync(
            (VisualElement)LyricsContainer.Children[index],
            ScrollToPosition.Center,
            animated: true);
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
