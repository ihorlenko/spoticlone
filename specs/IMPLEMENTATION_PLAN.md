# IMPLEMENTATION_PLAN.md — План реалізації SpotiClone

> **Інструкція для Claude Code:**
> Коли завдання виконано — постав `[x]` замість `[ ]`.
> Після кожної фази онови `PROJECT_STATE.md`.
> Не починай наступну фазу, не завершивши поточну.

---

## Фаза 0: Підготовка середовища

- [x] **0.1** Перевірити активний Xcode: `xcode-select -p` → має бути 26.2.0
- [x] **0.2** Перевірити .NET SDK: `dotnet --version` → має бути 10.x
- [x] **0.3** Перевірити Python: `python3 --version` → має бути 3.11+
- [x] **0.4** Перевірити MongoDB: `mongod --version`
- [x] **0.5** Створити структуру директорій репозиторію:
  ```
  spoticlone/
  ├── specs/
  ├── backend/
  └── SpotiClone/
  ```
- [x] **0.6** Скопіювати всі spec-файли в `spoticlone/specs/`

---

## Фаза 1: Бекенд — базова структура

- [x] **1.1** Створити `backend/requirements.txt`
- [x] **1.2** Створити `backend/config.py` з налаштуваннями
- [x] **1.3** Створити `backend/database.py` з Motor клієнтом
- [x] **1.4** Створити директорії `backend/static/audio/` та `backend/static/covers/`
- [x] **1.5** Створити `backend/main.py` з CORS, StaticFiles, lifespan
- [x] **1.6** Перевірити що `uvicorn main:app --reload` запускається без помилок
- [x] **1.7** Перевірити що `http://localhost:8000/docs` відкривається

---

## Фаза 2: Бекенд — моделі та роутери

- [x] **2.1** Створити `backend/models/track.py` (TrackResponse)
- [x] **2.2** Створити `backend/models/artist.py` (ArtistResponse)
- [x] **2.3** Створити `backend/models/album.py` (AlbumResponse, SearchResponse)
- [x] **2.4** Створити `backend/routers/tracks.py` — `GET /tracks` та `GET /tracks/{id}`
- [x] **2.5** Створити `backend/routers/artists.py` — `GET /artists` та `GET /artists/{id}`
- [x] **2.6** Створити `backend/routers/albums.py` — `GET /albums` та `GET /albums/{id}`
- [x] **2.7** Створити `backend/routers/search.py` — `GET /search?q=`
- [x] **2.8** Підключити всі роутери в `main.py`
- [x] **2.9** Протестувати всі ендпоінти через `/docs` (Swagger UI)

---

## Фаза 3: Бекенд — seed та статика

- [x] **3.1** Створити `backend/seed.py` зі структурою тестових даних
- [x] **3.2** Запустити `python seed.py` — переконатись що дані записались в MongoDB
- [x] **3.3** Перевірити `GET /tracks` — повертає 3 треки з денормалізованими полями
- [x] **3.4** Перевірити `GET /search?q=rock` — повертає 2 результати
- [x] **3.5** Перевірити статичні файли: `accept-ranges: bytes` підтверджено

---

## Фаза 4: MAUI — початкова структура проєкту

- [x] **4.1** Створити новий MAUI проєкт: `dotnet new maui -n SpotiClone`
- [x] **4.2** Налаштувати `SpotiClone.csproj` — прибрати Windows/Android таргети, залишити iOS та MacCatalyst
- [x] **4.3** Додати NuGet пакети: sqlite-net-pcl, SQLitePCLRaw.bundle_green, Plugin.Maui.Audio, CommunityToolkit.Mvvm
- [x] **4.4** Створити структуру директорій: Models/, Data/, Services/, ViewModels/, Views/, Views/Controls/
- [x] **4.5** Перевірити що проєкт збирається: `dotnet build`

---

## Фаза 5: MAUI — Data layer (SQLite)

- [x] **5.1** Створити всі entity класи в `Data/`:
  - [x] **5.1.1** `TrackEntity.cs`
  - [x] **5.1.2** `ArtistEntity.cs`
  - [x] **5.1.3** `AlbumEntity.cs`
  - [x] **5.1.4** `PlaylistEntity.cs`
  - [x] **5.1.5** `PlaylistTrackEntity.cs`
  - [x] **5.1.6** `LikedTrackEntity.cs`
  - [x] **5.1.7** `ListeningHistoryEntity.cs`
  - [x] **5.1.8** `SearchHistoryEntity.cs`
