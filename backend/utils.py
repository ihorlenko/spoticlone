from config import settings


def build_track(doc: dict) -> dict:
    """Build a TrackResponse-compatible dict from a MongoDB aggregate result."""
    artist = doc.get("artist") or {}
    album = doc.get("album") or {}
    audio_filename = doc.get("audio_filename", "")
    cover_filename = album.get("cover_filename", "")
    return {
        "_id": str(doc["_id"]),
        "title": doc["title"],
        "artist_id": str(doc.get("artist_id", "")),
        "album_id": str(doc.get("album_id", "")),
        "duration_ms": doc.get("duration_ms", 0),
        "audio_url": f"{settings.BASE_URL}/static/audio/{audio_filename}",
        "genre": doc.get("genre", ""),
        "created_at": doc["created_at"],
        "artist_name": artist.get("name"),
        "album_title": album.get("title"),
        "cover_url": f"{settings.BASE_URL}/static/covers/{cover_filename}" if cover_filename else None,
    }


def build_artist(doc: dict) -> dict:
    """Build an ArtistResponse-compatible dict from a MongoDB document."""
    cover_filename = doc.get("cover_filename", "")
    return {
        "_id": str(doc["_id"]),
        "name": doc["name"],
        "cover_url": f"{settings.BASE_URL}/static/covers/{cover_filename}",
        "created_at": doc["created_at"],
    }


def build_album(doc: dict) -> dict:
    """Build an AlbumResponse-compatible dict from a MongoDB aggregate result."""
    artist = doc.get("artist") or {}
    cover_filename = doc.get("cover_filename", "")
    return {
        "_id": str(doc["_id"]),
        "title": doc["title"],
        "artist_id": str(doc.get("artist_id", "")),
        "artist_name": artist.get("name"),
        "cover_url": f"{settings.BASE_URL}/static/covers/{cover_filename}",
        "year": doc.get("year", 0),
        "created_at": doc["created_at"],
    }
