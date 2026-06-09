from motor.motor_asyncio import AsyncIOMotorClient
from config import settings

client = AsyncIOMotorClient(settings.MONGODB_URI)
db = client[settings.DB_NAME]

tracks_collection = db["tracks"]
artists_collection = db["artists"]
albums_collection = db["albums"]