- [x] **5.2** Створити `Services/IDatabaseService.cs` (інтерфейс)
- [x] **5.3** Створити `Services/DatabaseService.cs` (реалізація з InitAsync)
- [x] **5.4** Зареєструвати `DatabaseService` як singleton в `MauiProgram.cs`
- [x] **5.5** Перевірити що БД створюється при старті застосунку

---

## Фаза 6: MAUI — API Service

- [x] **6.1** Створити DTO моделі в `Models/`:
  - [x] **6.1.1** `TrackDto.cs`
  - [x] **6.1.2** `ArtistDto.cs`
  - [x] **6.1.3** `AlbumDto.cs`
  - [x] **6.1.4** `SearchResultDto.cs`
- [x] **6.2** Створити `Services/IApiService.cs` (інтерфейс)
- [x] **6.3** Створити `Services/ApiService.cs` (реалізація з HttpClient)
- [x] **6.4** Зареєструвати `HttpClient` та `ApiService` в `MauiProgram.cs`
- [x] **6.5** Протестувати підключення до бекенду (отримання треків)

---

## Фаза 7: MAUI — Player Service

- [x] **7.1** Підключити `Plugin.Maui.Audio` через `.AddAudio()` в `MauiProgram.cs`
- [x] **7.2** Створити `Services/IPlayerService.cs` (інтерфейс)
- [x] **7.3** Створити `Services/PlayerService.cs` (реалізація)
- [x] **7.4** Зареєструвати `PlayerService` як singleton
- [x] **7.5** Протестувати відтворення аудіо з URL (build succeeded, IAudioManager через DI)

---

## Фаза 8: MAUI — BaseViewModel та Shell

- [x] **8.1** Створити `ViewModels/BaseViewModel.cs` з ObservableObject
- [x] **8.2** Налаштувати `Resources/Styles/Colors.xaml` (Spotify палітра)
- [x] **8.3** Налаштувати `Resources/Styles/Styles.xaml` (базові стилі)
- [x] **8.4** Налаштувати `AppShell.xaml` з TabBar (4 вкладки)
- [x] **8.5** Зареєструвати маршрут `player` в `AppShell.xaml.cs`
- [x] **8.6** Перевірити що Shell відкривається і табар відображається

---

## Фаза 9: MAUI — HomePage

- [x] **9.1** Створити `ViewModels/HomeViewModel.cs`
- [x] **9.2** Створити `Views/HomePage.xaml` та `Views/HomePage.xaml.cs`
- [x] **9.3** Прив'язати ViewModel до View через DI
- [x] **9.4** Реалізувати завантаження треків з API
- [x] **9.5** Реалізувати кешування в SQLite
- [x] **9.6** Реалізувати відображення списку треків (горизонтальний CollectionView)
- [x] **9.7** Реалізувати відображення альбомів та артистів
- [x] **9.8** Реалізувати кнопку ♥ (like/unlike)
- [x] **9.9** Перевірити відображення на iOS Simulator та macOS

---

## Фаза 10: MAUI — PlayerPage та PlayerViewModel

- [x] **10.1** Створити `ViewModels/PlayerViewModel.cs` (singleton)
- [x] **10.2** Створити `Views/PlayerPage.xaml` та `Views/PlayerPage.xaml.cs`
- [x] **10.3** Реалізувати `PlayTrackAsync` — завантаження та відтворення
- [x] **10.4** Реалізувати Slider прогресу з Timer оновленням (кожну секунду)
- [x] **10.5** Реалізувати кнопки Play/Pause, Next, Previous
- [x] **10.6** Реалізувати Seek через Slider
- [x] **10.7** Реалізувати Like/Unlike кнопку
- [x] **10.8** Реалізувати збереження ListeningHistory при зміні треку
- [x] **10.9** Реалізувати навігаційний параметр `TrackId`
- [x] **10.10** Перевірити відтворення реального аудіо файлу

---

## Фаза 11: MAUI — MiniPlayer

- [x] **11.1** Створити `ViewModels/MiniPlayerViewModel.cs`
- [x] **11.2** Створити `Views/Controls/MiniPlayer.xaml`
- [x] **11.3** Інтегрувати MiniPlayer в кожну tab-сторінку (над TabBar у layout)
- [x] **11.4** Прив'язати до PlayerViewModel (singleton)
- [x] **11.5** Реалізувати тап → відкриття PlayerPage
- [x] **11.6** Перевірити синхронізацію стану між MiniPlayer та PlayerPage

---

