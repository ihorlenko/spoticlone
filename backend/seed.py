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
    artist_bowie_id = ObjectId()
    artist_greenday_id = ObjectId()
    artist_elton_id = ObjectId()
    artist_whitney_id = ObjectId()

    artists = [
        {
            "_id": artist_mj_id,
            "name": "Michael Jackson",
            "cover_filename": "michael_jackson_thriller.jpg",
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
        {
            "_id": artist_bowie_id,
            "name": "David Bowie",
            "cover_filename": "David_Bowie_Space_Oddity.jpg",
            "created_at": now(),
        },
        {
            "_id": artist_greenday_id,
            "name": "Green Day",
            "cover_filename": "Green_Day_Wake_Me_Up_When_September_Ends.jpg",
            "created_at": now(),
        },
        {
            "_id": artist_elton_id,
            "name": "Elton John",
            "cover_filename": "elton_john_rocket_man.jpg",
            "created_at": now(),
        },
        {
            "_id": artist_whitney_id,
            "name": "Whitney Houston",
            "cover_filename": "Whitney_Houston_I_Wanna_Dance_with_Somebody.jpg",
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
    album_space_oddity_id = ObjectId()
    album_ziggy_id = ObjectId()
    album_american_idiot_id = ObjectId()
    album_21cb_id = ObjectId()
    album_honky_id = ObjectId()
    album_whitney_id = ObjectId()

    albums = [
        {
            "_id": album_thriller_id,
            "title": "Thriller",
            "artist_id": artist_mj_id,
            "cover_filename": "michael_jackson_thriller.jpg",
            "year": 1982,
            "created_at": now(),
        },
        {
            "_id": album_no_more_tears_id,
            "title": "No More Tears",
            "artist_id": artist_ozzy_id,
            "cover_filename": "Ozzy_Osbourne_no_more_tears.jpg",
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
        {
            "_id": album_space_oddity_id,
            "title": "Space Oddity",
            "artist_id": artist_bowie_id,
            "cover_filename": "David_Bowie_Space_Oddity.jpg",
            "year": 1969,
            "created_at": now(),
        },
        {
            "_id": album_ziggy_id,
            "title": "The Rise and Fall of Ziggy Stardust",
            "artist_id": artist_bowie_id,
            "cover_filename": "David_Bowie_Starman.jpg",
            "year": 1972,
            "created_at": now(),
        },
        {
            "_id": album_american_idiot_id,
            "title": "American Idiot",
            "artist_id": artist_greenday_id,
            "cover_filename": "Green_Day_Wake_Me_Up_When_September_Ends.jpg",
            "year": 2004,
            "created_at": now(),
        },
        {
            "_id": album_21cb_id,
            "title": "21st Century Breakdown",
            "artist_id": artist_greenday_id,
            "cover_filename": "GreenDay_21Guns.jpeg",
            "year": 2009,
            "created_at": now(),
        },
        {
            "_id": album_honky_id,
            "title": "Honky Château",
            "artist_id": artist_elton_id,
            "cover_filename": "elton_john_rocket_man.jpg",
            "year": 1972,
            "created_at": now(),
        },
        {
            "_id": album_whitney_id,
            "title": "Whitney",
            "artist_id": artist_whitney_id,
            "cover_filename": "Whitney_Houston_I_Wanna_Dance_with_Somebody.jpg",
            "year": 1987,
            "created_at": now(),
        },
    ]

    await albums_col.insert_many(albums)
    print(f"Inserted {len(albums)} albums.")

    # ── Tracks ───────────────────────────────────────────────────────────────
    tracks = [
        # Michael Jackson — Thriller
        {
            "_id": ObjectId(),
            "title": "Thriller",
            "artist_id": artist_mj_id,
            "album_id": album_thriller_id,
            "duration_ms": 358000,
            "audio_filename": "michael_jackson_thriller.mp3",
            "genre": "Pop",
            "created_at": now(),
        },
        {
            "_id": ObjectId(),
            "title": "Beat It",
            "artist_id": artist_mj_id,
            "album_id": album_thriller_id,
            "duration_ms": 258000,
            "audio_filename": "michael_jackson_beat_it.mp3",
            "genre": "Pop",
            "created_at": now(),
        },
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
        # Ozzy Osbourne — No More Tears
        {
            "_id": ObjectId(),
            "title": "No More Tears",
            "artist_id": artist_ozzy_id,
            "album_id": album_no_more_tears_id,
            "duration_ms": 444000,
            "audio_filename": "Ozzy_Osbourne_no_more_tears.mp3",
            "genre": "Rock",
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
            "title": "I Don't Want to Change the World",
            "artist_id": artist_ozzy_id,
            "album_id": album_no_more_tears_id,
            "duration_ms": 234000,
            "audio_filename": "Ozzy_Osbourne_i_dont_want_to_change_the_world.mp3",
            "genre": "Rock",
            "created_at": now(),
        },
        # Queen — A Night at the Opera
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
        {
            "_id": ObjectId(),
            "title": "Love of My Life",
            "artist_id": artist_queen_id,
            "album_id": album_night_at_opera_id,
            "duration_ms": 213000,
            "audio_filename": "queen_love_of_my_life.mp3",
            "genre": "Rock",
            "created_at": now(),
        },
        {
            "_id": ObjectId(),
            "title": "You're My Best Friend",
            "artist_id": artist_queen_id,
            "album_id": album_night_at_opera_id,
            "duration_ms": 170000,
            "audio_filename": "queen_youre_my_best_friend.mp3",
            "genre": "Rock",
            "created_at": now(),
        },
        # David Bowie — Space Oddity
        {
            "_id": ObjectId(),
            "title": "Space Oddity",
            "artist_id": artist_bowie_id,
            "album_id": album_space_oddity_id,
            "duration_ms": 315000,
            "audio_filename": "David_Bowie_Space_Oddity.mp3",
            "genre": "Rock",
            "created_at": now(),
        },
        # David Bowie — Ziggy Stardust
        {
            "_id": ObjectId(),
            "title": "Starman",
            "artist_id": artist_bowie_id,
            "album_id": album_ziggy_id,
            "duration_ms": 257000,
            "audio_filename": "David_Bowie_Starman.mp3",
            "genre": "Rock",
            "created_at": now(),
        },
        # Green Day — American Idiot
        {
            "_id": ObjectId(),
            "title": "Boulevard of Broken Dreams",
            "artist_id": artist_greenday_id,
            "album_id": album_american_idiot_id,
            "duration_ms": 263000,
            "audio_filename": "Greenday_Boulevard_of_Broken_Dreams.mp3",
            "genre": "Punk Rock",
            "created_at": now(),
        },
        {
            "_id": ObjectId(),
            "title": "Wake Me Up When September Ends",
            "artist_id": artist_greenday_id,
            "album_id": album_american_idiot_id,
            "duration_ms": 285000,
            "audio_filename": "Green_Day_Wake_Me_Up_When_September_Ends.mp3",
            "genre": "Punk Rock",
            "created_at": now(),
        },
        # Green Day — 21st Century Breakdown
        {
            "_id": ObjectId(),
            "title": "21 Guns",
            "artist_id": artist_greenday_id,
            "album_id": album_21cb_id,
            "duration_ms": 330000,
            "audio_filename": "GreenDay_21Guns.mp3",
            "genre": "Punk Rock",
            "created_at": now(),
        },
        {
            "_id": ObjectId(),
            "title": "Know Your Enemy",
            "artist_id": artist_greenday_id,
            "album_id": album_21cb_id,
            "duration_ms": 198000,
            "audio_filename": "GreenDay_Know_Your_Enemy.mp3",
            "genre": "Punk Rock",
            "created_at": now(),
        },
        # Elton John — Honky Château
        {
            "_id": ObjectId(),
            "title": "Rocket Man",
            "artist_id": artist_elton_id,
            "album_id": album_honky_id,
            "duration_ms": 249000,
            "audio_filename": "elton_john_rocket_man.mp3",
            "genre": "Pop",
            "created_at": now(),
        },
        # Whitney Houston — Whitney
        {
            "_id": ObjectId(),
            "title": "I Wanna Dance with Somebody",
            "artist_id": artist_whitney_id,
            "album_id": album_whitney_id,
            "duration_ms": 291000,
            "audio_filename": "Whitney_Houston_I_Wanna_Dance_with_Somebody.mp3",
            "genre": "Pop",
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
