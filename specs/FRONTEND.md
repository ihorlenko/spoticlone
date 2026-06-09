# FRONTEND.md — .NET MAUI специфікація

## Структура проєкту

```
SpotiClone/
├── SpotiClone.csproj
├── MauiProgram.cs               # DI реєстрація, плагіни
├── AppShell.xaml                # Shell навігація
├── AppShell.xaml.cs
├── App.xaml                     # Глобальні ресурси
├── App.xaml.cs
│
├── Models/                      # DTO моделі (відповідають API)
│   ├── TrackDto.cs
│   ├── ArtistDto.cs
│   ├── AlbumDto.cs
│   └── SearchResultDto.cs
│
├── Data/                        # SQLite сутності
│   ├── TrackEntity.cs
│   ├── ArtistEntity.cs
│   ├── AlbumEntity.cs
│   ├── PlaylistEntity.cs
│   ├── PlaylistTrackEntity.cs
│   ├── LikedTrackEntity.cs
│   ├── ListeningHistoryEntity.cs
│   └── SearchHistoryEntity.cs
│
├── Services/
│   ├── IApiService.cs           # Інтерфейс HTTP клієнта
│   ├── ApiService.cs            # Реалізація HTTP запитів
│   ├── IDatabaseService.cs      # Інтерфейс SQLite
│   ├── DatabaseService.cs       # Реалізація SQLite
│   ├── IPlayerService.cs        # Інтерфейс аудіо плеєра
│   └── PlayerService.cs         # Реалізація Plugin.Maui.Audio
│
├── ViewModels/
│   ├── BaseViewModel.cs         # INotifyPropertyChanged base
│   ├── HomeViewModel.cs
│   ├── SearchViewModel.cs
│   ├── LibraryViewModel.cs
│   ├── PlayerViewModel.cs
│   ├── HistoryViewModel.cs
│   └── MiniPlayerViewModel.cs
│
├── Views/
│   ├── HomePage.xaml
│   ├── HomePage.xaml.cs
│   ├── SearchPage.xaml
│   ├── SearchPage.xaml.cs
│   ├── LibraryPage.xaml
│   ├── LibraryPage.xaml.cs
│   ├── PlayerPage.xaml
│   ├── PlayerPage.xaml.cs
│   ├── HistoryPage.xaml
│   ├── HistoryPage.xaml.cs
│   └── Controls/
│       └── MiniPlayer.xaml      # Глобальний мінімальний плеєр
│
└── Resources/
    ├── Styles/
    │   ├── Colors.xaml
    │   └── Styles.xaml
    ├── Fonts/
    └── Images/
```

---

## NuGet пакети

```xml
<PackageReference Include="sqlite-net-pcl" Version="1.9.172" />
<PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.8" />
<PackageReference Include="Plugin.Maui.Audio" Version="3.0.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
```

---

## MauiProgram.cs — реєстрація DI

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseAudio()  // Plugin.Maui.Audio
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
            });

        // Services
        builder.Services.AddSingleton<IApiService, ApiService>();
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<IPlayerService, PlayerService>();

        // ViewModels (singleton для PlayerViewModel — стан плеєра глобальний)
        builder.Services.AddSingleton<PlayerViewModel>();
        builder.Services.AddSingleton<MiniPlayerViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<LibraryViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();

        // Views
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<LibraryPage>();
        builder.Services.AddTransient<PlayerPage>();
        builder.Services.AddTransient<HistoryPage>();

        return builder.Build();
    }
}
```

---

## Shell навігація (AppShell.xaml)

```xml
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:SpotiClone.Views"
       x:Class="SpotiClone.AppShell">

    <Shell.Resources>
        <!-- Shell стилі — темна тема Spotify -->
    </Shell.Resources>

    <TabBar>
        <ShellContent Title="Головна"
                      Icon="icon_home.png"
                      ContentTemplate="{DataTemplate views:HomePage}"
                      Route="home" />

        <ShellContent Title="Пошук"
                      Icon="icon_search.png"
                      ContentTemplate="{DataTemplate views:SearchPage}"
                      Route="search" />

        <ShellContent Title="Бібліотека"
                      Icon="icon_library.png"
                      ContentTemplate="{DataTemplate views:LibraryPage}"
                      Route="library" />

        <ShellContent Title="Історія"
                      Icon="icon_history.png"
                      ContentTemplate="{DataTemplate views:HistoryPage}"
                      Route="history" />
    </TabBar>

    <!-- PlayerPage — модальний, без табу -->
