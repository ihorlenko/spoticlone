using SpotiClone.ViewModels;

namespace SpotiClone.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is not HomeViewModel vm) return;
        if (vm.RecentTracks.Count == 0)
            vm.LoadCommand.Execute(null);
        else
            await vm.RefreshLikedStatusAsync();
    }
}
