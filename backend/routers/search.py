import asyncio
from fastapi import APIRouter, Query
from database import tracks_collection, artists_collection, albums_collection
from models.album import SearchResponse
from models.track import TrackResponse
from models.artist import ArtistResponse
from models.album import AlbumResponse
from utils import build_track, build_artist, build_album

router = APIRouter()

_TEXT_FILTER = lambda q: {"$text": {"$search": q}}

_TRACK_LOOKUP = [
    {"$lookup": {"from": "artists", "localField": "artist_id", "foreignField": "_id", "as": "artist"}},
    {"$lookup": {"from": "albums", "localField": "album_id", "foreignField": "_id", "as": "album"}},
    {"$unwind": {"path": "$artist", "preserveNullAndEmptyArrays": True}},
    {"$unwind": {"path": "$album", "preserveNullAndEmptyArrays": True}},
]

_ALBUM_LOOKUP = [
    {"$lookup": {"from": "artists", "localField": "artist_id", "foreignField": "_id", "as": "artist"}},
    {"$unwind": {"path": "$artist", "preserveNullAndEmptyArrays": True}},
]


@router.get("", response_model=SearchResponse)
async def search(
    q: str = Query(..., min_length=1),
    limit: int = Query(10, ge=1, le=50),
):
    text_match = {"$text": {"$search": q}}

    track_docs, artist_docs, album_docs = await asyncio.gather(
        tracks_collection.aggregate(
            [{"$match": text_match}] + _TRACK_LOOKUP + [{"$limit": limit}]
        ).to_list(length=limit),
        artists_collection.aggregate(
            [{"$match": text_match}, {"$limit": limit}]
        ).to_list(length=limit),
        albums_collection.aggregate(
            [{"$match": text_match}] + _ALBUM_LOOKUP + [{"$limit": limit}]
        ).to_list(length=limit),
    )

    tracks = [TrackResponse(**build_track(t)) for t in track_docs]
    artists = [ArtistResponse(**build_artist(a)) for a in artist_docs]
    albums = [AlbumResponse(**build_album(a)) for a in album_docs]

    return SearchResponse(
        tracks=tracks,
        artists=artists,
        albums=albums,
        query=q,
        total_results=len(tracks) + len(artists) + len(albums),
    )
