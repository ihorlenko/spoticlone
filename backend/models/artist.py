from pydantic import BaseModel, Field
from typing import Optional, List, Any
from datetime import datetime


class ArtistResponse(BaseModel):
    id: str = Field(alias="_id")
    name: str
    cover_url: str
    created_at: datetime
    tracks: Optional[List[Any]] = None
    albums: Optional[List[Any]] = None

    model_config = {"populate_by_name": True}
