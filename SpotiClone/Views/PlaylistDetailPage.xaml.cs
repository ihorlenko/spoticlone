using SpotiClone.ViewModels;

namespace SpotiClone.Views;

[QueryProperty(nameof(PlaylistId), "PlaylistId")]
[QueryProperty(nameof(PlaylistName), "PlaylistName")]
public partial class PlaylistDetailPage : ContentPage
{
    private readonly PlaylistDetailViewModel _viewModel;
    private int _pendingId;
    private string _pendingName = string.Empty;

    public string PlaylistId
    {
        set
        {
            if (int.TryParse(value, out var id))
                _pendingId = id;
        }
    }

    public string PlaylistName
    {
        set => _pendingName = value;
    }

    public PlaylistDetailPage(PlaylistDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_pendingId > 0)
            await _viewModel.InitAsync(_pendingId, _pendingName);
    }
}
