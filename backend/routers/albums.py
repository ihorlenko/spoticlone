from fastapi import APIRouter, HTTPException, Query
from bson import ObjectId
from database import albums_collection, tracks_collection
from models.album import AlbumResponse
from models.track import TrackResponse
from utils import build_track, build_album
from config import settings

router = APIRouter()

_ARTIST_LOOKUP = [
    {"$lookup": {"from": "artists", "localField": "artist_id", "foreignField": "_id", "as": "artist"}},
    {"$unwind": {"path": "$artist", "preserveNullAndEmptyArrays": True}},
]

_TRACK_ARTIST_LOOKUP = [
    {"$lookup": {"from": "artists", "localField": "artist_id", "foreignField": "_id", "as": "artist"}},
    {"$unwind": {"path": "$artist", "preserveNullAndEmptyArrays": True}},
]


@router.get("", response_model=list[AlbumResponse])
async def get_albums(
    limit: int = Query(20, ge=1, le=100),
    skip: int = Query(0, ge=0),
):
    pipeline = _ARTIST_LOOKUP + [{"$skip": skip}, {"$limit": limit}]
    docs = await albums_collection.aggregate(pipeline).to_list(length=limit)
    return [AlbumResponse(**build_album(doc)) for doc in docs]


@router.get("/{album_id}", response_model=AlbumResponse)
async def get_album(album_id: str):
    try:
        obj_id = ObjectId(album_id)
    except Exception:
        raise HTTPException(status_code=422, detail="Invalid album ID")

    pipeline = [{"$match": {"_id": obj_id}}] + _ARTIST_LOOKUP + [{"$limit": 1}]
    docs = await albums_collection.aggregate(pipeline).to_list(length=1)
    if not docs:
        raise HTTPException(status_code=404, detail="Album not found")

    doc = docs[0]
    cover_filename = doc.get("cover_filename", "")
    cover_url = f"{settings.BASE_URL}/static/covers/{cover_filename}"

    track_docs = await tracks_collection.aggregate(
        [{"$match": {"album_id": obj_id}}] + _TRACK_ARTIST_LOOKUP
    ).to_list(length=100)

    tracks = []
    for t in track_docs:
        track_dict = build_track(t)
        track_dict["album_title"] = doc["title"]
        track_dict["cover_url"] = cover_url if cover_filename else None
        tracks.append(TrackResponse(**track_dict))

    result = build_album(doc)
    result["tracks"] = tracks
    return AlbumResponse(**result)
