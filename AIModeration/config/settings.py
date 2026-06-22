from pydantic_settings import BaseSettings
from functools import lru_cache
from typing import Optional
import os
import torch


class Settings(BaseSettings):
    """Application configuration loaded from environment variables."""
    
    # Server config
    APP_NAME: str = "AI Moderation Service"
    APP_VERSION: str = "1.0.0"
    DEBUG: bool = os.getenv("AI_DEBUG", "false").lower() == "true"
    
    # Server host/port
    HOST: str = os.getenv("AI_HOST", "0.0.0.0")
    PORT: int = int(os.getenv("AI_PORT", 8000))
    
    # Redis config
    REDIS_HOST: str = os.getenv("REDIS_HOST", "redis")
    REDIS_PORT: int = int(os.getenv("REDIS_PORT", 6379))
    REDIS_DB: int = int(os.getenv("REDIS_DB", 0))
    REDIS_PASSWORD: Optional[str] = os.getenv("REDIS_PASSWORD", None)
    
    # Model paths
    SPAM_MODEL_PATH: str = os.getenv("SPAM_MODEL_PATH", "app/models/spam_1")
    TOXIC_MODEL_PATH: str = os.getenv("TOXIC_MODEL_PATH", "app/models/toxic_3")
    CLIP_MODEL_NAME: str = os.getenv("CLIP_MODEL_NAME", "openai/clip-vit-base-patch32")
    TEXT_EMBEDDING_MODEL_NAME: str = os.getenv("TEXT_EMBEDDING_MODEL_NAME", "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2")
    # DISTILBERT_MODEL_NAME: str = os.getenv("DISTILBERT_MODEL_NAME", "distilbert-base-uncased")
    WHISPER_MODEL_NAME: str = os.getenv("WHISPER_MODEL_NAME", "small")
    
    # Processing config
    TEXT_BATCH_SIZE: int = int(os.getenv("TEXT_BATCH_SIZE", 32))
    TEXT_WINDOW_SIZE: int = int(os.getenv("TEXT_WINDOW_SIZE", 128))
    TEXT_STRIDE: int = int(os.getenv("TEXT_STRIDE", 64))
    
    # Embedding config
    EMBEDDING_DIMENSION: int = int(os.getenv("EMBEDDING_DIMENSION", 384))
    COSINE_SIMILARITY_THRESHOLD: float = float(os.getenv("COSINE_SIMILARITY_THRESHOLD", 0.85))
    
    # Timeout config (in seconds)
    MODEL_LOAD_TIMEOUT: int = int(os.getenv("MODEL_LOAD_TIMEOUT", 300))
    INFERENCE_TIMEOUT: int = int(os.getenv("INFERENCE_TIMEOUT", 60))
    REQUEST_TIMEOUT: int = int(os.getenv("REQUEST_TIMEOUT", 300))
    
    # Logging
    LOG_LEVEL: str = os.getenv("AI_LOG_LEVEL", "INFO")
    
    # Device config
    DEVICE: str = os.getenv("DEVICE", "cuda" if torch.cuda.is_available() else "cpu")
    
    
    class Config:
        env_file = ".env"
        case_sensitive = True


@lru_cache
def get_settings() -> Settings:
    """Get cached settings instance."""
    return Settings()