</Shell>
```

**Маршрути реєстрації (AppShell.xaml.cs):**
```csharp
Routing.RegisterRoute("player", typeof(PlayerPage));
```

**Навігація на PlayerPage:**
```csharp
await Shell.Current.GoToAsync("player", new Dictionary<string, object>
{
    ["TrackId"] = track.Id
});
```

---

## BaseViewModel

```csharp
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    public bool IsNotBusy => !IsBusy;
}
```

Використовує `CommunityToolkit.Mvvm` source generators: `[ObservableProperty]`, `[RelayCommand]`.

---

## Дизайн система (Colors.xaml)

Кольорова палітра в стилі Spotify:

```xml
<ResourceDictionary>
    <!-- Основні кольори -->
    <Color x:Key="SpotifyGreen">#1DB954</Color>
    <Color x:Key="BackgroundDark">#121212</Color>
    <Color x:Key="BackgroundCard">#282828</Color>
    <Color x:Key="BackgroundElevated">#3E3E3E</Color>
    <Color x:Key="TextPrimary">#FFFFFF</Color>
    <Color x:Key="TextSecondary">#B3B3B3</Color>
    <Color x:Key="TextMuted">#535353</Color>

    <!-- Семантичні -->
    <Color x:Key="AccentColor">#1DB954</Color>
    <Color x:Key="DangerColor">#E91429</Color>
    <Color x:Key="PageBackground">#121212</Color>
</ResourceDictionary>
```

---

## Глобальні стилі (Styles.xaml)

```xml
<!-- Label styles -->
<Style x:Key="TitleLabel" TargetType="Label">
    <Setter Property="FontSize" Value="24"/>
    <Setter Property="FontAttributes" Value="Bold"/>
    <Setter Property="TextColor" Value="{StaticResource TextPrimary}"/>
</Style>

<Style x:Key="SubtitleLabel" TargetType="Label">
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="TextColor" Value="{StaticResource TextSecondary}"/>
</Style>

<Style x:Key="CaptionLabel" TargetType="Label">
    <Setter Property="FontSize" Value="12"/>
    <Setter Property="TextColor" Value="{StaticResource TextMuted}"/>
</Style>

<!-- Button styles -->
<Style x:Key="PrimaryButton" TargetType="Button">
    <Setter Property="BackgroundColor" Value="{StaticResource SpotifyGreen}"/>
    <Setter Property="TextColor" Value="{StaticResource BackgroundDark}"/>
    <Setter Property="CornerRadius" Value="20"/>
    <Setter Property="FontAttributes" Value="Bold"/>
</Style>

<!-- Card style -->
<Style x:Key="CardFrame" TargetType="Frame">
    <Setter Property="BackgroundColor" Value="{StaticResource BackgroundCard}"/>
    <Setter Property="CornerRadius" Value="8"/>
    <Setter Property="Padding" Value="12"/>
    <Setter Property="HasShadow" Value="False"/>
</Style>
```

---

## Сторінки

### 1. HomePage (Головна)

**Призначення:** Відображення контенту з бекенду — нові треки, альбоми, артисти.

**ViewModel: HomeViewModel**
```csharp
public partial class HomeViewModel : BaseViewModel
{
    [ObservableProperty] private ObservableCollection<TrackDto> _recentTracks = new();
    [ObservableProperty] private ObservableCollection<AlbumDto> _featuredAlbums = new();
    [ObservableProperty] private ObservableCollection<ArtistDto> _artists = new();

    [RelayCommand]
    private async Task LoadAsync() { ... }

    [RelayCommand]
    private async Task PlayTrackAsync(TrackDto track) { ... }

