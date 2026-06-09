from pydantic import BaseModel, Field
from typing import Optional, List
from datetime import datetime
from models.track import TrackResponse
from models.artist import ArtistResponse


class AlbumResponse(BaseModel):
    id: str = Field(alias="_id")
    title: str
    artist_id: str
    artist_name: Optional[str] = None
    cover_url: str
    year: int
    created_at: datetime
    tracks: Optional[List[TrackResponse]] = None

    model_config = {"populate_by_name": True}


class SearchResponse(BaseModel):
    tracks: List[TrackResponse]
    artists: List[ArtistResponse]
    albums: List[AlbumResponse]
    query: str
    total_results: int
