using Microsoft.Extensions.DependencyInjection;
using SpotiClone.ViewModels;

namespace SpotiClone.Views.Controls;

public partial class MiniPlayer : ContentView
{
    private readonly MiniPlayerViewModel _viewModel;
    private double _panStartRatio;

    public MiniPlayer()
    {
        InitializeComponent();
        _viewModel = IPlatformApplication.Current!.Services.GetRequiredService<MiniPlayerViewModel>();
        BindingContext = _viewModel;
        _viewModel.Player.PropertyChanged += OnPlayerPropertyChanged;
        SeekBarContainer.SizeChanged += (_, _) => UpdateFill();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width > 0) UpdateFill();
    }

    private void OnPlayerPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayerViewModel.CurrentPositionMs) && !_viewModel.Player.IsSeeking)
            UpdateFill();
    }

    private void UpdateFill()
    {
        var w = SeekBarContainer.Width > 0 ? SeekBarContainer.Width : Width;
        if (w <= 0 || _viewModel.Player.DurationMs <= 0) return;
        var ratio = _viewModel.Player.CurrentPositionMs / _viewModel.Player.DurationMs;
        SeekBarFill.WidthRequest = w * ratio;
    }

    private void OnSeekBarTapped(object? sender, TappedEventArgs e)
    {
        var pos = e.GetPosition(SeekBarContainer);
        if (pos is null || SeekBarContainer.Width <= 0) return;
        var ratio = Math.Clamp(pos.Value.X / SeekBarContainer.Width, 0, 1);
        _viewModel.Player.BeginSeek();
        _viewModel.Player.CompleteSeek(_viewModel.Player.DurationMs * ratio);
        SeekBarFill.WidthRequest = SeekBarContainer.Width * ratio;
    }

    private void OnSeekBarPanned(object? sender, PanUpdatedEventArgs e)
    {
        if (SeekBarContainer.Width <= 0) return;
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _viewModel.Player.BeginSeek();
                _panStartRatio = _viewModel.Player.CurrentPositionMs / _viewModel.Player.DurationMs;
                break;
            case GestureStatus.Running:
                var ratio = Math.Clamp(_panStartRatio + e.TotalX / SeekBarContainer.Width, 0, 1);
                SeekBarFill.WidthRequest = SeekBarContainer.Width * ratio;
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                var finalRatio = Math.Clamp(_panStartRatio + e.TotalX / SeekBarContainer.Width, 0, 1);
                _viewModel.Player.CompleteSeek(_viewModel.Player.DurationMs * finalRatio);
                break;
        }
    }
}
