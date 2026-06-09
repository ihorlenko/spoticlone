# ARCHITECTURE.md — Загальна архітектура SpotiClone

## Огляд системи

SpotiClone — це кросплатформний стримінговий застосунок, що складається з двох незалежних компонентів: FastAPI-бекенду та .NET MAUI клієнта. Бекенд симулює стримінговий сервіс (як Spotify), клієнт — це мобільний/десктопний додаток.

```
┌─────────────────────────────────────────────────────────┐
│                    MAUI Client                          │
│  iOS / macOS (Mac Catalyst)                             │
│                                                         │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐             │
│  │  Views   │  │ViewModels│  │ Services │             │
│  │  (XAML)  │◄─│  (C#)    │◄─│  (C#)    │             │
│  └──────────┘  └──────────┘  └────┬─────┘             │
│                                    │                    │
│  ┌─────────────────────────────────▼──────────────┐   │
│  │              SQLite (local cache)               │   │
│  │  Track, Artist, Album, Playlist, PlaylistTrack  │   │
│  │  LikedTrack, ListeningHistory, SearchHistory    │   │
│  └─────────────────────────────────────────────────┘   │
└───────────────────────────┬─────────────────────────────┘
                            │ HTTP / REST
                            │ Audio streaming (Range requests)
┌───────────────────────────▼─────────────────────────────┐
│                   FastAPI Backend                        │
│                                                         │
│  ┌──────────┐  ┌──────────┐  ┌──────────────────────┐  │
│  │ Routers  │  │  Models  │  │   Static Files       │  │
│  │ /tracks  │  │ (Pydantic│  │   /static/audio/     │  │
│  │ /artists │  │  + Motor)│  │   /static/covers/    │  │
│  │ /albums  │  └──────────┘  └──────────────────────┘  │
│  │ /search  │                                           │
│  └────┬─────┘                                           │
│       │                                                 │
│  ┌────▼──────────────┐                                  │
│  │  MongoDB           │                                  │
│  │  db: spoticlone    │                                  │
│  │  collections:      │                                  │
│  │  - tracks          │                                  │
│  │  - artists         │                                  │
│  │  - albums          │                                  │
│  └────────────────────┘                                  │
└─────────────────────────────────────────────────────────┘
```

---

## Технологічний стек

### Бекенд
| Компонент | Технологія | Версія |
|-----------|-----------|--------|
| Framework | FastAPI | ≥ 0.110 |
| MongoDB driver | Motor (async) | ≥ 3.3 |
| Pydantic | v2 | ≥ 2.0 |
| Python | — | 3.11+ |
| Static files | FastAPI StaticFiles | — |

### Клієнт
| Компонент | Технологія | Версія |
|-----------|-----------|--------|
| Framework | .NET MAUI | .NET 10 |
| Мова | C# | 13 |
| UI | XAML + Shell | — |
| Патерн | MVVM | — |
| SQLite | sqlite-net-pcl | ≥ 1.9 |
| HTTP | HttpClient (DI) | — |
| Аудіо | Plugin.Maui.Audio | ≥ 3.0 |
| JSON | System.Text.Json | — |

---

## Принципи взаємодії компонентів

### Потік даних (Browse → Play)
```
1. Користувач відкриває Головну сторінку
2. ViewModel викликає ApiService.GetTracksAsync()
3. ApiService робить GET /tracks до FastAPI
4. FastAPI дістає документи з MongoDB
5. Відповідь: JSON масив об'єктів Track
6. ViewModel зберігає в кеш SQLite (через DatabaseService)
7. View відображає список треків

8. Користувач натискає на трек → Play
9. PlayerViewModel отримує URL: {BASE_URL}/static/audio/{filename}
10. Plugin.Maui.Audio стримує аудіо по HTTP
11. PlayerViewModel записує ListeningHistory в SQLite
```

### Потік даних (Like → Library)
```
1. Користувач натискає ♥ на треку
2. ViewModel викликає DatabaseService.AddLikedTrackAsync()
3. Трек зберігається в SQLite таблицю LikedTrack
4. Library сторінка читає LikedTrack з SQLite (локально)
```

### Потік даних (Search)
```
1. Користувач вводить запит
2. ViewModel викликає ApiService.SearchAsync(query)
3. ApiService робить GET /search?q={query} до FastAPI
4. FastAPI шукає по MongoDB (text index)
5. Результат повертається клієнту
6. ViewModel зберігає запит в SearchHistory (SQLite)
```

---

## Розподіл відповідальності: SQLite vs MongoDB

| Дані | Де зберігається | Чому |
|------|----------------|------|
| Треки, артисти, альбоми (метадані) | MongoDB (бекенд) | Контент сервісу, джерело правди |
| Кеш треків/артистів/альбомів | SQLite (клієнт) | Офлайн-доступ, швидкість |
| Плейлисти користувача | SQLite (клієнт) | Персональні дані користувача |
| Лайкнуті треки | SQLite (клієнт) | Персональні дані користувача |
| Історія прослуховувань | SQLite (клієнт) | Локальна аналітика |
| Історія пошуку | SQLite (клієнт) | Локальна зручність |
| Аудіофайли | Файлова система бекенду | Бінарні дані, стримінг |
| Обкладинки / фото | Файлова система бекенду | Статичні медіа |

---

## Безпека та конфігурація

- Авторизація відсутня (один анонімний користувач)
- CORS на бекенді: дозволено всі origins для локальної розробки
- Base URL бекенду зберігається в `appsettings.json` клієнта
- Жодних API ключів у коді — лише через змінні середовища або конфіг

---

## Локальна розробка

### Запуск бекенду
```bash
cd backend
pip install -r requirements.txt
uvicorn main:app --reload --host 0.0.0.0 --port 8000
```

### Запуск MongoDB
```bash
mongod --dbpath ./data/db
```

### Запуск MAUI (iOS Simulator)
```bash
# Переконайся що активний Xcode 26.2.0
xcode-select -p
dotnet build SpotiClone/SpotiClone.csproj -t:Run -f net10.0-ios
```

### Seed даних
```bash
cd backend
python seed.py  # Заповнює MongoDB тестовими даними
# Аудіофайли (.mp3) кладеш вручну в backend/static/audio/
# Обкладинки (.jpg/.png) кладеш вручну в backend/static/covers/
```
