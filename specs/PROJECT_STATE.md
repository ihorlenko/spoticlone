# PROJECT_STATE.md — Поточний стан проєкту

> **Цей файл редагує Claude Code** після кожної сесії роботи.
> Людина читає його на початку наступної сесії щоб зрозуміти стан.

---

## Загальний прогрес

**Поточна фаза:** 15 — Фінальна перевірка та polish (в процесі)
**Остання активна сесія:** 2026-06-08
**Загальна готовність:** 97% (15/16 фаз — фаза 15 виконана на ~75%)

---

## Що зроблено (останні зміни)

### Фаза 15 — В процесі (2026-06-08)
- ✅ `IDatabaseService` — додано `GetCachedAlbumsAsync()` та `GetCachedArtistsAsync()`
- ✅ `DatabaseService` — реалізовано нові методи кешу для альбомів і артистів
- ✅ `HomeViewModel.LoadAsync()` fallback — тепер завантажує треки + альбоми + артистів з SQLite при недоступному бекенді
- ✅ `PlayerViewModel.PlayTrackAsync()` — try/catch навколо `_playerService.PlayAsync` → DisplayAlert "Не вдалося завантажити аудіо" при помилці
- ✅ `SearchViewModel` — додано `ShowNoResults` property (true коли запит >= 2 символів та 0 результатів)
- ✅ `SearchPage.xaml` — empty state "🔍 Нічого не знайдено / Спробуйте інший запит" (IsVisible=ShowNoResults)
- ✅ Всі entity класи (`Data/*.cs`) — виправлено 34 CS8618 warnings: `= string.Empty` для рядкових полів
- ✅ `DatabaseService._db` — `= null!` для suppression CS8618
- ✅ `dotnet build -f net10.0-maccatalyst` — Build succeeded, **0 errors, 0 warnings** (було 34 warnings!)
- ⬜ Тестування на iOS Simulator / macOS (потребує запуску)
- ⬜ Скріншоти для курсової роботи

### Фаза 14 — Завершено (2026-06-08)
- ✅ `ViewModels/HistoryViewModel.cs` — LoadAsync (GetListeningHistoryAsync → History collection), ClearHistoryAsync (DisplayAlertAsync підтвердження → ClearListeningHistoryAsync → History.Clear), PlayTrackAsync (LikedTrackEntity → пошук в кеші GetCachedTracksAsync → fallback на IApiService.GetTrackAsync → PlayerViewModel.PlayTrackAsync → Shell.GoToAsync("player"))
- ✅ `HasNoHistory` — computed property з `[NotifyPropertyChangedFor]` на History
- ✅ `Views/HistoryPage.xaml` — Grid 2 рядки (контент + MiniPlayer), ActivityIndicator (IsBusy), ScrollView (IsNotBusy), Header Grid (TitleLabel + "Очистити" кнопка DangerColor), empty state (🕐 + "Немає записів"), BindableLayout для History з TapGestureRecognizer → PlayTrackCommand, Cover (RoundRectangle 6), TrackTitle/ArtistName, ListenedAt (dd.MM HH:mm)
- ✅ `Views/HistoryPage.xaml.cs` — DI-ін'єкція HistoryViewModel, `OnAppearing → LoadCommand`
- ✅ `MauiProgram.cs` — зареєстровано `HistoryViewModel` (transient) та `HistoryPage` (transient)
- ✅ `dotnet build -f net10.0-maccatalyst` — Build succeeded, 0 errors, 34 warnings (ті самі CS8618)

