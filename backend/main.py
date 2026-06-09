from contextlib import asynccontextmanager
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles
from database import tracks_collection, artists_collection, albums_collection
from routers import tracks, artists, albums, search


@asynccontextmanager
async def lifespan(app: FastAPI):
    await tracks_collection.create_index([("title", "text"), ("genre", "text")])
    await artists_collection.create_index([("name", "text")])
    await albums_collection.create_index([("title", "text")])
    await tracks_collection.create_index("artist_id")
    await tracks_collection.create_index("album_id")
    await albums_collection.create_index("artist_id")
    yield


app = FastAPI(title="SpotiClone API", lifespan=lifespan)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

app.mount("/static", StaticFiles(directory="static"), name="static")

app.include_router(tracks.router, prefix="/tracks", tags=["tracks"])
app.include_router(artists.router, prefix="/artists", tags=["artists"])
app.include_router(albums.router, prefix="/albums", tags=["albums"])
app.include_router(search.router, prefix="/search", tags=["search"])
