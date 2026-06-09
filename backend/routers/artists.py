from fastapi import APIRouter, HTTPException, Query
from bson import ObjectId
from database import artists_collection, tracks_collection, albums_collection
from models.artist import ArtistResponse
from models.track import TrackResponse
from models.album import AlbumResponse
from utils import build_track, build_artist, build_album

router = APIRouter()

_TRACK_LOOKUP_PIPELINE = [
    {"$lookup": {"from": "albums", "localField": "album_id", "foreignField": "_id", "as": "album"}},
    {"$unwind": {"path": "$album", "preserveNullAndEmptyArrays": True}},
]

_ALBUM_LOOKUP_PIPELINE = [
    {"$lookup": {"from": "artists", "localField": "artist_id", "foreignField": "_id", "as": "artist"}},
    {"$unwind": {"path": "$artist", "preserveNullAndEmptyArrays": True}},
]


@router.get("", response_model=list[ArtistResponse])
async def get_artists(
    limit: int = Query(20, ge=1, le=100),
    skip: int = Query(0, ge=0),
):
    docs = await artists_collection.find({}).skip(skip).limit(limit).to_list(length=limit)
    return [ArtistResponse(**build_artist(doc)) for doc in docs]


@router.get("/{artist_id}", response_model=ArtistResponse)
async def get_artist(artist_id: str):
    try:
        obj_id = ObjectId(artist_id)
    except Exception:
        raise HTTPException(status_code=422, detail="Invalid artist ID")

    doc = await artists_collection.find_one({"_id": obj_id})
    if not doc:
        raise HTTPException(status_code=404, detail="Artist not found")

    track_docs = await tracks_collection.aggregate(
        [{"$match": {"artist_id": obj_id}}] + _TRACK_LOOKUP_PIPELINE
    ).to_list(length=100)

    album_docs = await albums_collection.aggregate(
        [{"$match": {"artist_id": obj_id}}] + _ALBUM_LOOKUP_PIPELINE
    ).to_list(length=100)

    result = build_artist(doc)
    result["tracks"] = [TrackResponse(**{**build_track(t), "artist_name": doc["name"]}) for t in track_docs]
    result["albums"] = [AlbumResponse(**build_album(a)) for a in album_docs]

    return ArtistResponse(**result)
