using SpotiClone.Views;

namespace SpotiClone;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("player", typeof(PlayerPage));
        Routing.RegisterRoute("playlist-detail", typeof(PlaylistDetailPage));
    }
}
