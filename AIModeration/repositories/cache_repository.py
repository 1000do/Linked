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
    COURSE_PREFIX = "mod:course:"
    EMBEDDING_PREFIX = "mod:embedding:"
    LOG_PREFIX = "mod:log:"
    
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
            
            embedding = json.loads(data)
            logger.debug(f"Retrieved embedding: {key}")
            return embedding
            
        except json.JSONDecodeError as e:
            logger.error(f"Failed to decode embedding for {key}: {e}")
            raise CacheNotFoundException(key, {"error": str(e)})
    
    def set_material_embedding(self, material_id: int, embedding: List[float], ttl: int = None) -> bool:
        """
        Store material embedding in cache.
        
        Args:
            material_id: Material identifier
            embedding: 768-dim embedding vector
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
            json_data = json.dumps(embedding)
            self.redis_client.setex(key, ttl, json_data)
            logger.debug(f"Embedding cached: {key}")
            return True
        except Exception as e:
            logger.error(f"Failed to cache embedding: {e}")
            return False
    
    # ========================================================================
    # SIMILARITY SEARCH
    # ========================================================================
    
    def find_similar_embeddings(
        self,
        query_embedding: List[float],
        threshold: float = 0.85,
        limit: int = 10,
        exclude_material_id: Optional[int] = None
    ) -> List[Dict[str, Any]]:
        """
        Find cached embeddings similar to query (cosine similarity > threshold).
        Note: This is a simplified implementation. For production, consider using
        Redis with vector module or separate vector DB.
        
        Args:
            query_embedding: Query embedding vector
            threshold: Similarity threshold (0-1)
            limit: Max results
            exclude_material_id: Material to exclude from results
            
        Returns:
            List of {material_id, similarity_score}
        """
        if not self._ensure_connected():
            logger.warning("Cannot search embeddings: Redis unavailable")
            return []
        
        # Get all embedding keys
        pattern = f"{self.EMBEDDING_PREFIX}*"
        keys = self.redis_client.keys(pattern)
        
        results = []
        query_array = np.array(query_embedding)
        
        for key in keys:
            try:
                embedding_data = self.redis_client.get(key)
                embedding = json.loads(embedding_data)
                embedding_array = np.array(embedding)
                
                # Compute cosine similarity
                similarity = self._cosine_similarity(query_array, embedding_array)
                
                # Extract material_id from key
                material_id = int(key.replace(self.EMBEDDING_PREFIX, ""))
                
                if exclude_material_id and material_id == exclude_material_id:
                    continue
                
                if similarity >= threshold:
                    results.append({
                        "material_id": material_id,
                        "similarity_score": float(similarity)
                    })
                    
            except Exception as e:
                logger.warning(f"Error processing embedding {key}: {e}")
                continue
        
        # Sort by similarity descending and limit
        results.sort(key=lambda x: x["similarity_score"], reverse=True)
        return results[:limit]
    
    @staticmethod
    def _cosine_similarity(a: np.ndarray, b: np.ndarray) -> float:
        """Compute cosine similarity between two vectors."""
        dot_product = np.dot(a, b)
        norm_a = np.linalg.norm(a)
        norm_b = np.linalg.norm(b)
        
        if norm_a == 0 or norm_b == 0:
            return 0.0
        
        return dot_product / (norm_a * norm_b)
    
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
            
