"""Seed script: clears MongoDB and inserts test data matching files in static/."""
import asyncio
import os
from datetime import datetime, timezone

from bson import ObjectId
from motor.motor_asyncio import AsyncIOMotorClient

MONGODB_URI = os.getenv("MONGODB_URI", "mongodb://localhost:27017")
DB_NAME = os.getenv("DB_NAME", "spoticlone")

AUDIO_DIR = os.path.join(os.path.dirname(__file__), "static", "audio")
COVERS_DIR = os.path.join(os.path.dirname(__file__), "static", "covers")


def _warn_missing(path: str, label: str) -> None:
    if not os.path.exists(path):
        print(f"  WARNING: {label} not found at {path}")


def now() -> datetime:
    return datetime.now(timezone.utc)


async def seed() -> None:
    client = AsyncIOMotorClient(MONGODB_URI)
    db = client[DB_NAME]

    tracks_col = db["tracks"]
    artists_col = db["artists"]
    albums_col = db["albums"]

    # Clear existing data
    await tracks_col.delete_many({})
    await artists_col.delete_many({})
    await albums_col.delete_many({})
    print("Collections cleared.")

    # ── Artists ──────────────────────────────────────────────────────────────
    artist_mj_id = ObjectId()
    artist_ozzy_id = ObjectId()
    artist_queen_id = ObjectId()

    artists = [
        {
            "_id": artist_mj_id,
            "name": "Michael Jackson",
            "cover_filename": "michael-jackson_human-nature.jpg",
            "created_at": now(),
        },
        {
            "_id": artist_ozzy_id,
            "name": "Ozzy Osbourne",
            "cover_filename": "ozzy-osbourne_Mama-Im-Coming-Home.jpg",
            "created_at": now(),
        },
        {
            "_id": artist_queen_id,
            "name": "Queen",
            "cover_filename": "queen_bohemian-rhapsody.jpg",
            "created_at": now(),
        },
    ]

    for a in artists:
        _warn_missing(os.path.join(COVERS_DIR, a["cover_filename"]), f"Cover '{a['cover_filename']}'")

    await artists_col.insert_many(artists)
    print(f"Inserted {len(artists)} artists.")

    # ── Albums ───────────────────────────────────────────────────────────────
    album_thriller_id = ObjectId()
    album_no_more_tears_id = ObjectId()
    album_night_at_opera_id = ObjectId()

    albums = [
        {
            "_id": album_thriller_id,
            "title": "Thriller",
            "artist_id": artist_mj_id,
            "cover_filename": "michael-jackson_human-nature.jpg",
            "year": 1982,
            "created_at": now(),
        },
        {
            "_id": album_no_more_tears_id,
            "title": "No More Tears",
            "artist_id": artist_ozzy_id,
            "cover_filename": "ozzy-osbourne_Mama-Im-Coming-Home.jpg",
            "year": 1991,
            "created_at": now(),
        },
        {
            "_id": album_night_at_opera_id,
            "title": "A Night at the Opera",
            "artist_id": artist_queen_id,
            "cover_filename": "queen_bohemian-rhapsody.jpg",
            "year": 1975,
            "created_at": now(),
        },
    ]

    await albums_col.insert_many(albums)
    print(f"Inserted {len(albums)} albums.")

    # ── Tracks ───────────────────────────────────────────────────────────────
    tracks = [
        {
            "_id": ObjectId(),
            "title": "Human Nature",
            "artist_id": artist_mj_id,
            "album_id": album_thriller_id,
            "duration_ms": 245000,
            "audio_filename": "michael-jackson_human-nature.mp3",
            "genre": "Pop",
            "created_at": now(),
        },
        {
            "_id": ObjectId(),
            "title": "Mama, I'm Coming Home",
            "artist_id": artist_ozzy_id,
            "album_id": album_no_more_tears_id,
            "duration_ms": 261000,
            "audio_filename": "ozzy-osbourne_Mama-Im-Coming-Home.mp3",
            "genre": "Rock",
            "created_at": now(),
        },
        {
            "_id": ObjectId(),
            "title": "Bohemian Rhapsody",
            "artist_id": artist_queen_id,
            "album_id": album_night_at_opera_id,
            "duration_ms": 355000,
            "audio_filename": "queen_bohemian-rhapsody.mp3",
            "genre": "Rock",
            "created_at": now(),
        },
    ]

    for t in tracks:
        _warn_missing(os.path.join(AUDIO_DIR, t["audio_filename"]), f"Audio '{t['audio_filename']}'")

    await tracks_col.insert_many(tracks)
    print(f"Inserted {len(tracks)} tracks.")

    # ── Indexes ──────────────────────────────────────────────────────────────
    await tracks_col.create_index([("title", "text"), ("genre", "text")])
    await artists_col.create_index([("name", "text")])
    await albums_col.create_index([("title", "text")])
    await tracks_col.create_index("artist_id")
    await tracks_col.create_index("album_id")
    await albums_col.create_index("artist_id")
    print("Indexes created.")

    client.close()
    print("\nDone! Database seeded successfully.")


if __name__ == "__main__":
    asyncio.run(seed())
