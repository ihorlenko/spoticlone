from typing import Optional
from fastapi import APIRouter, HTTPException, Query
from bson import ObjectId
from database import tracks_collection
from models.track import TrackResponse
from utils import build_track

router = APIRouter()

_LOOKUP_PIPELINE = [
    {"$lookup": {"from": "artists", "localField": "artist_id", "foreignField": "_id", "as": "artist"}},
    {"$lookup": {"from": "albums", "localField": "album_id", "foreignField": "_id", "as": "album"}},
    {"$unwind": {"path": "$artist", "preserveNullAndEmptyArrays": True}},
    {"$unwind": {"path": "$album", "preserveNullAndEmptyArrays": True}},
]


@router.get("", response_model=list[TrackResponse])
async def get_tracks(
    limit: int = Query(20, ge=1, le=100),
    skip: int = Query(0, ge=0),
    genre: Optional[str] = None,
):
    pipeline = []
    if genre:
        pipeline.append({"$match": {"genre": genre}})
    pipeline = pipeline + _LOOKUP_PIPELINE + [{"$skip": skip}, {"$limit": limit}]
    docs = await tracks_collection.aggregate(pipeline).to_list(length=limit)
    return [TrackResponse(**build_track(doc)) for doc in docs]


@router.get("/{track_id}", response_model=TrackResponse)
async def get_track(track_id: str):
    try:
        obj_id = ObjectId(track_id)
    except Exception:
        raise HTTPException(status_code=422, detail="Invalid track ID")
    pipeline = [{"$match": {"_id": obj_id}}] + _LOOKUP_PIPELINE + [{"$limit": 1}]
    docs = await tracks_collection.aggregate(pipeline).to_list(length=1)
    if not docs:
        raise HTTPException(status_code=404, detail="Track not found")
    return TrackResponse(**build_track(docs[0]))
