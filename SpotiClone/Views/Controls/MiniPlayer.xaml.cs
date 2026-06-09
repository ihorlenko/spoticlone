using Microsoft.Extensions.DependencyInjection;
using SpotiClone.ViewModels;

namespace SpotiClone.Views.Controls;

public partial class MiniPlayer : ContentView
{
    public MiniPlayer()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler?.MauiContext?.Services is { } services)
            BindingContext = services.GetRequiredService<MiniPlayerViewModel>();
    }
}
