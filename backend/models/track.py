from pydantic import BaseModel, Field
from typing import Optional
from datetime import datetime


class TrackResponse(BaseModel):
    id: str = Field(alias="_id")
    title: str
    artist_id: str
    album_id: str
    duration_ms: int
    audio_url: str
    genre: str
    created_at: datetime
    artist_name: Optional[str] = None
    album_title: Optional[str] = None
    cover_url: Optional[str] = None

    model_config = {"populate_by_name": True}
