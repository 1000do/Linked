"""Service for Stage 1: Duplication Detection."""

import logging
import time
from typing import Dict, List, Optional, Tuple, Any
from core.models import StageLog, CourseModerationResponse, ModerationStatus
from core.exceptions import CacheNotFoundException, DataValidationException, ModerationException
from services.base_service import BaseService
from repositories.cache_repository import CacheRepository
from config.settings import Settings, get_settings

logger = logging.getLogger(__name__)


class DuplicationService(BaseService):
    """Handle Stage 1: Exact and semantic duplication checking."""
    
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
    
    async def check_exact_duplication(
        self,
        course_id: int,
        title_hash: str,
        description_hash: str,
        thumbnail_hash: Optional[str] = None,
        material_hashes: Optional[List[str]] = None,
    ) -> Tuple[bool, List[int], List[int], List[StageLog]]:
        """
        Step 1: Check for exact duplication via MD5 hashes.
        
        Queries Redis for past hashes (entire history) to find exact matches.
        
        Args:
            course_id: Course being checked
            title_hash: MD5 hash of course title
            description_hash: MD5 hash of course description
            thumbnail_hash: MD5 hash of course thumbnail
            material_hashes: MD5 hashes of learning materials
            
        Returns:
            (is_duplicate, matched_course_ids, matched_material_ids, stage_logs)
        """
        step = 1
        start_time = time.time()
        stage_logs = []
        
        try:
            self.log_step_entry(self.STAGE, step, action="exact_duplication_check")
            
            matched_courses = []
            matched_materials = []
            
            # Note: In a real implementation with a database, you would query:
            # SELECT course_id FROM course_exts 
            # WHERE title_hash = ? OR description_hash = ? OR thumbnail_hash = ?
            # For this MVP with Redis only, we check the cached data structure
            
            # Check course-level hashes
            # This is simplified - in production, you'd have a dedicated hash index
            try:
                course_data = self.cache_repo.get_course_data(course_id)
                
                # Log: checking hashes in Redis
                self.log_step_entry(self.STAGE, step, checking="course_hashes", course_id=course_id)
                
                # For MVP, we simulate finding no matches (real implementation would query history)
                # In production: Query database for matching hashes across all courses created
                # within retention period (entire history as per refined plan)
                
                # Pseudo-logic (commented out - requires DB access):
                # matching_courses = db.query("SELECT course_id FROM course_exts WHERE title_hash = ?", title_hash)
                # if matching_courses:
                #     matched_courses = [c.id for c in matching_courses if c.course_id != course_id]
                
                is_duplicate = len(matched_courses) > 0
                
                latency_ms = (time.time() - start_time) * 1000
                
                log_entry = self.build_stage_log(
                    stage=self.STAGE,
                    step=step,
                    result="NO_MATCH" if not is_duplicate else "MATCH_FOUND",
                    reason=(
                        f"No exact hash duplicates found (checked {course_id})"
                        if not is_duplicate
                        else f"Found {len(matched_courses)} duplicate courses"
                    ),
                    confidence_score=1.0 if is_duplicate else 1.0,
                    flagged_content=matched_courses if matched_courses else None,
                    details={
                        "title_hash": title_hash[:8] + "...",
                        "matched_course_count": len(matched_courses),
                        "matched_courses": matched_courses,
                    },
                    latency_ms=latency_ms,
                )
                
                stage_logs.append(log_entry)
                self.log_result(
                    self.STAGE, step,
                    result="NO_MATCH" if not is_duplicate else "MATCH_FOUND",
                    reason=log_entry["reason"],
                    matched_courses=len(matched_courses)
                )
                
                return is_duplicate, matched_courses, matched_materials, stage_logs
            
            except CacheNotFoundException as e:
                # Cache miss is acceptable for new courses
                self.logger.info(f"Cache miss (expected for new course): {e.message}")
                return False, [], [], stage_logs
        
        except Exception as e:
            self.log_error(self.STAGE, step, str(e))
            raise ModerationException(
                f"Exact duplication check failed: {e}",
                code="EXACT_DEDUP_ERROR"
            )
    
    async def check_semantic_duplication(
        self,
        course_id: int,
        material_ids: List[int],
        material_embeddings: Optional[List[List[float]]] = None,
    ) -> Tuple[bool, List[int], List[float], List[StageLog]]:
        """
        Step 2: Check for semantic duplication via cosine similarity.
        
        Compares material embeddings against cached embeddings to find similar content.
        
        Args:
            course_id: Course being checked
            material_ids: IDs of materials in course
            material_embeddings: 768-dim embeddings of materials
            
        Returns:
            (is_duplicate, matched_material_ids, similarity_scores, stage_logs)
        """
        step = 2
        start_time = time.time()
        stage_logs = []
        
        try:
            self.log_step_entry(self.STAGE, step, action="semantic_duplication_check")
            
            if not material_embeddings:
                # No embeddings to check
                latency_ms = (time.time() - start_time) * 1000
                log_entry = self.build_stage_log(
                    stage=self.STAGE,
                    step=step,
                    result="SKIPPED",
                    reason="No embeddings provided for semantic check",
                    confidence_score=1.0,
                    latency_ms=latency_ms,
                )
                stage_logs.append(log_entry)
                return False, [], [], stage_logs
            
            matched_materials = []
            similarity_scores = []
            
            # Check each material embedding against cached embeddings
            for material_id, embedding in zip(material_ids, material_embeddings):
                try:
                    # Find similar embeddings in cache (cosine similarity > threshold)
                    similar = self.cache_repo.find_similar_embeddings(
                        query_embedding=embedding,
                        threshold=self.settings.COSINE_SIMILARITY_THRESHOLD,
                        exclude_material_id=material_id,
                    )
                    
                    if similar:
                        matched_materials.extend([m["material_id"] for m in similar])
                        similarity_scores.extend([m["similarity_score"] for m in similar])
                        
                        self.logger.info(
                            f"Material {material_id} has {len(similar)} similar matches "
                            f"(threshold: {self.settings.COSINE_SIMILARITY_THRESHOLD})"
                        )
                
                except Exception as e:
                    self.logger.warning(f"Error checking material {material_id} embeddings: {e}")
                    continue
            
            is_duplicate = len(matched_materials) > 0
            
            latency_ms = (time.time() - start_time) * 1000
            
            log_entry = self.build_stage_log(
                stage=self.STAGE,
                step=step,
                result="MATCH_FOUND" if is_duplicate else "NO_MATCH",
                reason=(
                    f"Found {len(matched_materials)} semantically similar materials (cosine similarity > {self.settings.COSINE_SIMILARITY_THRESHOLD})"
                    if is_duplicate
                    else f"No semantic duplicates found (threshold: {self.settings.COSINE_SIMILARITY_THRESHOLD})"
                ),
                confidence_score=max(similarity_scores) if similarity_scores else 1.0,
                flagged_content=matched_materials if matched_materials else None,
                details={
                    "checked_materials": len(material_ids),
                    "matched_count": len(matched_materials),
                    "similarity_threshold": self.settings.COSINE_SIMILARITY_THRESHOLD,
                    "max_similarity": max(similarity_scores) if similarity_scores else 0.0,
                },
                latency_ms=latency_ms,
            )
            
            stage_logs.append(log_entry)
            self.log_result(
                self.STAGE, step,
                result="MATCH_FOUND" if is_duplicate else "NO_MATCH",
                reason=log_entry["reason"],
                matched_count=len(matched_materials)
            )
            
            return is_duplicate, matched_materials, similarity_scores, stage_logs
        
        except Exception as e:
            self.log_error(self.STAGE, step, str(e))
            raise ModerationException(
                f"Semantic duplication check failed: {e}",
                code="SEMANTIC_DEDUP_ERROR"
            )
    
    async def orchestrate_stage1(
        self,
        course_id: int,
        title_hash: Optional[str] = None,
        description_hash: Optional[str] = None,
        thumbnail_hash: Optional[str] = None,
        material_hashes: Optional[List[str]] = None,
        material_ids: Optional[List[int]] = None,
        material_embeddings: Optional[List[List[float]]] = None,
    ) -> CourseModerationResponse:
        """
        Orchestrate Stage 1: Run both exact and semantic deduplication.
        
        Returns FLAGGED immediately if Step 1 detects duplicates.
        Otherwise runs Step 2 and returns result.
        
        Args:
            course_id: Course to moderate
            title_hash: MD5 hash of course title
            description_hash: MD5 hash of course description
            thumbnail_hash: MD5 hash of course thumbnail (optional)
            material_hashes: MD5 hashes of materials (optional)
            material_ids: IDs of materials (for embedding lookup)
            material_embeddings: Embeddings of materials
            
        Returns:
            CourseModerationResponse with full trace
        """
        stage_start = time.time()
        all_stage_logs = []
        
        try:
            self.log_stage_entry(self.STAGE, course_id, action="START")
            
            # ====================================================================
            # Step 1: Exact Duplication Check
            # ====================================================================
            try:
                exact_dup, matched_courses, _, step1_logs = await self.check_exact_duplication(
                    course_id=course_id,
                    title_hash=title_hash,
                    description_hash=description_hash,
                    thumbnail_hash=thumbnail_hash,
                    material_hashes=material_hashes,
                )
                
                all_stage_logs.extend(step1_logs)
                
                # If exact duplicates found, return immediately
                if exact_dup:
                    total_latency = (time.time() - stage_start) * 1000
                    self.logger.warning(
                        f"[Stage {self.STAGE}] FLAGGED for course {course_id}: "
                        f"Exact duplicate found - matched courses: {matched_courses}"
                    )
                    
                    return CourseModerationResponse(
                        course_id=course_id,
                        moderation_status=ModerationStatus.FLAGGED.value,
                        stage_logs=all_stage_logs,
                        flagged_fields=[f"duplicate_of_course_{cid}" for cid in matched_courses],
                        overall_confidence_score=step1_logs[0]["confidence_score"] if step1_logs else 0.95,
                        total_latency_ms=total_latency,
                    )
            
            except Exception as e:
                self.logger.error(f"Step 1 failed: {e}")
                all_stage_logs.append(self.build_stage_log(
                    stage=self.STAGE,
                    step=1,
                    result="ERROR",
                    reason=f"Exact duplication check failed: {e}",
                    confidence_score=0.0,
                    details={"error": str(e)},
                ))
            
            # ====================================================================
            # Step 2: Semantic Duplication Check
            # ====================================================================
            try:
                semantic_dup, matched_materials, similarity_scores, step2_logs = await self.check_semantic_duplication(
                    course_id=course_id,
                    material_ids=material_ids or [],
                    material_embeddings=material_embeddings,
                )
                
                all_stage_logs.extend(step2_logs)
                
                # If semantic duplicates found, return FLAGGED
                if semantic_dup:
                    total_latency = (time.time() - stage_start) * 1000
                    self.logger.warning(
                        f"[Stage {self.STAGE}] FLAGGED for course {course_id}: "
                        f"Semantic duplicate found - matched materials: {matched_materials}"
                    )
                    
                    confidence = max(similarity_scores) if similarity_scores else 0.85
                    
                    return CourseModerationResponse(
                        course_id=course_id,
                        moderation_status=ModerationStatus.FLAGGED.value,
                        stage_logs=all_stage_logs,
                        flagged_fields=[f"duplicate_of_material_{mid}" for mid in matched_materials],
                        overall_confidence_score=confidence,
                        total_latency_ms=total_latency,
                    )
            
            except Exception as e:
                self.logger.error(f"Step 2 failed: {e}")
                all_stage_logs.append(self.build_stage_log(
                    stage=self.STAGE,
                    step=2,
                    result="ERROR",
                    reason=f"Semantic duplication check failed: {e}",
                    confidence_score=0.0,
                    details={"error": str(e)},
                ))
            
            # ====================================================================
            # No duplicates found - APPROVED
            # ====================================================================
            total_latency = (time.time() - stage_start) * 1000
            self.logger.info(f"[Stage {self.STAGE}] APPROVED for course {course_id}")
            
            # Calculate average confidence of all steps
            confidences = [log["confidence_score"] for log in all_stage_logs if log["result"] != "ERROR"]
            avg_confidence = sum(confidences) / len(confidences) if confidences else 1.0
            
            return CourseModerationResponse(
                course_id=course_id,
                moderation_status=ModerationStatus.APPROVED.value,
                stage_logs=all_stage_logs,
                flagged_fields=[],
                overall_confidence_score=avg_confidence,
                total_latency_ms=total_latency,
            )
        
        except Exception as e:
            self.logger.error(f"Stage 1 orchestration failed: {e}")
            total_latency = (time.time() - stage_start) * 1000
            
            return CourseModerationResponse(
                course_id=course_id,
                moderation_status=ModerationStatus.MANUAL_AUDIT.value,
                stage_logs=all_stage_logs,
                flagged_fields=[],
                overall_confidence_score=0.0,
                total_latency_ms=total_latency,
            )
