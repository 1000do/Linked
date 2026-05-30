import json
import logging
from typing import List, Optional
from core.models import (
    CourseDetailDto,
    AiModelDto,
    MaterialEmbeddingResponse,
    
)
from repositories.cache_repository import CacheRepository

logger = logging.getLogger(__name__)

class RedisService:
    def __init__(self, cache_repository: Optional[CacheRepository] = None):
        self.cache_repository = cache_repository or CacheRepository()

    def get_course_details(self, course_id: int) -> Optional[CourseDetailDto]:
        """
        Fetches cached course data from Redis and maps it to CourseDetailDto.
        """
        try:
            course_data = self.cache_repository.get_course_data(course_id)
            if not course_data:
                return None
            return CourseDetailDto.model_validate(course_data)
        except Exception as e:
            logger.error(f"Error getting course details from Redis for course {course_id}: {e}")
            return None

    def get_ai_models(self, key_suffix: str) -> List[AiModelDto]:
        """
        Fetches models, parses JSON array, and maps to List[ai_model_dto].
        """
        try:
            raw_models = self.cache_repository.get_ai_models(key_suffix)
            if not raw_models:
                return []
            parsed = json.loads(raw_models)
            if isinstance(parsed, list):
                return [AiModelDto.model_validate(m) for m in parsed]
            elif isinstance(parsed, dict):
                return [AiModelDto.model_validate(parsed)]
            return []
        except Exception as e:
            logger.error(f"Error fetching AI models for suffix {key_suffix}: {e}")
            return []

    def set_material_embedding(self, dto: MaterialEmbeddingResponse, ttl: Optional[int] = None) -> bool:
        """
        Caches a newly generated embedding.
        """
        if not dto.material_id or not dto.embedding:
            return False
        emb_type = dto.embedding_type
        if not emb_type or emb_type not in ("text", "media"):
            emb_type = "media" if len(dto.embedding) == 512 else "text"
        return self.cache_repository.set_material_embedding(dto.material_id, dto.embedding, emb_type, ttl)

    def get_all_existing_embeddings(self) -> List[MaterialEmbeddingResponse]:
        """
        Scans all keys under material_embedding:, robustly parses JSON array lists of floats
        or serialized DTO models from C#, and returns a List of MaterialEmbeddingResponse.
        """
        try:
            raw_matches = self.cache_repository.get_existing_embeddings()
            results = []
            for key, val in raw_matches.items():
                # Extract material_id from key
                try:
                    material_id = int(key.replace(self.cache_repository.EMBEDDING_PREFIX, ""))
                except ValueError:
                    continue

                try:
                    parsed = json.loads(val)
                    embedding_vector = None
                    embedding_type = None
                    if isinstance(parsed, list):
                        # Direct list of floats
                        embedding_vector = [float(x) for x in parsed]
                    elif isinstance(parsed, dict):
                        # C# DTO format
                        raw_emb = parsed.get("Embedding") or parsed.get("embedding")
                        if isinstance(raw_emb, list):
                            embedding_vector = [float(x) for x in raw_emb]
                        elif isinstance(raw_emb, str):
                            # Try parsing the string embedded vector e.g. "[0.1, 0.2, ...]"
                            try:
                                parsed_emb = json.loads(raw_emb)
                                if isinstance(parsed_emb, list):
                                    embedding_vector = [float(x) for x in parsed_emb]
                            except Exception:
                                pass
                        
                        embedding_type = parsed.get("EmbeddingType") or parsed.get("embeddingType")
                    
                    if embedding_vector:
                        if not embedding_type or embedding_type not in ("text", "media"):
                            embedding_type = "media" if len(embedding_vector) == 512 else "text"
                        embedding_id = parsed.get("EmbeddingId") or parsed.get("embeddingId")
                        
                        results.append(MaterialEmbeddingResponse(
                            embedding_id= embedding_id if embedding_id else 0,
                            material_id=material_id,
                            embedding=embedding_vector,
                            embedding_type=embedding_type
                        ))
                except Exception as e:
                    logger.warning(f"Failed to parse embedding value for key {key}: {e}")
            return results
        except Exception as e:
            logger.error(f"Error getting all existing embeddings: {e}")
            return []
