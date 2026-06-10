using CommunityToolkit.Mvvm.ComponentModel;

namespace SpotiClone.Models;

public partial class LyricLine : ObservableObject
{
    public int TimeMs { get; init; }
    public string Text { get; init; } = string.Empty;
    [ObservableProperty] private bool _isActive;
}
