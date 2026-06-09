# BACKEND.md — FastAPI + MongoDB специфікація

## Структура проєкту

```
backend/
├── main.py              # FastAPI app, CORS, static files, router підключення
├── config.py            # Конфігурація (MongoDB URI, BASE_URL тощо)
├── database.py          # Motor client, db instance, колекції
├── models/
│   ├── track.py         # Pydantic схеми для Track
│   ├── artist.py        # Pydantic схеми для Artist
│   └── album.py         # Pydantic схеми для Album
├── routers/
│   ├── tracks.py        # /tracks ендпоінти
│   ├── artists.py       # /artists ендпоінти
│   ├── albums.py        # /albums ендпоінти
│   └── search.py        # /search ендпоінт
├── seed.py              # Скрипт заповнення MongoDB тестовими даними
├── static/
│   ├── audio/           # MP3 файли (додаються вручну)
│   └── covers/          # JPG/PNG обкладинки (додаються вручну)
└── requirements.txt
```

---

## Залежності (requirements.txt)

```
fastapi>=0.110.0
motor>=3.3.0
pydantic>=2.0.0
uvicorn[standard]>=0.27.0
python-multipart>=0.0.9
aiofiles>=23.0.0
```

---

## Конфігурація (config.py)

```python
from pydantic_settings import BaseSettings

class Settings(BaseSettings):
    MONGODB_URI: str = "mongodb://localhost:27017"
    DB_NAME: str = "spoticlone"
    BASE_URL: str = "http://localhost:8000"
    AUDIO_DIR: str = "static/audio"
    COVERS_DIR: str = "static/covers"

settings = Settings()
```

---

## База даних (database.py)

```python
from motor.motor_asyncio import AsyncIOMotorClient
from config import settings

client = AsyncIOMotorClient(settings.MONGODB_URI)
db = client[settings.DB_NAME]

tracks_collection = db["tracks"]
artists_collection = db["artists"]
albums_collection = db["albums"]
```

### MongoDB індекси (створюються при старті)
```python
# В main.py, on_startup:
await tracks_collection.create_index([("title", "text"), ("genre", "text")])
await artists_collection.create_index([("name", "text")])
await albums_collection.create_index([("title", "text")])
await tracks_collection.create_index("artist_id")
await tracks_collection.create_index("album_id")
await albums_collection.create_index("artist_id")
```

---

## MongoDB схеми документів

### Collection: `tracks`
```json
{
  "_id": "ObjectId (auto)",
  "title": "string",
  "artist_id": "string (ObjectId ref)",
  "album_id": "string (ObjectId ref)",
  "duration_ms": "integer",
  "audio_filename": "string (e.g. 'track1.mp3')",
  "genre": "string",
  "created_at": "datetime"
}
```

### Collection: `artists`
```json
{
  "_id": "ObjectId (auto)",
  "name": "string",
  "cover_filename": "string (e.g. 'artist1.jpg')",
  "created_at": "datetime"
}
```

### Collection: `albums`
```json
{
  "_id": "ObjectId (auto)",
  "title": "string",
  "artist_id": "string (ObjectId ref)",
  "cover_filename": "string (e.g. 'album1.jpg')",
  "year": "integer",
  "created_at": "datetime"
}
```

---

## Pydantic моделі

### Track (models/track.py)
```python
from pydantic import BaseModel, Field
from typing import Optional
from datetime import datetime

class TrackResponse(BaseModel):
    id: str = Field(alias="_id")
    title: str
    artist_id: str
    album_id: str
    duration_ms: int
    audio_url: str          # Повний URL: BASE_URL/static/audio/{filename}
    genre: str
    created_at: datetime

    # Денормалізовані поля для зручності клієнта
    artist_name: Optional[str] = None
    album_title: Optional[str] = None
    cover_url: Optional[str] = None  # URL обкладинки альбому

    model_config = {"populate_by_name": True}
```

### Artist (models/artist.py)
```python
class ArtistResponse(BaseModel):
    id: str = Field(alias="_id")
    name: str
    cover_url: str          # Повний URL: BASE_URL/static/covers/{filename}
    created_at: datetime

    model_config = {"populate_by_name": True}
```

### Album (models/album.py)
```python
class AlbumResponse(BaseModel):
    id: str = Field(alias="_id")
    title: str
    artist_id: str
    artist_name: Optional[str] = None
    cover_url: str          # Повний URL: BASE_URL/static/covers/{filename}
    year: int
    created_at: datetime
    tracks: Optional[list[TrackResponse]] = None  # Лише в GET /albums/{id}

    model_config = {"populate_by_name": True}
```

### Search
```python
class SearchResponse(BaseModel):
    tracks: list[TrackResponse]
    artists: list[ArtistResponse]
    albums: list[AlbumResponse]
    query: str
    total_results: int
```

---

## API ендпоінти

### Tracks — `/tracks`

#### `GET /tracks`
Повертає список усіх треків з денормалізованими даними артиста та альбому.

**Query params:**
- `limit: int = 20` — кількість результатів
- `skip: int = 0` — зміщення (пагінація)
- `genre: str = None` — фільтр по жанру

**Response:** `200 OK`
```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "title": "Назва треку",
    "artist_id": "507f1f77bcf86cd799439012",
    "album_id": "507f1f77bcf86cd799439013",
    "duration_ms": 213000,
    "audio_url": "http://localhost:8000/static/audio/track1.mp3",
    "genre": "Pop",
    "created_at": "2024-01-01T00:00:00Z",
    "artist_name": "Ім'я артиста",
    "album_title": "Назва альбому",
    "cover_url": "http://localhost:8000/static/covers/album1.jpg"
  }
]
```