### Фаза 13 — Завершено (2026-06-08)
- ✅ `ViewModels/LibraryViewModel.cs` — SelectedTabIndex (0=Треки/1=Плейлисти), ShowTracks/ShowPlaylists (computed), HasNoLikedTracks/HasNoPlaylists (computed), LoadAsync, SelectTracksTabCommand, SelectPlaylistsTabCommand
- ✅ `CreatePlaylistAsync` → `DisplayPromptAsync` → `CreatePlaylistAsync(name)` → додає до Playlists колекції
- ✅ `DeletePlaylistAsync(PlaylistEntity)` → `DisplayAlertAsync` (підтвердження) → `DeletePlaylistAsync(id)` → видаляє з колекції
- ✅ `PlayLikedTrackAsync(LikedTrackEntity)` → конвертує в TrackDto → `_playerViewModel.PlayTrackAsync(dto, queue)` → `Shell.GoToAsync("player")`
- ✅ `UnlikeTrackAsync(LikedTrackEntity)` → `RemoveLikedTrackAsync` → видаляє з LikedTracks
- ✅ `Views/LibraryPage.xaml` — Grid 2 рядки (контент + MiniPlayer), tab switcher з DataTriggers (SpotifyGreen активна / BackgroundElevated неактивна), empty state для обох вкладок (♡ + "Немає..."), BindableLayout для LikedTracks і Playlists, команди через `{RelativeSource AncestorType={x:Type ContentPage}}, Path=BindingContext.CommandName`
- ✅ `Views/LibraryPage.xaml.cs` — DI-ін'єкція LibraryViewModel, `OnAppearing → LoadCommand`
- ✅ `MauiProgram.cs` — зареєстровано `LibraryViewModel` (transient) та `LibraryPage` (transient)
- ✅ `DisplayAlert` → `DisplayAlertAsync` (CS0618 виправлено)
- ✅ `dotnet build -f net10.0-maccatalyst` — Build succeeded, 0 errors, 34 warnings (ті самі CS8618)

### Фаза 12 — Завершено (2026-06-08)
- ✅ `ViewModels/SearchViewModel.cs` — повна реалізація: debounce 500ms (CancellationTokenSource), `OnSearchQueryChanged` → ClearResults / ShowHistory / RunDebouncedSearchAsync, `PerformSearchAsync` → API → TrackResults/ArtistResults/AlbumResults + запис SearchHistory в SQLite
- ✅ `HasTrackResults`, `HasArtistResults`, `HasAlbumResults` — обчислені властивості з OnPropertyChanged
- ✅ `PlayTrackCommand` → `_playerViewModel.PlayTrackAsync(track, queue)` + `Shell.GoToAsync("player")`
- ✅ `SelectHistoryItemCommand` → встановлює SearchQuery (тригерить debounce)
- ✅ `DeleteHistoryItemCommand` / `ClearHistoryCommand` → SQLite видалення + оновлення колекції
- ✅ `LoadHistoryCommand` → завантажує SearchHistory з SQLite при відкритті сторінки
- ✅ `Views/SearchPage.xaml` — повна реалізація: SearchBar (debounce через OnSearchQueryChanged), SearchHistory (BindableLayout з TapGestureRecognizer), результати (3 секції Треки/Артисти/Альбоми через BindableLayout), ActivityIndicator (IsBusy)
- ✅ Виправлено баг XAML: `TextColor="{StaticResource CaptionLabel}"` → `Style="{StaticResource CaptionLabel}"` (CaptionLabel є Style, не Color)
- ✅ `Views/SearchPage.xaml.cs` — DI-ін'єкція SearchViewModel, `BindingContext = _viewModel`, `OnAppearing → LoadHistoryCommand`
- ✅ `MauiProgram.cs` — зареєстровано `SearchViewModel` (transient) та `SearchPage` (transient)
- ✅ `dotnet build -f net10.0-maccatalyst` — Build succeeded, 0 errors, 34 warnings (ті самі CS8618)

### Фаза 11 — Завершено (2026-06-08)
- ✅ Створено `ViewModels/MiniPlayerViewModel.cs` (singleton) — ObservableObject з `Player` (PlayerViewModel), `HasCurrentTrack` (ObservableProperty), `OpenPlayerCommand`
- ✅ `HasCurrentTrack` оновлюється через підписку на `PlayerViewModel.PropertyChanged` при зміні `CurrentTrack`
- ✅ `OpenPlayerCommand` → `Shell.Current.GoToAsync("player", {TrackId})` якщо CurrentTrack не null
- ✅ Створено `Views/Controls/MiniPlayer.xaml` — ContentView: 3 колонки (60px cover, * info, 44px play/pause, 44px next)
- ✅ `IsVisible="{Binding HasCurrentTrack}"` на inner Grid — зникає коли нема треку (Row Height=Auto → 0px)
- ✅ DataTrigger для ▶/⏸ іконки на основі `Player.IsPlaying`
- ✅ `TapGestureRecognizer` → `OpenPlayerCommand` (відкриває PlayerPage)
- ✅ Створено `Views/Controls/MiniPlayer.xaml.cs` — `OnHandlerChanged` самостійно резолвить `MiniPlayerViewModel` з DI (handler pattern)
- ✅ Зареєстровано `MiniPlayerViewModel` як singleton в `MauiProgram.cs`
- ✅ Оновлено `Views/HomePage.xaml` — Grid з 2 рядками (Row 0: контент, Row 1: MiniPlayer), bottom padding ScrollView зменшено з 80 до 16
- ✅ Оновлено stub-сторінки (SearchPage, LibraryPage, HistoryPage) — додано Grid з MiniPlayer (Row 1), додано `xmlns:controls` namespace
- ✅ `dotnet build -f net10.0-maccatalyst` — Build succeeded, 0 errors

