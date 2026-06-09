from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    MONGODB_URI: str = "mongodb://localhost:27017"
    DB_NAME: str = "spoticlone"
    BASE_URL: str = "http://localhost:8000"
    AUDIO_DIR: str = "static/audio"
    COVERS_DIR: str = "static/covers"


settings = Settings()