**Реалізація (агрегація MongoDB):**
```python
pipeline = [
    {"$lookup": {"from": "artists", "localField": "artist_id", "foreignField": "_id", "as": "artist"}},
    {"$lookup": {"from": "albums", "localField": "album_id", "foreignField": "_id", "as": "album"}},
    {"$unwind": {"path": "$artist", "preserveNullAndEmptyArrays": True}},
    {"$unwind": {"path": "$album", "preserveNullAndEmptyArrays": True}},
    {"$skip": skip},
    {"$limit": limit}
]
```

---

#### `GET /tracks/{track_id}`
Повертає один трек за ID.

**Response:** `200 OK` — об'єкт `TrackResponse`
**Error:** `404 Not Found` — `{"detail": "Track not found"}`

---

### Artists — `/artists`

#### `GET /artists`
Повертає список усіх артистів.

**Query params:**
- `limit: int = 20`
- `skip: int = 0`

**Response:** `200 OK` — масив `ArtistResponse`

---

#### `GET /artists/{artist_id}`
Повертає артиста та його треки.

**Response:** `200 OK`
```json
{
  "id": "...",
  "name": "Ім'я артиста",
  "cover_url": "http://localhost:8000/static/covers/artist1.jpg",
  "created_at": "...",
  "tracks": [ ...TrackResponse ],
  "albums": [ ...AlbumResponse ]
}
```

---

### Albums — `/albums`

#### `GET /albums`
Повертає список усіх альбомів з іменем артиста.

**Query params:**
- `limit: int = 20`
- `skip: int = 0`

**Response:** `200 OK` — масив `AlbumResponse` (без поля `tracks`)

---

#### `GET /albums/{album_id}`
Повертає альбом із повним списком треків.

**Response:** `200 OK` — об'єкт `AlbumResponse` (з полем `tracks`)

---

### Search — `/search`

#### `GET /search`
Повнотекстовий пошук по трекам, артистам та альбомам одночасно.

**Query params:**
- `q: str` — пошуковий запит (обов'язковий, мінімум 1 символ)
- `limit: int = 10` — ліміт для кожної категорії

**Response:** `200 OK`
```json
{
  "tracks": [ ...TrackResponse ],
  "artists": [ ...ArtistResponse ],
  "albums": [ ...AlbumResponse ],
  "query": "the weeknd",
  "total_results": 5
}
```

**Реалізація:** MongoDB `$text` search по відповідних колекціях паралельно через `asyncio.gather`.

---

### Static Files

FastAPI монтує статичні файли:
```python
app.mount("/static", StaticFiles(directory="static"), name="static")
```

**Аудіо стримінг:**
FastAPI StaticFiles автоматично підтримує HTTP Range requests — браузер/клієнт може запитувати часткові відрізки файлу. Це забезпечує:
- Перемотування треку без повного завантаження
- Відновлення відтворення після паузи
- Коректну роботу `Plugin.Maui.Audio` з HTTP URLs

**URL патерни:**
```
http://localhost:8000/static/audio/{filename}.mp3
http://localhost:8000/static/covers/{filename}.jpg
```

---

## main.py структура

```python
from fastapi import FastAPI
from fastapi.staticfiles import StaticFiles
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager
from database import tracks_collection, artists_collection, albums_collection
from routers import tracks, artists, albums, search

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Створення індексів при старті
    await tracks_collection.create_index([("title", "text"), ("genre", "text")])
    await artists_collection.create_index([("name", "text")])
    await albums_collection.create_index([("title", "text")])
    yield

app = FastAPI(title="SpotiClone API", lifespan=lifespan)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

app.mount("/static", StaticFiles(directory="static"), name="static")

app.include_router(tracks.router, prefix="/tracks", tags=["tracks"])
app.include_router(artists.router, prefix="/artists", tags=["artists"])
app.include_router(albums.router, prefix="/albums", tags=["albums"])
app.include_router(search.router, prefix="/search", tags=["search"])
```

---

## Seed скрипт (seed.py)

Скрипт очищає MongoDB та заповнює тестовими даними. Аудіофайли та обкладинки користувач кладе вручну.

**Структура seed даних:**
- 3–5 артистів
- 5–8 альбомів
- 10 треків
- Жанри: Pop, Rock, Electronic, Hip-Hop, Jazz

**Запуск:**
```bash
python seed.py
```

Скрипт перевіряє наявність файлів у `static/audio/` та `static/covers/` і попереджає якщо вони відсутні, але не зупиняється — метадані все одно записуються.

---

## Обробка помилок

| Ситуація | HTTP статус | Відповідь |
|----------|-------------|-----------|
| ID не знайдено | 404 | `{"detail": "Not found"}` |
| Невалідний ObjectId | 422 | Pydantic validation error |
| MongoDB недоступна | 503 | `{"detail": "Database unavailable"}` |
| Порожній пошуковий запит | 422 | Validation error |

---

## Конвертація ObjectId

MongoDB використовує `ObjectId` для `_id`. При серіалізації конвертуй в рядок:
```python
def serialize_doc(doc: dict) -> dict:
    """Convert MongoDB document ObjectId fields to strings."""
    doc["_id"] = str(doc["_id"])
    if "artist_id" in doc:
        doc["artist_id"] = str(doc["artist_id"])
    if "album_id" in doc:
        doc["album_id"] = str(doc["album_id"])
    return doc
```