### Фаза 10 — Завершено (2026-06-08)
- ✅ Створено `ViewModels/PlayerViewModel.cs` (singleton) — ObservableObject з CurrentTrack, IsPlaying, IsLiked, прогрес (CurrentPositionMs/DurationMs/Formatted), черга треків
- ✅ `PlayTrackAsync(TrackDto, List<TrackDto>?)` — зберігає історію попереднього треку, оновлює чергу, викликає _playerService.PlayAsync, запускає IDispatcherTimer
- ✅ `EnsureTrackLoadedAsync(trackId)` — викликається з PlayerPage при навігації; якщо CurrentTrack.Id вже == trackId, нічого не робить (аудіо вже грає)
- ✅ `PlayPauseCommand`, `NextCommand`, `PreviousCommand` — Previous: якщо > 3с → rewind, інакше попередній трек
- ✅ `ToggleLikeCommand` — додає/видаляє LikedTrackEntity з SQLite
- ✅ `CloseCommand` → `Shell.Current.GoToAsync("..")`
- ✅ `BeginSeek()` / `CompleteSeek(ms)` — публічні методи для code-behind Slider events
- ✅ `SaveListeningHistoryAsync()` — записує ListeningHistoryEntity при зміні треку
- ✅ Timer оновлює CurrentPositionMs кожну секунду, ігнорує оновлення під час drag (_isSeeking)
- ✅ Оновлено `Views/PlayerPage.xaml` — Grid 5 rows: header+close, cover (Border RoundRectangle 16), track info+like (DataTrigger ♥/♡), Slider+times, controls (⏮ ▶/⏸ ⏭)
- ✅ Оновлено `Views/PlayerPage.xaml.cs` — `[QueryProperty]` для TrackId, DI injection PlayerViewModel, OnSliderDragStarted/Completed → BeginSeek/CompleteSeek
- ✅ Оновлено `ViewModels/HomeViewModel.cs` — замінено _playerService на _playerViewModel; PlayTrackAsync тепер викликає `_playerViewModel.PlayTrackAsync(track, queue)` з повною чергою треків
- ✅ Оновлено `MauiProgram.cs` — `PlayerViewModel` (singleton), `PlayerPage` (transient)
- ✅ `dotnet build -f net10.0-maccatalyst` — Build succeeded, 0 errors

### Фаза 9 — Завершено (2026-06-08)
- ✅ `ViewModels/TrackLikeItem.cs` (в HomeViewModel.cs) — ObservableObject-обгортка для TrackDto з реактивним `IsLiked`
- ✅ `ViewModels/HomeViewModel.cs` оновлено — IPlayerService injection, кешування треків/альбомів/артистів у SQLite, fallback на кеш при відсутності мережі
- ✅ `Views/HomePage.xaml` — повна реалізація: ActivityIndicator (IsBusy), ScrollView, 3 горизонтальні CollectionView (треки, альбоми, артисти), ♥/♡ кнопка через DataTrigger
- ✅ `Views/HomePage.xaml.cs` — DI-ін'єкція HomeViewModel, OnAppearing→LoadCommand
- ✅ `MauiProgram.cs` — зареєстровано HomeViewModel (transient) та HomePage (transient)
- ✅ x:DataType прибрано з ContentPage (runtime bindings — правило MAUI: DataTemplate без x:DataType дає MAUIG1001 при x:DataType на Page)
- ✅ `dotnet build -f net10.0-maccatalyst` — Build succeeded, 0 errors

