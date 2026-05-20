"""Service for Stage 2: Toxicity & Spam Detection."""

import logging
import time
from typing import Dict, List, Optional, Any, Tuple
from core.models import StageLog, CourseModerationResponse, ModerationStatus
from core.exceptions import ModerationException, FileProcessingException
from services.base_service import BaseService
from services.text_classifier_service import TextClassifierService
from services.text_extraction_service import TextExtractionService
from repositories.cache_repository import CacheRepository

logger = logging.getLogger(__name__)


class ToxicityService(BaseService):
    """Handle Stage 2: Toxicity & Spam detection in text and media."""
    
    STAGE = 2
    
    def __init__(
        self,
        text_classifier: TextClassifierService = None,
        text_extractor: TextExtractionService = None,
        cache_repository: CacheRepository = None,
    ):
        """
        Initialize toxicity service.
        
        Args:
            text_classifier: Text classifier service
            text_extractor: Text extraction service
            cache_repository: Cache repository
        """
        super().__init__("ToxicityService")
        self.text_classifier = text_classifier or TextClassifierService()
        self.text_extractor = text_extractor or TextExtractionService()
        self.cache_repo = cache_repository or CacheRepository()
    
    async def check_text_toxicity(
        self,
        title: str,
        description: str,
        lesson_texts: Optional[List[str]] = None,
    ) -> Tuple[bool, List[str], List[StageLog]]:
        """
        Step 1: Check title, description, and lesson texts for toxicity.
        
        Args:
            title: Course title
            description: Course description
            lesson_texts: List of lesson titles and descriptions
            
        Returns:
            (is_flagged, flagged_fields, stage_logs)
        """
        step = 1
        start_time = time.time()
        stage_logs = []
        
        try:
            self.log_step_entry(self.STAGE, step, action="text_toxicity_check")
            
            flagged_fields = []
            max_confidence = 0.0
            
            # Check title
            if title:
                try:
                    result, conf, details = await self.text_classifier.classify_text(title)
                    if result == "FLAGGED":
                        flagged_fields.append("title")
                        max_confidence = max(max_confidence, conf)
                        self.logger.warning(f"Flagged title: {result} (conf: {conf})")
                except Exception as e:
                    self.logger.warning(f"Failed to classify title: {e}")
            
            # Check description
            if description:
                try:
                    result, conf, details = await self.text_classifier.classify_text(description)
                    if result == "FLAGGED":
                        flagged_fields.append("description")
                        max_confidence = max(max_confidence, conf)
                        self.logger.warning(f"Flagged description: {result} (conf: {conf})")
                except Exception as e:
                    self.logger.warning(f"Failed to classify description: {e}")
            
            # Check lesson texts
            if lesson_texts:
                for idx, lesson_text in enumerate(lesson_texts):
                    try:
                        result, conf, details = await self.text_classifier.classify_text(lesson_text)
                        if result == "FLAGGED":
                            flagged_fields.append(f"lesson_{idx}")
                            max_confidence = max(max_confidence, conf)
                            self.logger.warning(f"Flagged lesson {idx}: {result} (conf: {conf})")
                    except Exception as e:
                        self.logger.warning(f"Failed to classify lesson {idx}: {e}")
            
            is_flagged = len(flagged_fields) > 0
            
            latency_ms = (time.time() - start_time) * 1000
            
            log_entry = self.build_stage_log(
                stage=self.STAGE,
                step=step,
                result="FLAGGED" if is_flagged else "APPROVED",
                reason=(
                    f"Toxicity detected in fields: {', '.join(flagged_fields)}"
                    if is_flagged
                    else "No toxicity detected in text fields"
                ),
                confidence_score=max_confidence if is_flagged else 1.0,
                flagged_content=flagged_fields if is_flagged else None,
                details={
                    "checked_fields": ["title", "description"] + (["lessons"] if lesson_texts else []),
                    "flagged_count": len(flagged_fields),
                },
                latency_ms=latency_ms,
            )
            
            stage_logs.append(log_entry)
            self.log_result(
                self.STAGE, step,
                result="FLAGGED" if is_flagged else "APPROVED",
                reason=log_entry["reason"],
                flagged_count=len(flagged_fields)
            )
            
            return is_flagged, flagged_fields, stage_logs
        
        except Exception as e:
            self.log_error(self.STAGE, step, str(e))
            raise ModerationException(
                f"Text toxicity check failed: {e}",
                code="TEXT_TOXICITY_ERROR"
            )
    
    async def check_media_text_toxicity(
        self,
        material_list: Optional[List[Dict[str, Any]]] = None,
    ) -> Tuple[bool, List[str], List[StageLog]]:
        """
        Step 2: Extract text from media and check for toxicity.
        
        For image/video: Use OCR/Whisper
        For PDF/Word: Extract text
        For PPT/Excel: Mark as PENDING_MODEL
        
        Args:
            material_list: List of {material_id, file_path, file_type, file_bytes}
            
        Returns:
            (is_flagged, flagged_materials, stage_logs)
        """
        step = 2
        start_time = time.time()
        stage_logs = []
        
        try:
            self.log_step_entry(self.STAGE, step, action="media_text_toxicity_check")
            
            if not material_list:
                # No materials to check
                latency_ms = (time.time() - start_time) * 1000
                log_entry = self.build_stage_log(
                    stage=self.STAGE,
                    step=step,
                    result="SKIPPED",
                    reason="No materials provided for extraction",
                    confidence_score=1.0,
                    latency_ms=latency_ms,
                )
                stage_logs.append(log_entry)
                return False, [], stage_logs
            
            flagged_materials = []
            max_confidence = 0.0
            materials_checked = 0
            materials_pending = 0
            
            for material in material_list:
                try:
                    material_id = material.get("material_id")
                    file_type = material.get("file_type", "").lower()
                    file_bytes = material.get("file_bytes")
                    
                    if not file_bytes:
                        self.logger.warning(f"No file bytes for material {material_id}")
                        continue
                    
                    # Skip pending model types
                    if file_type in ["pptx", "ppt", "xlsx", "xls"]:
                        materials_pending += 1
                        self.logger.info(f"Material {material_id} ({file_type}): PENDING_MODEL")
                        continue
                    
                    # Extract text
                    extracted_text, extraction_conf, extraction_log = await self.text_extractor.extract_generic(
                        content=file_bytes,
                        material_type=file_type,
                    )
                    
                    if not extracted_text or not extracted_text.strip():
                        self.logger.debug(f"No text extracted from material {material_id}")
                        continue
                    
                    materials_checked += 1
                    
                    # Classify extracted text
                    result, conf, details = await self.text_classifier.classify_text(extracted_text)
                    
                    if result == "FLAGGED":
                        flagged_materials.append(f"material_{material_id}")
                        max_confidence = max(max_confidence, conf)
                        self.logger.warning(
                            f"Flagged material {material_id} ({file_type}): {result} (conf: {conf})"
                        )
                
                except FileProcessingException as e:
                    self.logger.warning(f"Failed to extract text from material {material_id}: {e.message}")
                    continue
                except Exception as e:
                    self.logger.warning(f"Error processing material {material_id}: {e}")
                    continue
            
            is_flagged = len(flagged_materials) > 0
            
            latency_ms = (time.time() - start_time) * 1000
            
            log_entry = self.build_stage_log(
                stage=self.STAGE,
                step=step,
                result="FLAGGED" if is_flagged else "APPROVED",
                reason=(
                    f"Toxicity detected in extracted media text from materials: {', '.join(flagged_materials)}"
                    if is_flagged
                    else f"No toxicity in extracted media text ({materials_checked} materials checked, {materials_pending} pending vision model)"
                ),
                confidence_score=max_confidence if is_flagged else 1.0,
                flagged_content=flagged_materials if is_flagged else None,
                details={
                    "total_materials": len(material_list),
                    "materials_checked": materials_checked,
                    "materials_pending_vision": materials_pending,
                    "flagged_count": len(flagged_materials),
                },
                latency_ms=latency_ms,
            )
            
            stage_logs.append(log_entry)
            self.log_result(
                self.STAGE, step,
                result="FLAGGED" if is_flagged else "APPROVED",
                reason=log_entry["reason"],
                flagged_count=len(flagged_materials),
                pending_vision=materials_pending
            )
            
            return is_flagged, flagged_materials, stage_logs
        
        except Exception as e:
            self.log_error(self.STAGE, step, str(e))
            raise ModerationException(
                f"Media text toxicity check failed: {e}",
                code="MEDIA_TOXICITY_ERROR"
            )
    
    async def check_visual_toxicity(self) -> Tuple[bool, List[str], List[StageLog]]:
        """
        Step 3: Check media visuals for toxicity (DEFERRED).
        
        Requires trained vision models (ViT/CLIP/CNN ensemble) - not yet available.
        
        Returns:
            (False, [], [pending_log])
        """
        step = 3
        
        log_entry = self.build_stage_log(
            stage=self.STAGE,
            step=step,
            result="PENDING_MODEL",
            reason="Vision model (ViT/CLIP/CNN ensemble) not yet trained",
            confidence_score=0.0,
            details={
                "status": "DEFERRED",
                "note": "This step will be implemented after model training is complete",
            },
            latency_ms=0.0,
        )
        
        return False, [], [log_entry]
    
    async def orchestrate_stage2(
        self,
        course_id: int,
        title: Optional[str] = None,
        description: Optional[str] = None,
        lesson_texts: Optional[List[str]] = None,
        material_list: Optional[List[Dict[str, Any]]] = None,
    ) -> CourseModerationResponse:
        """
        Orchestrate Stage 2: Run toxicity checks in sequence.
        
        Returns FLAGGED immediately if Step 1 detects issues.
        Continues to Step 2 if Step 1 passes.
        Step 3 (vision) is logged as PENDING_MODEL.
        
        Args:
            course_id: Course to moderate
            title: Course title
            description: Course description
            lesson_texts: Lesson texts
            material_list: List of materials with extracted content
            
        Returns:
            CourseModerationResponse with full trace
        """
        stage_start = time.time()
        all_stage_logs = []
        
        # Load from Redis cache if not provided in the parameters
        if not title or not description:
            try:
                course_data = self.cache_repo.get_course_data(course_id)
                if course_data:
                    title = title or course_data.get("Title") or course_data.get("title") or ""
                    description = description or course_data.get("Description") or course_data.get("description") or ""
                    
                    # Extract lesson texts
                    if not lesson_texts:
                        lesson_texts = []
                        lessons = course_data.get("Lessons") or course_data.get("lessons") or []
                        for lesson in lessons:
                            lesson_title = lesson.get("Title") or lesson.get("title") or ""
                            lesson_text = lesson.get("LessonText") or lesson.get("lesson_text") or ""
                            if lesson_title:
                                lesson_texts.append(lesson_title)
                            if lesson_text:
                                lesson_texts.append(lesson_text)
                                
                    # Extract materials list
                    if not material_list:
                        material_list = []
                        lessons = course_data.get("Lessons") or course_data.get("lessons") or []
                        for lesson in lessons:
                            materials = lesson.get("LearningMaterials") or lesson.get("learning_materials") or []
                            for mat in materials:
                                material_list.append({
                                    "material_id": mat.get("MaterialId") or mat.get("material_id"),
                                    "content_text": mat.get("ContentText") or mat.get("content_text") or "",
                                    "file_url": mat.get("FileUrl") or mat.get("file_url") or "",
                                })
            except Exception as cache_err:
                self.logger.warning(f"Failed to fetch course details from cache: {cache_err}")

        # Ensure we have fallback string values
        title = title or ""
        description = description or ""
        lesson_texts = lesson_texts or []
        material_list = material_list or []

        try:
            self.log_stage_entry(self.STAGE, course_id, action="START")
            
            # ====================================================================
            # Step 1: Text Toxicity Check
            # ====================================================================
            try:
                text_flagged, flagged_fields, step1_logs = await self.check_text_toxicity(
                    title=title,
                    description=description,
                    lesson_texts=lesson_texts,
                )
                
                all_stage_logs.extend(step1_logs)
                
                # If text toxicity found, return immediately
                if text_flagged:
                    total_latency = (time.time() - stage_start) * 1000
                    self.logger.warning(
                        f"[Stage {self.STAGE}] FLAGGED for course {course_id}: "
                        f"Text toxicity detected - fields: {flagged_fields}"
                    )
                    
                    return CourseModerationResponse(
                        CourseId=course_id,
                        ModerationStatus=ModerationStatus.FLAGGED.value,
                        stageLogs=all_stage_logs,
                        flaggedFields=flagged_fields,
                        overall_confidence_score=step1_logs[0]["confidence_score"] if step1_logs else 0.9,
                        total_latency_ms=total_latency,
                    )
            
            except Exception as e:
                self.logger.error(f"Step 1 failed: {e}")
                all_stage_logs.append(self.build_stage_log(
                    stage=self.STAGE,
                    step=1,
                    result="ERROR",
                    reason=f"Text toxicity check failed: {e}",
                    confidence_score=0.0,
                    details={"error": str(e)},
                ))
            
            # ====================================================================
            # Step 2: Media Text Toxicity Check
            # ====================================================================
            try:
                media_flagged, flagged_materials, step2_logs = await self.check_media_text_toxicity(
                    material_list=material_list,
                )
                
                all_stage_logs.extend(step2_logs)
                
                # If media toxicity found, return FLAGGED
                if media_flagged:
                    total_latency = (time.time() - stage_start) * 1000
                    self.logger.warning(
                        f"[Stage {self.STAGE}] FLAGGED for course {course_id}: "
                        f"Media text toxicity detected - materials: {flagged_materials}"
                    )
                    
                    return CourseModerationResponse(
                        CourseId=course_id,
                        ModerationStatus=ModerationStatus.FLAGGED.value,
                        stageLogs=all_stage_logs,
                        flaggedFields=flagged_materials,
                        overall_confidence_score=step2_logs[0]["confidence_score"] if step2_logs else 0.85,
                        total_latency_ms=total_latency,
                    )
            
            except Exception as e:
                self.logger.error(f"Step 2 failed: {e}")
                all_stage_logs.append(self.build_stage_log(
                    stage=self.STAGE,
                    step=2,
                    result="ERROR",
                    reason=f"Media text toxicity check failed: {e}",
                    confidence_score=0.0,
                    details={"error": str(e)},
                ))
            
            # ====================================================================
            # Step 3: Vision Toxicity Check (DEFERRED)
            # ====================================================================
            try:
                _, _, step3_logs = await self.check_visual_toxicity()
                all_stage_logs.extend(step3_logs)
            except Exception as e:
                self.logger.error(f"Step 3 failed: {e}")
            
            # ====================================================================
            # No toxicity found - APPROVED
            # ====================================================================
            total_latency = (time.time() - stage_start) * 1000
            self.logger.info(f"[Stage {self.STAGE}] APPROVED for course {course_id}")
            
            # Calculate average confidence of all steps
            confidences = [log["confidence_score"] for log in all_stage_logs if log["result"] not in ["ERROR", "PENDING_MODEL"]]
            avg_confidence = sum(confidences) / len(confidences) if confidences else 1.0
            
            return CourseModerationResponse(
                CourseId=course_id,
                ModerationStatus=ModerationStatus.APPROVED.value,
                stageLogs=all_stage_logs,
                flaggedFields=[],
                overall_confidence_score=avg_confidence,
                total_latency_ms=total_latency,
            )
        
        except Exception as e:
            self.logger.error(f"Stage 2 orchestration failed: {e}")
            total_latency = (time.time() - stage_start) * 1000
            
            return CourseModerationResponse(
                CourseId=course_id,
                ModerationStatus=ModerationStatus.MANUAL_AUDIT.value,
                stageLogs=all_stage_logs,
                flaggedFields=[],
                overall_confidence_score=0.0,
                total_latency_ms=total_latency,
            )