    [RelayCommand]
    private async Task OpenAlbumAsync(AlbumDto album) { ... }
}
```

**View структура (HomePage.xaml):**
```
ScrollView
└── VerticalStackLayout
    ├── Label "Ласкаво просимо" (заголовок)
    ├── Label "Нові треки" (секція)
    ├── CollectionView (горизонтальний, треки)
    │   └── DataTemplate: картка треку (обкладинка + назва + артист + ♥ кнопка)
    ├── Label "Альбоми" (секція)
    ├── CollectionView (горизонтальний, альбоми)
    │   └── DataTemplate: картка альбому (обкладинка + назва + рік)
    └── Label "Артисти" (секція)
        └── CollectionView (горизонтальний, артисти)
            └── DataTemplate: картка артиста (фото + ім'я)
```

**Поведінка:**
- При завантаженні сторінки — `LoadCommand` виконується автоматично
- Треки і альбоми кешуються в SQLite після завантаження
- Натискання на трек → запускає відтворення + відкриває PlayerPage
- Натискання на ♥ → додає/видаляє з LikedTrack
- ActivityIndicator під час завантаження (IsBusy)

---

### 2. SearchPage (Пошук)

**Призначення:** Пошук треків, артистів, альбомів через API.

**ViewModel: SearchViewModel**
```csharp
public partial class SearchViewModel : BaseViewModel
{
    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private ObservableCollection<TrackDto> _trackResults = new();
    [ObservableProperty] private ObservableCollection<ArtistDto> _artistResults = new();
    [ObservableProperty] private ObservableCollection<AlbumDto> _albumResults = new();
    [ObservableProperty] private ObservableCollection<SearchHistoryEntity> _searchHistory = new();
    [ObservableProperty] private bool _hasResults;
    [ObservableProperty] private bool _showHistory;

    partial void OnSearchQueryChanged(string value)
    {
        // Debounce: якщо query порожній — показати історію
        // Якщо query >= 2 символи — запустити пошук
    }

    [RelayCommand]
    private async Task SearchAsync() { ... }

    [RelayCommand]
    private async Task PlayTrackAsync(TrackDto track) { ... }

    [RelayCommand]
    private async Task DeleteHistoryItemAsync(SearchHistoryEntity item) { ... }

    [RelayCommand]
    private async Task ClearHistoryAsync() { ... }
}
```

**View структура (SearchPage.xaml):**
```
VerticalStackLayout
├── SearchBar (прив'язка до SearchQuery)
├── [Якщо ShowHistory && query порожній]
│   ├── Label "Нещодавні пошуки"
│   └── CollectionView (SearchHistory)
│       └── DataTemplate: рядок запиту + кнопка видалення
└── [Якщо HasResults]
    └── ScrollView
        ├── Label "Треки" (якщо є)
        ├── CollectionView (TrackResults, вертикальний)
        ├── Label "Артисти" (якщо є)
        ├── CollectionView (ArtistResults)
        └── Label "Альбоми" (якщо є)
            └── CollectionView (AlbumResults)
```

**Поведінка:**
- Пошук з debounce 500ms після зупинки введення
- Мінімум 2 символи для старту пошуку
- При порожньому полі — показати SearchHistory
- Кожен пошук записується в SearchHistory (SQLite)
- Натискання на трек → відтворення

---

### 3. LibraryPage (Бібліотека)

**Призначення:** Лайкнуті треки та плейлисти користувача.

**ViewModel: LibraryViewModel**
```csharp
public partial class LibraryViewModel : BaseViewModel
{
    [ObservableProperty] private ObservableCollection<LikedTrackEntity> _likedTracks = new();
    [ObservableProperty] private ObservableCollection<PlaylistEntity> _playlists = new();
    [ObservableProperty] private int _selectedTabIndex; // 0 = Треки, 1 = Плейлисти

    [RelayCommand]
    private async Task LoadAsync() { ... }

    [RelayCommand]
    private async Task CreatePlaylistAsync() { ... }

    [RelayCommand]
    private async Task DeletePlaylistAsync(PlaylistEntity playlist) { ... }

    [RelayCommand]
    private async Task PlayLikedTrackAsync(LikedTrackEntity track) { ... }

    [RelayCommand]
    private async Task UnlikeTrackAsync(LikedTrackEntity track) { ... }
}
```

**View структура (LibraryPage.xaml):**
```
VerticalStackLayout
├── Label "Моя бібліотека"
├── [Tab switcher: "Треки" | "Плейлисти"]
├── [Якщо tab = Треки]
│   └── CollectionView (LikedTracks)
│       └── DataTemplate: обкладинка + назва + артист + кнопка ♥ (видалити)
└── [Якщо tab = Плейлисти]
    ├── Button "Створити плейлист"
    └── CollectionView (Playlists)
        └── DataTemplate: іконка + назва + дата + кнопка видалити
```

**Поведінка:**
- При відкритті сторінки — `LoadCommand` (дані з SQLite, не API)
- Створення плейлиста → діалог введення назви → збереження в SQLite
- Видалення плейлиста → підтвердження → видалення з SQLite
- Unlike треку → видалення з LikedTrack
- Натискання на трек → відтворення

---

### 4. PlayerPage (Плеєр)

**Призначення:** Повноекранний плеєр з управлінням відтворенням.

**Параметри навігації:** Отримує `TrackId` через `[QueryProperty]`.

**ViewModel: PlayerViewModel** (Singleton — стан плеєра зберігається між переходами)
```csharp
public partial class PlayerViewModel : BaseViewModel
{
    private readonly IPlayerService _playerService;
    private readonly IDatabaseService _dbService;

    // Поточний трек
    [ObservableProperty] private TrackDto _currentTrack;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _isLiked;

    // Прогрес
    [ObservableProperty] private double _currentPositionMs;
    [ObservableProperty] private double _durationMs;
    [ObservableProperty] private string _currentPositionFormatted; // "1:23"
    [ObservableProperty] private string _durationFormatted;        // "3:45"

    // Черга
    private List<TrackDto> _queue = new();
    private int _currentIndex;

    [RelayCommand] private async Task PlayPauseAsync() { ... }
    [RelayCommand] private async Task NextAsync() { ... }
    [RelayCommand] private async Task PreviousAsync() { ... }
    [RelayCommand] private async Task SeekAsync(double positionMs) { ... }
    [RelayCommand] private async Task ToggleLikeAsync() { ... }

    public async Task PlayTrackAsync(TrackDto track, List<TrackDto> queue = null) { ... }

    // Записує ListeningHistory при зупинці/зміні треку
    private async Task SaveListeningHistoryAsync() { ... }
}
```

**View структура (PlayerPage.xaml):**
```
Grid (повний екран, темний фон)
├── [Рядок 0] Button "↓" (закрити, повернутись назад)
├── [Рядок 1] Image (обкладинка альбому, 300x300, rounded corners)
├── [Рядок 2] VerticalStackLayout
│   ├── Label (назва треку, великий, білий)
│   └── Label (ім'я артиста, сірий)
├── [Рядок 3] Button "♥" (like/unlike, зелений якщо лайкнуто)
├── [Рядок 4] Slider (прогрес відтворення)
│   + Label (поточний час) + Label (тривалість)
└── [Рядок 5] HorizontalStackLayout (кнопки управління)
    ├── Button "⏮" (попередній)
    ├── Button "▶/⏸" (play/pause, велика)
    └── Button "⏭" (наступний)
```

**Поведінка:**
- Отримує TrackId через навігаційний параметр → завантажує трек
- Slider оновлюється кожну секунду через Timer
- При натисканні ♥ — додає/видаляє з LikedTrack (SQLite)
- При закритті/зміні треку — записує ListeningHistory
- Закриття → `Shell.Current.GoToAsync("..")`

---

### 5. HistoryPage (Історія)

**Призначення:** Список прослуханих треків з SQLite.

**ViewModel: HistoryViewModel**
```csharp
public partial class HistoryViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<ListeningHistoryEntity> _history = new();

    [RelayCommand] private async Task LoadAsync() { ... }
    [RelayCommand] private async Task ClearHistoryAsync() { ... }
    [RelayCommand] private async Task PlayTrackAsync(ListeningHistoryEntity item) { ... }
}
```

**View структура (HistoryPage.xaml):**
```
VerticalStackLayout
├── HorizontalStackLayout
│   ├── Label "Історія прослуховувань"
│   └── Button "Очистити"
└── CollectionView (History)
    └── DataTemplate:
        ├── Image (обкладинка)
        ├── VerticalStackLayout
        │   ├── Label (назва треку)
        │   └── Label (артист)
        └── Label (дата + час прослуховування)
```

---

## MiniPlayer (Controls/MiniPlayer.xaml)

Глобальний міні-плеєр, що відображається внизу екрана над панеллю табів. Використовує `PlayerViewModel` (singleton).

**Структура:**
```xml
<Grid HeightRequest="60" BackgroundColor="{StaticResource BackgroundCard}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="60"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="44"/>
    </Grid.ColumnDefinitions>

    <!-- Обкладинка -->
    <Image Grid.Column="0" Source="{Binding CurrentTrack.CoverUrl}"/>

    <!-- Назва + артист -->
    <VerticalStackLayout Grid.Column="1">
        <Label Text="{Binding CurrentTrack.Title}"/>
        <Label Text="{Binding CurrentTrack.ArtistName}"/>
    </VerticalStackLayout>

    <!-- Play/Pause -->
    <Button Grid.Column="2" Command="{Binding PlayPauseCommand}"/>

    <!-- Tap → відкрити PlayerPage -->
    <Grid.GestureRecognizers>
        <TapGestureRecognizer Command="{Binding OpenPlayerCommand}"/>
    </Grid.GestureRecognizers>
</Grid>
```

MiniPlayer вбудовується в `AppShell.xaml` через `Shell.BottomBar` або як фіксований елемент над TabBar.

---

## ApiService

```csharp
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:8000"; // або з конфігу

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<List<TrackDto>> GetTracksAsync(int limit = 20, int skip = 0)
    {
        var response = await _httpClient.GetAsync($"/tracks?limit={limit}&skip={skip}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TrackDto>>();
    }

    public async Task<SearchResultDto> SearchAsync(string query)
    {
        var encoded = Uri.EscapeDataString(query);
        var response = await _httpClient.GetAsync($"/search?q={encoded}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SearchResultDto>();
    }

    // ... інші методи
}
```

---

## PlayerService

```csharp
public class PlayerService : IPlayerService
{
    private readonly IAudioManager _audioManager;
    private IAudioPlayer _player;

    public PlayerService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public async Task PlayAsync(string audioUrl)
    {
        _player?.Stop();
        _player?.Dispose();

        var stream = await GetStreamFromUrlAsync(audioUrl);
        _player = _audioManager.CreatePlayer(stream);
        _player.Play();
    }

    public void Pause() => _player?.Pause();
    public void Resume() => _player?.Play();
    public void Stop() => _player?.Stop();
    public void Seek(double positionSeconds) => _player?.Seek(positionSeconds);

    public double CurrentPosition => _player?.CurrentPosition ?? 0;
    public double Duration => _player?.Duration ?? 0;
    public bool IsPlaying => _player?.IsPlaying ?? false;

    private async Task<Stream> GetStreamFromUrlAsync(string url)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        return await response.Content.ReadAsStreamAsync();
    }
}
```

---

## DTO моделі (Models/)

```csharp
// Models/TrackDto.cs
public class TrackDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("artist_id")]
    public string ArtistId { get; set; }

    [JsonPropertyName("album_id")]
    public string AlbumId { get; set; }

    [JsonPropertyName("duration_ms")]
    public int DurationMs { get; set; }

    [JsonPropertyName("audio_url")]
    public string AudioUrl { get; set; }

    [JsonPropertyName("genre")]
    public string Genre { get; set; }

    [JsonPropertyName("artist_name")]
    public string ArtistName { get; set; }

    [JsonPropertyName("album_title")]
    public string AlbumTitle { get; set; }

    [JsonPropertyName("cover_url")]
    public string CoverUrl { get; set; }
}
```

Аналогічно для `ArtistDto`, `AlbumDto`, `SearchResultDto`.

---

## Адаптивність iOS vs macOS

| Елемент | iOS | macOS (Mac Catalyst) |
|---------|-----|---------------------|
| TabBar | Внизу | Внизу або бічна панель |
| MiniPlayer | Над TabBar | Над TabBar |
| PlayerPage | Модальний знизу | Модальний або окреме вікно |
| Розмір обкладинки | 280px | 320px |
| Шрифт Title | 22sp | 26sp |

Використовуй `OnPlatform` для специфічних налаштувань:
```xml
<Label FontSize="{OnPlatform iOS=22, MacCatalyst=26}"/>
```