### Фаза 8 — Завершено (2026-06-08)
- ✅ Створено `ViewModels/BaseViewModel.cs` — ObservableObject з `IsBusy`, `IsNotBusy`, `Title`
- ✅ Оновлено `Resources/Styles/Colors.xaml` — Spotify палітра (SpotifyGreen, BackgroundDark, BackgroundCard, TextPrimary/Secondary/Muted тощо). Legacy-ключі збережені для сумісності з Styles.xaml
- ✅ Оновлено `Resources/Styles/Styles.xaml` — глобальний Shell/Page фон → BackgroundDark, TabBar → SpotifyGreen активний / TextSecondary неактивний. Додано іменовані стилі: TitleLabel, SubtitleLabel, CaptionLabel, PrimaryButton, CardBorder (Border замість Frame, бо Frame застарілий у .NET 10)
- ✅ Створено stub-сторінки: HomePage, SearchPage, LibraryPage, HistoryPage, PlayerPage (Views/)
- ✅ Оновлено `AppShell.xaml` — TabBar з 4 вкладками (Головна / Пошук / Бібліотека / Історія)
- ✅ Оновлено `AppShell.xaml.cs` — `Routing.RegisterRoute("player", typeof(PlayerPage))`
- ✅ Перевірено: `dotnet build -f net10.0-maccatalyst` — Build succeeded, 0 errors

### Фаза 7 — Завершено (2026-06-08)
- ✅ Створено `Services/IPlayerService.cs` — інтерфейс з 5 методами та 3 властивостями
- ✅ Створено `Services/PlayerService.cs` — реалізація через `IAudioManager` (Plugin.Maui.Audio)
- ✅ `GetStreamFromUrlAsync` завантажує аудіо в `MemoryStream` (безпечно, без витоку HttpClient)
- ✅ Підключено Plugin.Maui.Audio через `.AddAudio()` (не `.UseAudio()` — такого методу в v3.0.0 немає)
- ✅ Зареєстровано `IPlayerService/PlayerService` як singleton в `MauiProgram.cs`
- ✅ Перевірено: `dotnet build -f net10.0-maccatalyst` — Build succeeded, 0 errors

### Фаза 6 — Завершено (2026-06-08)
- ✅ Створено 4 DTO моделі в `Models/`: TrackDto, ArtistDto, AlbumDto, SearchResultDto
- ✅ Усі поля з `[JsonPropertyName]` атрибутами (snake_case → PascalCase)
- ✅ ArtistDto має опціональні `Tracks` та `Albums` для детального ендпоінту
- ✅ AlbumDto має опціональне поле `Tracks` (лише в GET /albums/{id})
- ✅ SearchResultDto відповідає бекенду (tracks/artists/albums/query/total_results)
- ✅ Створено `Services/IApiService.cs` з 7 методами (GetTracks, GetTrack, GetArtists, GetArtist, GetAlbums, GetAlbum, Search)
- ✅ Створено `Services/ApiService.cs` — HttpClient singleton, BaseAddress = http://localhost:8000
- ✅ Зареєстровано `HttpClient` та `IApiService/ApiService` як singleton в `MauiProgram.cs`
- ✅ Перевірено: `dotnet build -f net10.0-maccatalyst` — Build succeeded, 0 errors
- ✅ Перевірено: `GET http://localhost:8000/tracks` — бекенд відповідає коректним JSON

### Фаза 5 — Завершено (2026-06-08)
- ✅ Створено 8 entity класів у `Data/`: TrackEntity, ArtistEntity, AlbumEntity, PlaylistEntity, PlaylistTrackEntity, LikedTrackEntity, ListeningHistoryEntity, SearchHistoryEntity
- ✅ Створено `Services/IDatabaseService.cs` — повний інтерфейс (кеш, лайки, плейлисти, історія, пошук)
- ✅ Створено `Services/DatabaseService.cs` — реалізація з lazy `InitAsync()` (lazy ініціалізація при першому виклику)
- ✅ Bulk insert через `RunInTransactionAsync` + `InsertOrReplace` (sqlite-net-pcl 1.9 не має `InsertAllAsync(replace:true)`)
- ✅ Зареєстровано `IDatabaseService` / `DatabaseService` як singleton в `MauiProgram.cs`
- ✅ Перевірено: `dotnet build -f net10.0-maccatalyst` — Build succeeded, 0 errors

