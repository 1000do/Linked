"""Service for Stage 1: Duplication Detection."""

import logging
import time
import numpy as np
from typing import Dict, List, Optional, Tuple, Any
from core.models import StageLog, CourseModerationResponse, ModerationStatus, DuplicationStatus, MaterialEmbeddingResponse
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
        # return float(dot_product / (norm_a * norm_b))
        sim = dot_product / (norm_a * norm_b)
        return float(np.clip(sim, -1.0, 1.0))
    


    def similarity_search(
        self,
        new_embedding: MaterialEmbeddingResponse,
        existing_embeddings: List[MaterialEmbeddingResponse],
        threshold: float
    ) -> Optional[Dict[str, Any]]:
        """
        Find cached embeddings similar to query.
        """
        if not new_embedding.embedding or not new_embedding.material_id:
            return None

        # best_similar = None
        # best_score = -1.0

        result = {
            "new_material_id": new_embedding.material_id,
            "existing_material_id": None,
            "similarity_score": 0.0
        }

        scores = []
        match_found = False

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
            scores.append(sim)
            if sim >= threshold:
                match_found = True
                result['similarity_score'] = sim
                result['existing_material_id'] = existing.material_id
                break
        
        if not match_found:
            # Hanlde non-exisiting embeddings case
            result['similarity_score'] = sum(scores) / len(scores) if scores else 0.0
        
        return result


        
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
                    result=ModerationStatus.SKIPPED.value,
                    reason="No new embeddings provided for semantic check",
                    confidence_score=1.0,
                    latency_ms=latency_ms,
                    model_id=model_id
                )
                stage_logs.append(log_entry)
                return False, [], [], stage_logs
            
            
            similarity_scores = []
            flagged_candidates = []
            
            checked_once = False
            for key, new_embeddings in new_embeddings_dict.items():
                model_id = int(key.split("_")[1])
                for new_emb in new_embeddings:
                    entry_time = time.time()
                    candidate_id = new_emb.material_id
                    candidate = f"material_{candidate_id}"
                    
                   
                    

                    # Search for matches
                    sim_search_res = self.similarity_search(new_emb, existing_embeddings, threshold)
                    match_id = sim_search_res.get("existing_material_id", None)
                    score = sim_search_res.get("similarity_score", 0.0)

                    # if MATCH FOUND
                    if match_id:
                        result = DuplicationStatus.MATCH_FOUND.value
                        reason = f"Found a semantic duplication for {candidate} on existing_material_{match_id} (cosine similarity: {score} >= {threshold})"
                        flagged_content = [candidate]

                        flagged_candidates.append(candidate_id)
                        similarity_scores.append(score)
                    else:
                        result = DuplicationStatus.NO_MATCH.value
                        reason = f"No semantic duplicates found for {candidate} (average cosine similarity: {score} < {threshold})"
                        flagged_content = None
                                            
                    
                    details_entry = {
                        "model_id": model_id,
                        "candidate_material_id": candidate_id,
                        "existing_material_id": match_id,
                        "similarity_score": score
                    }


                    
                    start = entry_time if checked_once else start_time
                    
                    log_entry = self.build_stage_log(
                        stage=self.STAGE,
                        step=step,
                        result=result,
                        reason=reason,
                        confidence_score=score,
                        flagged_content=flagged_content,
                        details=details_entry,
                        latency_ms=(time.time() - start) * 1000,
                        model_id=model_id
                    )
                    
                    logger.info(f"Stage log:\n{log_entry}")
                    stage_logs.append(log_entry)
                    
                    if not checked_once:
                        checked_once = True

            
            is_duplicate = len(flagged_candidates) > 0
            
           
            
            result_str = DuplicationStatus.MATCH_FOUND.value if is_duplicate else DuplicationStatus.NO_MATCH.value
            reason_str = (
                f"Found {len(flagged_candidates)} semantically duplicate materials (cosine similarity > {threshold})"
                if is_duplicate
                else f"No semantic duplicates found (threshold: {threshold})"
            )
            
            self.log_result(
                self.STAGE, step,
                result=result_str,
                reason=reason_str,
                matched_count=len(flagged_candidates)
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
            
           
            
            return is_duplicate, flagged_candidates, similarity_scores, stage_logs
        
        except Exception as e:
            self.log_error(self.STAGE, step, str(e))
            raise ModerationException(
                f"Semantic duplication check failed: {e}",
                code="SEMANTIC_DEDUP_ERROR"
            )
