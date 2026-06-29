"""Redis cache repository for moderation data access."""

import json
import redis
import logging
import numpy as np
from typing import Optional, Dict, List, Any
from config.settings import Settings, get_settings
from core.exceptions import CacheNotFoundException


logger = logging.getLogger(__name__)


class CacheRepository:
    """Single data access layer for Redis cache."""
    
    # Key prefixes
    COURSE_PREFIX = "course_moderation:detail:"
    EMBEDDING_PREFIX = "material_embedding:"
    PENDING_EMBEDDING_PREFIX = "pending_embedding:"
    LOG_PREFIX = "mod:log:"
    AI_MODEL_PREFIX = "ai_models:"
    EMBEDDINGS_INITIALIZED = "material_embedding:initialized"
    
    # TTL settings (in seconds)
    CACHE_TTL = 86400  # 24 hours
    
    def __init__(self, settings: Settings = None):
        """Initialize Redis connection (lazy)."""
        self.settings = settings or get_settings()
        self.redis_client = None
        self._connection_attempted = False
        self._connection_failed = False
    
    def _ensure_connected(self) -> bool:
        """Ensure Redis connection is established (lazy connection)."""
        if self.redis_client is not None:
            return True  # Already connected
        
        if self._connection_failed:
            return False  # Previous attempt failed
        
        try:
            self.redis_client = redis.Redis(
                host=self.settings.REDIS_HOST,
                port=self.settings.REDIS_PORT,
                db=self.settings.REDIS_DB,
                password=self.settings.REDIS_PASSWORD,
                decode_responses=True,
                socket_connect_timeout=5,
                socket_keepalive=True,
            )
            # Test connection
            self.redis_client.ping()
            logger.info(f"✓ Redis connected: {self.settings.REDIS_HOST}:{self.settings.REDIS_PORT}")
            self._connection_attempted = True
            return True
        except Exception as e:
            logger.warning(f"⚠ Redis connection unavailable: {e}")
            self._connection_failed = True
            self._connection_attempted = True
            return False
    
    def get_course_data(self, course_id: int) -> Optional[Dict[str, Any]]:
        """
        Retrieve pre-cached course data from Redis.
        
        Args:
            course_id: Course identifier
            
        Returns:
            Course data dict or None if not found
            
        Raises:
            CacheNotFoundException: If key doesn't exist
        """
        if not self._ensure_connected():
            raise CacheNotFoundException(f"{course_id}", {"reason": "Redis unavailable"})
        
        key = f"{self.COURSE_PREFIX}{course_id}"
        
        try:
            data = self.redis_client.get(key)
            if data is None:
                logger.warning(f"Cache miss: {key}")
                raise CacheNotFoundException(key, {"course_id": course_id})
            
            course_data = json.loads(data)
            logger.debug(f"Cache hit: {key}")
            return course_data
            
        except json.JSONDecodeError as e:
            logger.error(f"Failed to decode cache data for {key}: {e}")
            raise CacheNotFoundException(key, {"error": str(e)})
    
    def set_course_data(self, course_id: int, data: Dict[str, Any], ttl: int = None) -> bool:
        """
        Store course data in Redis cache.
        
        Args:
            course_id: Course identifier
            data: Course data dictionary
            ttl: Time-to-live in seconds (default: 24h)
            
        Returns:
            Success flag
        """
        if not self._ensure_connected():
            logger.warning(f"Cannot cache course {course_id}: Redis unavailable")
            return False
        
        key = f"{self.COURSE_PREFIX}{course_id}"
        ttl = ttl or self.CACHE_TTL
        
        try:
            json_data = json.dumps(data)
            self.redis_client.setex(key, ttl, json_data)
            logger.info(f"Cache set: {key} (TTL: {ttl}s)")
            return True
        except Exception as e:
            logger.error(f"Failed to cache course data: {e}")
            return False
    
    def invalidate_course(self, course_id: int) -> bool:
        """
        Delete course from cache (on update).
        
        Args:
            course_id: Course identifier
            
        Returns:
            Success flag
        """
        if not self._ensure_connected():
            logger.warning(f"Cannot invalidate course {course_id}: Redis unavailable")
            return False
        
        key = f"{self.COURSE_PREFIX}{course_id}"
        
        try:
            deleted = self.redis_client.delete(key)
            logger.info(f"Cache invalidated: {key} ({deleted} keys deleted)")
            return deleted > 0
        except Exception as e:
            logger.error(f"Failed to invalidate cache: {e}")
            return False
    
    # ========================================================================
    # EMBEDDINGS
    # ========================================================================
    
    def get_material_embeddings(self, material_id: int) -> Optional[List[float]]:
        """
        Retrieve cached embeddings for a material.
        
        Args:
            material_id: Material identifier
            
        Returns:
            768-dim embedding vector or None
            
        Raises:
            CacheNotFoundException: If not found
        """
        if not self._ensure_connected():
            raise CacheNotFoundException(f"{material_id}", {"reason": "Redis unavailable"})
        
        key = f"{self.EMBEDDING_PREFIX}{material_id}"
        
        try:
            data = self.redis_client.get(key)
            if data is None:
                raise CacheNotFoundException(key, {"material_id": material_id})
            
            parsed = json.loads(data)
            embedding = parsed
            if isinstance(parsed, dict):
                embedding = parsed.get("embedding")
                
            logger.debug(f"Retrieved embedding: {key}")
            return embedding
            
        except json.JSONDecodeError as e:
            logger.error(f"Failed to decode embedding for {key}: {e}")
            raise CacheNotFoundException(key, {"error": str(e)})
    
    def set_material_embedding(self, material_id: int, embedding: List[float], embedding_type: str, ttl: int = None) -> bool:
        """
        Store material embedding in cache.
        
        Args:
            material_id: Material identifier
            embedding: embedding vector
            embedding_type: 'text' or 'media'
            ttl: Time-to-live in seconds
            
        Returns:
            Success flag
        """
        if not self._ensure_connected():
            logger.warning(f"Cannot cache embedding {material_id}: Redis unavailable")
            return False
        
        key = f"{self.EMBEDDING_PREFIX}{material_id}"
        ttl = ttl or self.CACHE_TTL
        
        try:
            dto_dict = {
                "embedding_id": 0,
                "material_id": material_id,
                "embedding": embedding,
                "embedding_type": embedding_type
            }

            json_data = json.dumps(dto_dict)
            logger.info(f"\nCaching MaterialEmbeddingResponse...\nCache key: {key}\nValue: {json_data}")
            self.redis_client.setex(key, ttl, json_data)
            logger.debug(f"Embedding cached: {key}")
            return True
        except Exception as e:
            logger.error(f"Failed to cache embedding: {e}")
            return False

    def set_pending_material_embedding(self, material_id: int, embedding: List[float], embedding_type: str, ttl: int = None) -> bool:
        """
        Store pending material embedding in cache.
        
        Args:
            material_id: Material identifier
            embedding: embedding vector
            embedding_type: 'text' or 'media'
            ttl: Time-to-live in seconds
            
        Returns:
            Success flag
        """
        if not self._ensure_connected():
            logger.warning(f"Cannot cache pending embedding {material_id}: Redis unavailable")
            return False
        
        key = f"{self.PENDING_EMBEDDING_PREFIX}{material_id}"
        ttl = ttl or self.CACHE_TTL
        
        try:
            dto_dict = {
                "embedding_id": 0,
                "material_id": material_id,
                "embedding": embedding,
                "embedding_type": embedding_type
            }

            json_data = json.dumps(dto_dict)
            logger.info(f"\nCaching Pending MaterialEmbeddingResponse...\nCache key: {key}\nValue: {json_data}")
            self.redis_client.setex(key, ttl, json_data)
            logger.debug(f"Pending Embedding cached: {key}")
            return True
        except Exception as e:
            logger.error(f"Failed to cache pending embedding: {e}")
            return False
    
    # ========================================================================
    # GET ALL EXISTING EMBEDDINGS & AI MODELS
    # ========================================================================
    
    def get_existing_embeddings(self, key_prefix: str = "material_embedding:") -> Dict[str, str]:
        """
        Query Redis cache for all matches by scanning the key_prefix until cursor == 0.
        Excludes the key 'material_embedding:initialized'.
        Returns a dictionary of key-value pairs.
        """
        if not self._ensure_connected():
            logger.warning("Cannot scan embeddings: Redis unavailable")
            return {}
            
        cursor = 0
        matches = {}
        
        while True:
            cursor, keys = self.redis_client.scan(cursor=cursor, match=f"{key_prefix}*", count=100)
            for k in keys:
                if k == self.EMBEDDINGS_INITIALIZED:
                    continue
                try:
                    val = self.redis_client.get(k)
                    if val is not None:
                        matches[k] = val
                except Exception as e:
                    logger.warning(f"Error fetching embedding for key {k}: {e}")
            if cursor == 0:
                break
                
        return matches

    def get_ai_models(self, key_suffix: str) -> Optional[str]:
        """
        Query Redis cache with cache_key formatted from prefix and key_suffix.
        """
        if not self._ensure_connected():
            logger.warning("Cannot fetch AI models: Redis unavailable")
            return None
            
        cache_key = f"{self.AI_MODEL_PREFIX}{key_suffix}"
        try:
            return self.redis_client.get(cache_key)
        except Exception as e:
            logger.error(f"Error fetching AI models for key {cache_key}: {e}")
            return None
    
    # ========================================================================
    # LOGGING
    # ========================================================================
    
    def cache_moderation_log(self, course_id: int, log_data: Dict[str, Any]) -> bool:
        """
        Cache moderation log entry.
        
        Args:
            course_id: Course identifier
            log_data: Log entry data
            
        Returns:
            Success flag
        """
        if not self._ensure_connected():
            logger.warning(f"Cannot cache log for course {course_id}: Redis unavailable")
            return False
        
        key = f"{self.LOG_PREFIX}{course_id}:{int(__import__('time').time())}"
        
        try:
            json_data = json.dumps(log_data)
            self.redis_client.setex(key, self.CACHE_TTL, json_data)
            logger.debug(f"Log cached: {key}")
            return True
        except Exception as e:
            logger.error(f"Failed to cache log: {e}")
            return False
    
    # ========================================================================
    # HEALTH CHECK
    # ========================================================================
    
    def health_check(self) -> bool:
        """Check if Redis is accessible."""
        if not self._ensure_connected():
            return False
        
        try:
            self.redis_client.ping()
            return True
        except Exception as e:
            logger.warning(f"Redis health check failed: {e}")
            self._connection_failed = True
            return False
            