### Фаза 4 — Завершено (2026-06-08)
- ✅ Створено MAUI проєкт `dotnet new maui -n SpotiClone`
- ✅ Налаштовано `SpotiClone.csproj` — лише `net10.0-ios` та `net10.0-maccatalyst` (Android/Windows видалено)
- ✅ Додано NuGet пакети: sqlite-net-pcl 1.9.172, SQLitePCLRaw.bundle_green 2.1.8, Plugin.Maui.Audio 3.0.0, CommunityToolkit.Mvvm 8.3.2
- ✅ Створено структуру директорій: Models/, Data/, Services/, ViewModels/, Views/, Views/Controls/
- ✅ Перевірено: `dotnet build -f net10.0-maccatalyst` — Build succeeded, 0 errors

### Фаза 3 — Завершено (2026-06-08)
- ✅ Створено `backend/seed.py` — синхронний asyncio-скрипт з motor
- ✅ Seed дані: 3 артисти (Michael Jackson, Ozzy Osbourne, Queen), 3 альбоми, 3 треки
- ✅ Seed використовує реальні імена файлів з `static/audio/` та `static/covers/`
- ✅ Перевірено: `GET /tracks` → 3 треки з денормалізованими artist_name, album_title, cover_url
- ✅ Перевірено: `GET /search?q=rock` → 2 результати (Bohemian Rhapsody + Mama I'm Coming Home)
- ✅ Перевірено: статичні файли обслуговуються з `accept-ranges: bytes` (підтримка Range requests)

### Фаза 2 — Завершено (2026-06-08)
- ✅ Створено `backend/models/track.py` — TrackResponse (з денормалізованими artist_name, album_title, cover_url)
- ✅ Створено `backend/models/artist.py` — ArtistResponse (з опціональними tracks/albums для детального ендпоінту)
- ✅ Створено `backend/models/album.py` — AlbumResponse, SearchResponse
- ✅ Створено `backend/utils.py` — спільні хелпери build_track/build_artist/build_album
- ✅ Створено `backend/routers/tracks.py` — GET /tracks (з пагінацією та фільтром genre), GET /tracks/{id}
- ✅ Створено `backend/routers/artists.py` — GET /artists, GET /artists/{id} (з треками та альбомами)
- ✅ Створено `backend/routers/albums.py` — GET /albums, GET /albums/{id} (з треками)
- ✅ Створено `backend/routers/search.py` — GET /search?q= (asyncio.gather по 3 колекціях)
- ✅ Оновлено `backend/main.py` — підключено всі 4 роутери
- ✅ Перевірено: `/docs` відповідає 200, всі 8 ендпоінтів зареєстровані та повертають коректний JSON

### Фаза 1 — Завершено (2026-06-08)
- ✅ Створено `backend/requirements.txt` (fastapi, motor, pydantic-settings, uvicorn, aiofiles)
- ✅ Створено `backend/config.py` з pydantic-settings (MONGODB_URI, DB_NAME, BASE_URL тощо)
- ✅ Створено `backend/database.py` з Motor async client та колекціями tracks/artists/albums
- ✅ Створено `backend/main.py` з CORS middleware, StaticFiles mount, lifespan (MongoDB індекси)
- ✅ Створено `.venv` у `backend/` з усіма залежностями
- ✅ Перевірено: `uvicorn main:app` запускається без помилок
- ✅ Перевірено: `http://localhost:8000/docs` відповідає HTML (Swagger UI)

### Фаза 0 — Завершено (2026-06-08)
- ✅ Перевірено Xcode 26.2.0 (активний)
- ✅ Перевірено .NET SDK 10.0.201
- ✅ Встановлено Python 3.11.15 через Homebrew (`/opt/homebrew/bin/python3.11`)
- ✅ Перевірено MongoDB 8.2.7
- ✅ Створено директорію `backend/` зі структурою: `models/`, `routers/`, `static/audio/`, `static/covers/`
- ✅ Директорія `specs/` вже існувала з усіма 7 файлами

---

## Що робити далі

**Фаза 15 залишилось:**
- Запустити застосунок на iOS Simulator або macOS (Mac Catalyst) і перевірити реальне відтворення
- Зробити скріншоти для курсової роботи: HomePage, SearchPage, PlayerPage, LibraryPage, HistoryPage
- Команда запуску: `dotnet build -t:Run -f net10.0-maccatalyst` (або відкрити у Xcode)

---

## Відомі проблеми / Блокери

- **Python PATH:** Системний `python3` = 3.9.6. Для запуску бекенду використовуй `/opt/homebrew/bin/python3.11` або `python3.11`. Краще створити venv: `python3.11 -m venv backend/.venv`
- **SpotiClone/ директорія:** Не створена навмисно — `dotnet new maui -n SpotiClone` (Фаза 4) створить її сам. Якщо створити порожню директорію наперед — dotnet new може видати помилку.

---

## Технічні нотатки

### Середовище
- MacBook Air M1, macOS Tahoe 26 beta
- Активний Xcode: ✅ **26.2.0** (`/Applications/Xcode-26.2.0.app/Contents/Developer`)
- .NET SDK: ✅ **10.0.201**
- Python: ✅ **3.11.15** (`/opt/homebrew/bin/python3.11`) — системний python3 = 3.9.6, не використовувати
- MongoDB: ✅ **8.2.7** — встановлено, потребує запуску `mongod`

### Файли аудіо та обкладинок
- Аудіофайли (.mp3) користувач додає вручну в `backend/static/audio/`
- Обкладинки (.jpg/.png) користувач додає вручну в `backend/static/covers/`
- Після додавання файлів — запустити `python seed.py` для заповнення MongoDB

### Xcode версія
Якщо виникають помилки компіляції MAUI — виконай:
```bash
sudo xcode-select -s /Applications/Xcode-26.2.0.app/Contents/Developer
```

---

## Структура файлів (актуальна)

```
spoticlone/
├── specs/            ← ✅ готово (всі 7 файлів)
├── backend/          ← ✅ повністю реалізовано (Фази 1–2)
│   ├── main.py       ← CORS, StaticFiles, 4 роутери підключені
│   ├── config.py
│   ├── database.py
│   ├── utils.py      ← build_track/build_artist/build_album
│   ├── models/
│   │   ├── track.py    ← TrackResponse
│   │   ├── artist.py   ← ArtistResponse
│   │   └── album.py    ← AlbumResponse, SearchResponse
│   ├── routers/
│   │   ├── tracks.py   ← GET /tracks, GET /tracks/{id}
│   │   ├── artists.py  ← GET /artists, GET /artists/{id}
│   │   ├── albums.py   ← GET /albums, GET /albums/{id}
│   │   └── search.py   ← GET /search?q=
│   ├── seed.py       ← 3 артисти / 3 альбоми / 3 треки
│   └── static/
│       ├── audio/    ← ✅ 3 MP3 файли
│       └── covers/   ← ✅ 3 JPG обкладинки
└── SpotiClone/       ← ✅ створено (Фаза 4)
    ├── SpotiClone.csproj  ← iOS + MacCatalyst only, 4 NuGet пакети
    ├── MauiProgram.cs ← IDatabaseService зареєстровано як singleton
    ├── Models/       ← ✅ 4 DTO класи (Фаза 6)
    ├── Data/         ← ✅ 8 entity класів (Фаза 5)
    │   ├── TrackEntity.cs
    │   ├── ArtistEntity.cs
    │   ├── AlbumEntity.cs
    │   ├── PlaylistEntity.cs
    │   ├── PlaylistTrackEntity.cs
    │   ├── LikedTrackEntity.cs
    │   ├── ListeningHistoryEntity.cs
    │   └── SearchHistoryEntity.cs
    ├── Services/     ← ✅ IDatabaseService + DatabaseService (Фаза 5), IApiService + ApiService (Фаза 6), IPlayerService + PlayerService (Фаза 7)
    ├── ViewModels/   ← ✅ BaseViewModel (Фаза 8)
    └── Views/
        ├── HomePage.xaml       ← ✅ stub (Фаза 8)
        ├── SearchPage.xaml     ← ✅ stub (Фаза 8)
        ├── LibraryPage.xaml    ← ✅ stub (Фаза 8)
        ├── HistoryPage.xaml    ← ✅ stub (Фаза 8)
        ├── PlayerPage.xaml     ← ✅ stub (Фаза 8)
        └── Controls/ ← порожня (заповниться в Фазі 11)
```

---

## Журнал сесій

| Дата | Що зроблено | Проблеми |
|------|-------------|----------|
| — | Специфікації написані в Claude Chat | — |
| 2026-06-08 | Фаза 0: перевірка середовища, створення директорій | Python 3.9 → встановлено 3.11.15 через brew |
| 2026-06-08 | Фаза 1: requirements.txt, config.py, database.py, main.py | — |
| 2026-06-08 | Фаза 2: models (track/artist/album), routers (tracks/artists/albums/search), utils.py | — |
| 2026-06-08 | Фаза 3: seed.py (3 артисти/альбоми/треки), перевірено /tracks та /search | — |
| 2026-06-08 | Фаза 4: MAUI проєкт (iOS+MacCatalyst), 4 NuGet пакети, структура директорій, build OK | — |
| 2026-06-08 | Фаза 5: 8 entity класів в Data/, IDatabaseService, DatabaseService, DI реєстрація, build OK | sqlite-net-pcl 1.9 не має InsertAllAsync(replace:true) → використано RunInTransactionAsync |
| 2026-06-08 | Фаза 6: 4 DTO в Models/, IApiService, ApiService (HttpClient singleton), DI реєстрація, build OK | — |
| 2026-06-08 | Фаза 7: IPlayerService, PlayerService (IAudioManager), .AddAudio() в MauiProgram.cs, build OK | .UseAudio() не існує в v3.0.0 → виправлено на .AddAudio() |
| 2026-06-08 | Фаза 8: BaseViewModel, Colors/Styles.xaml (Spotify), stub Views (5 сторінок), AppShell TabBar, маршрут player, build OK | Frame застарілий у .NET 10 → CardBorder замість CardFrame (TargetType="Border") |
| 2026-06-08 | Фаза 9: HomeViewModel (TrackLikeItem wrapper), HomePage.xaml (3 CollectionViews горизонтальні, ♥/♡ кнопка), DI реєстрація, build OK | x:DataType на ContentPage + DataTemplate без x:DataType → MAUIG1001 → прибрано x:DataType з Page |
| 2026-06-08 | Фаза 10: PlayerViewModel (singleton, timer, queue, like/history), PlayerPage.xaml (повноекранний плеєр), PlayerPage.xaml.cs (QueryProperty), HomeViewModel → PlayerViewModel, DI, build OK | object sender → object? sender для EventHandler nullability |
| 2026-06-08 | Фаза 11: MiniPlayerViewModel (singleton, HasCurrentTrack, OpenPlayerCommand), MiniPlayer.xaml (ContentView 60px, 4 cols), OnHandlerChanged DI resolver, інтеграція в усі 4 tab-сторінки, build OK | Shell.BottomBar не існує в MAUI → MiniPlayer додано в кожну tab-сторінку як Row 1 Grid |
| 2026-06-08 | Фаза 12: SearchViewModel (debounce 500ms, LoadHistoryCommand, PlayTrackCommand, DeleteHistoryItemCommand, ClearHistoryCommand), SearchPage.xaml.cs (DI+BindingContext+OnAppearing), MauiProgram DI реєстрація, XAML bug fix (CaptionLabel), build OK | — |
| 2026-06-08 | Фаза 13: LibraryViewModel (tab switcher, LoadAsync, CreatePlaylistCommand, DeletePlaylistCommand, PlayLikedTrackCommand, UnlikeTrackCommand), LibraryPage.xaml (DataTriggers tab styles, BindableLayout, empty states), LibraryPage.xaml.cs (DI), MauiProgram DI, DisplayAlertAsync fix, build OK | — |
| 2026-06-08 | Фаза 14: HistoryViewModel (LoadAsync, ClearHistoryAsync з підтвердженням, PlayTrackAsync з cache/API fallback), HistoryPage.xaml (header+кнопка Очистити, empty state, BindableLayout з TapGesture, ListenedAt формат), HistoryPage.xaml.cs (DI), MauiProgram DI реєстрація, build OK | — |
| 2026-06-08 | Фаза 15 (polish): GetCachedAlbums/Artists методи, HomeViewModel fallback повний, PlayerViewModel try/catch PlayAsync+DisplayAlert, SearchViewModel ShowNoResults, SearchPage empty state "нічого не знайдено", 34→0 CS8618 warnings (= string.Empty в entity класах), build 0 errors 0 warnings | — |