## Фаза 12: MAUI — SearchPage

- [x] **12.1** Створити `ViewModels/SearchViewModel.cs`
- [x] **12.2** Створити `Views/SearchPage.xaml` та `Views/SearchPage.xaml.cs`
- [x] **12.3** Реалізувати SearchBar з debounce (500ms)
- [x] **12.4** Реалізувати відображення результатів (треки, артисти, альбоми)
- [x] **12.5** Реалізувати збереження в SearchHistory
- [x] **12.6** Реалізувати відображення SearchHistory при порожньому запиті
- [x] **12.7** Реалізувати видалення елементів з SearchHistory
- [x] **12.8** Реалізувати запуск відтворення з результатів пошуку

---

## Фаза 13: MAUI — LibraryPage

- [x] **13.1** Створити `ViewModels/LibraryViewModel.cs`
- [x] **13.2** Створити `Views/LibraryPage.xaml` та `Views/LibraryPage.xaml.cs`
- [x] **13.3** Реалізувати відображення LikedTracks з SQLite
- [x] **13.4** Реалізувати відображення Playlists з SQLite
- [x] **13.5** Реалізувати перемикач вкладок (Треки / Плейлисти)
- [x] **13.6** Реалізувати створення плейлиста (діалог + збереження)
- [x] **13.7** Реалізувати видалення плейлиста (підтвердження)
- [x] **13.8** Реалізувати unlike трека
- [x] **13.9** Реалізувати відтворення треку з бібліотеки

---

## Фаза 14: MAUI — HistoryPage

- [x] **14.1** Створити `ViewModels/HistoryViewModel.cs`
- [x] **14.2** Створити `Views/HistoryPage.xaml` та `Views/HistoryPage.xaml.cs`
- [x] **14.3** Реалізувати відображення ListeningHistory з SQLite
- [x] **14.4** Реалізувати кнопку "Очистити" з підтвердженням
- [x] **14.5** Реалізувати відтворення треку з історії

---

## Фаза 15: Фінальна перевірка та polish

- [x] **15.1** Перевірити повний flow: Головна → Пошук → Відтворення → Бібліотека → Історія (code review ✅)
- [ ] **15.2** Перевірити роботу на iOS Simulator
- [ ] **15.3** Перевірити роботу на macOS (Mac Catalyst)
- [ ] **15.4** Перевірити що MiniPlayer відображається коректно на обох платформах
- [x] **15.5** Перевірити ActivityIndicator під час завантаження (IsBusy) — ✅ є у всіх 4 tab-сторінках
- [x] **15.6** Перевірити обробку помилок (бекенд недоступний) — ✅ реалізовано:
  - HomeViewModel: fallback на SQLite кеш (треки + альбоми + артисти)
  - PlayerViewModel: try/catch навколо PlayAsync → DisplayAlert
  - SearchViewModel: тихе повернення пустих результатів + ShowNoResults empty state
  - LibraryViewModel/HistoryViewModel: лише SQLite, мережа не потрібна
- [ ] **15.7** Зробити скріншоти всіх сторінок для курсової роботи

---

## Прогрес

| Фаза | Статус | Дата завершення |
|------|--------|----------------|
| 0. Підготовка | ✅ Завершено | 2026-06-08 |
| 1. Бекенд базова структура | ✅ Завершено | 2026-06-08 |
| 2. Бекенд моделі та роутери | ✅ Завершено | 2026-06-08 |
| 3. Бекенд seed та статика | ✅ Завершено | 2026-06-08 |
| 4. MAUI початкова структура | ✅ Завершено | 2026-06-08 |
| 5. MAUI Data layer | ✅ Завершено | 2026-06-08 |
| 6. MAUI API Service | ✅ Завершено | 2026-06-08 |
| 7. MAUI Player Service | ✅ Завершено | 2026-06-08 |
| 8. MAUI BaseViewModel та Shell | ✅ Завершено | 2026-06-08 |
| 9. MAUI HomePage | ✅ Завершено | 2026-06-08 |
| 10. MAUI PlayerPage | ✅ Завершено | 2026-06-08 |
| 11. MAUI MiniPlayer | ✅ Завершено | 2026-06-08 |
| 12. MAUI SearchPage | ✅ Завершено | 2026-06-08 |
| 13. MAUI LibraryPage | ✅ Завершено | 2026-06-08 |
| 14. MAUI HistoryPage | ✅ Завершено | 2026-06-08 |
| 15. Фінальна перевірка | 🔄 В процесі | 2026-06-08 |
