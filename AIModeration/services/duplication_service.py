"""Service for Stage 1: Duplication Detection."""

import logging
import time
import numpy as np
from typing import Dict, List, Optional, Tuple, Any
from core.models import StageLog, CourseModerationResponse, ModerationStatus, MaterialEmbeddingResponse
from core.exceptions import CacheNotFoundException, DataValidationException, ModerationException
from services.base_service import BaseService
from repositories.cache_repository import CacheRepository
from config.settings import Settings, get_settings

logger = logging.getLogger(__name__)


class DuplicationService(BaseService):
    """Handle Stage 1: Semantic duplication checking."""
    
    STAGE = 1
    
    def __init__(self, cache_repository: CacheRepository = None, settings: Settings = None):
        """
        Initialize duplication service.
        
        Args:
            cache_repository: Redis cache repository
            settings: Application settings
        """
        super().__init__("DuplicationService")
        self.cache_repo = cache_repository or CacheRepository(settings)
        self.settings = settings or get_settings()

    @staticmethod
    def cosine_similarity(a: List[float], b: List[float]) -> float:
        """Compute cosine similarity between two float vectors."""
        arr_a = np.array(a)
        arr_b = np.array(b)
        dot_product = np.dot(arr_a, arr_b)
        norm_a = np.linalg.norm(arr_a)
        norm_b = np.linalg.norm(arr_b)
        if norm_a == 0 or norm_b == 0:
            return 0.0
        return float(dot_product / (norm_a * norm_b))

    def similarity_search(
        self,
        new_embedding: MaterialEmbeddingResponse,
        existing_embeddings: List[MaterialEmbeddingResponse],
        threshold: float
    ) -> Optional[Dict[str, Any]]:
        """
        Find cached embeddings similar to query.
        """
        if not new_embedding.embedding:
            return None

        # best_similar = None
        # best_score = -1.0

        for existing in existing_embeddings:
            if not existing.embedding or existing.material_id == new_embedding.material_id:
                continue

            # Skip comparison if embedding types do not match
            if new_embedding.embedding_type and existing.embedding_type and new_embedding.embedding_type != existing.embedding_type:
                continue

            # Skip comparison if dimensions do not match to prevent numpy shape mismatch exceptions
            if len(new_embedding.embedding) != len(existing.embedding):
                continue

            sim = self.cosine_similarity(new_embedding.embedding, existing.embedding)
            if sim >= threshold:
                return {
                    "material_id": existing.material_id,
                    "similarity_score": sim
                }

        
            # if sim >= threshold and sim > best_score:
        #         best_score = sim
        #         best_similar = {
        #             "material_id": existing.material_id,
        #             "similarity_score": sim
        #         }

        # return best_similar


    async def check_semantic_duplication(
        self,
        new_embeddings_dict: Dict[str,List[MaterialEmbeddingResponse]],
        # new_embeddings: List[MaterialEmbeddingResponse],
        existing_embeddings: List[MaterialEmbeddingResponse],
        # model_id: int,
        threshold: float
    ) -> Tuple[bool, List[int], List[float], List[StageLog]]:
        """
        Step 1: Compare new embeddings against existing_embeddings.
        """
        step = 1
        start_time = time.time()
        stage_logs = []
        
        model_id = 0
        try:
            self.log_step_entry(self.STAGE, step, action="semantic_duplication_check")
            
            if not new_embeddings_dict or not any(lst for lst in new_embeddings_dict.values()):
                latency_ms = (time.time() - start_time) * 1000
                log_entry = self.build_stage_log(
                    stage=self.STAGE,
                    step=step,
                    result="SKIPPED",
                    reason="No new embeddings provided for semantic check",
                    confidence_score=1.0,
                    latency_ms=latency_ms,
                    model_id=model_id
                )
                stage_logs.append(log_entry)
                return False, [], [], stage_logs
            
            
            matched_material_ids = []
            similarity_scores = []
            overall_details = {}
            
            checked_once = False
            for key, new_embeddings in new_embeddings_dict.items():
                model_id = int(key.split("_")[1])
                for new_emb in new_embeddings:
                    entry_time = time.time()
                    
                    candidate = f"material_{new_emb.material_id}"
                    
                    score = 0.0
                    
                    similar = self.similarity_search(new_emb, existing_embeddings, threshold)
                    if similar:
                        matched_material_ids.append(similar["material_id"])
                        score = similar["similarity_score"]
                        similarity_scores.append(score)
                        result = "MATCH_FOUND"
                        reason = f"Found a semantically similar for material_{new_emb.material_id} (cosine similarity: {score} > {threshold})"
                        flagged_content = [candidate]
                        
                    
                    else:
                        result = "NO_MATCH"
                        reason = f"No semantic duplicates found (threshold: {threshold})"
                        flagged_content = None
                    


                    overall_details.setdefault(candidate, []).append({
                        "model_id": model_id,
                        "similarity_score": score
                    })
                    
                    start = entry_time if checked_once else start_time
                    
                    log_entry = self.build_stage_log(
                        stage=self.STAGE,
                        step=step,
                        result=result,
                        reason=reason,
                        confidence_score=score,
                        flagged_content=flagged_content,
                        details=overall_details,
                        latency_ms=(time.time() - start) * 1000,
                        model_id=model_id
                    )
                    stage_logs.append(log_entry)
                    
                    if not checked_once:
                        checked_once = True

            
            is_duplicate = len(matched_material_ids) > 0
            
           
            
            result_str = "MATCH_FOUND" if is_duplicate else "NO_MATCH"
            reason_str = (
                f"Found {len(matched_material_ids)} semantically similar materials (cosine similarity > {threshold})"
                if is_duplicate
                else f"No semantic duplicates found (threshold: {threshold})"
            )
            
            self.log_result(
                self.STAGE, step,
                result=result_str,
                reason=reason_str,
                matched_count=len(matched_material_ids)
            )
            
            # # Total latency
            # latency_ms = (time.time() - start_time) * 1000
            # # Average score or default to 1.0 if empty
            # confidence_score = float(np.mean(similarity_scores)) if similarity_scores else 1.0
            # # Map lists to matching string types expected by C# DTOs
            # flagged_list = [f"material_{x}" for x in matched_material_ids]
            
            # overall_details.update(
            #     {
            #         "checked_count": sum((len(embs) for embs in new_embeddings_dict.values())),
            #         "matched_count": len(matched_material_ids),
            #         "similarity_threshold": threshold,
            #     }
            # )
            
            # log_entry = self.build_stage_log(
            #     stage=self.STAGE,
            #     step=step,
            #     result=result_str,
            #     reason=reason_str,
            #     confidence_score=confidence_score,
            #     flagged_content=flagged_list if is_duplicate else None,
            #     details=overall_details,
            #     latency_ms=latency_ms,
            #     model_id=model_id
            # )
            
            # stage_logs.append(log_entry)
            
           
            
            return is_duplicate, matched_material_ids, similarity_scores, stage_logs
        
        except Exception as e:
            self.log_error(self.STAGE, step, str(e))
            raise ModerationException(
                f"Semantic duplication check failed: {e}",
                code="SEMANTIC_DEDUP_ERROR"
            )
