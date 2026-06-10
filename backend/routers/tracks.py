import os
from pathlib import Path
from typing import Optional
from fastapi import APIRouter, HTTPException, Query
from fastapi.responses import PlainTextResponse
from bson import ObjectId
from database import tracks_collection
from models.track import TrackResponse
from utils import build_track

_LYRICS_DIR = Path(__file__).parent.parent / "static" / "lyrics"

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


@router.get("/{track_id}/lyrics", response_class=PlainTextResponse)
async def get_lyrics(track_id: str):
    # validate: must be a 24-char hex ObjectId to prevent path traversal
    if not ObjectId.is_valid(track_id):
        raise HTTPException(status_code=422, detail="Invalid track ID")
    lrc_path = _LYRICS_DIR / f"{track_id}.lrc"
    if not lrc_path.exists():
        raise HTTPException(status_code=404, detail="Lyrics not found")
    return lrc_path.read_text(encoding="utf-8")


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
