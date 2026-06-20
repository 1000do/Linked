"""Service for Stage 2: Toxicity & Spam Detection (Harmful)."""

import logging
import time
import numpy as np
from typing import Dict, List, Optional, Any, Tuple
from core.models import StageLog, CourseModerationResponse, ModerationStatus, CourseDetailDto
from core.exceptions import ModerationException, FileProcessingException
from services.base_service import BaseService
from services.text_classifier_service import TextClassifierService
from services.text_extraction_service import TextExtractionService
from repositories.cache_repository import CacheRepository

logger = logging.getLogger(__name__)


class HarmfulService(BaseService):
    """Handle Stage 2: Toxicity & Spam detection in text and media."""
    
    STAGE = 2
    
    def __init__(
        self,
        text_classifier: TextClassifierService = None,
        text_extractor: TextExtractionService = None,
        cache_repository: CacheRepository = None,
    ):
        """
        Initialize harmful service.
        """
        super().__init__("HarmfulService")
        self.text_classifier = text_classifier or TextClassifierService()
        self.text_extractor = text_extractor or TextExtractionService()
        self.cache_repo = cache_repository or CacheRepository()

    async def check_text_harmful(
        self,
        course: CourseDetailDto,
        model_id: int,
        spam_threshold: float,
        toxic_threshold: float
    ) -> Tuple[bool, List[str], List[StageLog]]:
        """
        Step 1: Check course.title, course.description, course.what_you_will_learn, course.requirements,
        lesson.title, learning_material.title, learning_material.description.
        """
        step = 1
        start_time = time.time()
        stage_logs = []
        flagged_fields = []
        aggregate_scores = []
        overall_details = {}

        async def check_field(field_name: str, val: Optional[str]):
            if not val or not val.strip():
                return
            try:
                res_action, conf_score, details = self.text_classifier.classify_text(
                    val,
                    spam_threshold=spam_threshold,
                    toxic_threshold=toxic_threshold
                )
                overall_details[field_name] = details
                if res_action == ModerationStatus.FLAGGED.value:
                    flagged_fields.append(field_name)
                    aggregate_scores.append(conf_score)
            except Exception as e:
                self.logger.warning(f"Failed to classify field {field_name}: {e}")

        # Course Level Fields
        await check_field("course.title", course.title)
        await check_field("course.description", course.description)
        await check_field("course.what_you_will_learn", course.what_you_will_learn)
        await check_field("course.requirements", course.requirements)

        # Lesson and Material Level Fields
        for idx_l, lesson in enumerate(course.lessons):
            await check_field(f"lesson_{lesson.lesson_id}.title", lesson.title)
            for idx_m, mat in enumerate(lesson.materials):
                await check_field(f"material_{mat.material_id}.title", mat.material_title)
                await check_field(f"material_{mat.material_id}.description", mat.material_description)

        is_flagged = len(flagged_fields) > 0
        latency_ms = (time.time() - start_time) * 1000
        confidence_score = float(np.mean(aggregate_scores)) if aggregate_scores else 1.0

        reason = (
            f"Harmful content detected in fields: {', '.join(flagged_fields)}"
            if is_flagged
            else f"No harmful content detected in text fields (spam_threshold: {spam_threshold}, toxic_threshold: {toxic_threshold})"
        )
        overall_details['flagged_count'] = len(flagged_fields)
        result_status = ModerationStatus.FLAGGED.value if is_flagged else ModerationStatus.APPROVED.value

        log_entry = self.build_stage_log(
            stage=self.STAGE,
            step=step,
            result=result_status,
            reason=reason,
            confidence_score=confidence_score,
            flagged_content=flagged_fields if is_flagged else None,
            details=overall_details,
            latency_ms=latency_ms,
            model_id=model_id
        )
        stage_logs.append(log_entry)
        return is_flagged, flagged_fields, stage_logs

    async def check_media_text_harmful(
        self,
        candidates: Dict[str, Any],
        model_id: int,
        spam_threshold: float,
        toxic_threshold: float
    ) -> Tuple[bool, List[str], List[StageLog]]:
        """
        Step 2: Extract text from course media / resources and perform harmful classification.
        """
        step = 2
        start_time = time.time()
        stage_logs = []

        if not candidates:
            latency_ms = (time.time() - start_time) * 1000
            log_entry = self.build_stage_log(
                stage=self.STAGE,
                step=step,
                result="SKIPPED",
                reason="No media candidates provided for extraction",
                confidence_score=1.0,
                latency_ms=latency_ms,
                model_id=model_id
            )
            stage_logs.append(log_entry)
            return False, [], stage_logs

        flagged_content = []
        aggregate_scores = []
        overall_details = {}
        candidates_checked = 0
        candidates_pending = 0

        for alias, value in candidates.items():
            file_type = value.get('file_type')
            file_bytes = value.get('file_bytes')
            logger.info(f'Checking media text on {alias} (file type: {file_type})...')

            if not file_bytes:
                continue

            try:
                candidates_checked += 1
                
                final_action = "APPROVED"
                final_conf = 1.0
                final_details = {"status": "APPROVED", "reason": "No segments processed"}
                
                segment_count = 0
                all_segment_details = []
                all_segment_scores = []
                
                async for chunk_text, extraction_conf, extraction_log in self.text_extractor.extract_generic_stream(
                    content=file_bytes,
                    material_type=file_type
                ):
                    if not chunk_text or not chunk_text.strip():
                        continue
                        
                    segment_count += 1
                    result_action, conf, details = self.text_classifier.classify_text(
                        chunk_text,
                        spam_threshold=spam_threshold,
                        toxic_threshold=toxic_threshold
                    )
                    
                    all_segment_details.append({
                        "segment": segment_count,
                        "source": extraction_log.get("source", "unknown"),
                        "text_snippet": chunk_text[:60] + "..." if len(chunk_text) > 60 else chunk_text,
                        "classification": details
                    })
                    all_segment_scores.append(conf)
                    
                    if result_action == ModerationStatus.FLAGGED.value:
                        console_log = f'''
                        {alias}
                        Source: {extraction_log.get("source", "unknown")} 
                        Segment {segment_count} was FLAGGED
                        First 100 characters: {chunk_text[:100]}
                        '''
                        final_action = "FLAGGED"
                        final_conf = conf
                        final_details = details
                        logger.warning(f'Fail-fast triggered for {console_log}')
                        break
                        # logger.warning(f"Harmful content detected on {console_log}")
                    elif result_action == "MANUAL_AUDIT" and final_action != "FLAGGED":
                        final_action = "MANUAL_AUDIT"
                        final_conf = conf
                        final_details = details

                if segment_count == 0:
                    overall_details[alias] = {"status": "SKIPPED", "reason": "No text extracted"}
                    continue

                if final_action == "APPROVED":
                    avg_conf = float(np.mean(all_segment_scores)) if all_segment_scores else 1.0
                    overall_details[alias] = {
                        "action": "APPROVED",
                        "score": avg_conf,
                        "raw_label": "SAFE",
                        "segments_processed": segment_count,
                        "details": all_segment_details
                    }
                else:
                    overall_details[alias] = final_details
                
                if final_action == ModerationStatus.FLAGGED.value:
                    flagged_content.append(alias)
                    aggregate_scores.append(final_conf)

            except Exception as e:
                self.logger.warning(f"Error extracting/classifying media {alias}: {e}")
                overall_details[alias] = {"status": "ERROR", "reason": str(e)}
                continue

        is_flagged = len(flagged_content) > 0
        latency_ms = (time.time() - start_time) * 1000
        confidence_score = float(np.mean(aggregate_scores)) if aggregate_scores else 1.0

        reason = (
            f"Harmful content detected in: {', '.join(flagged_content)}"
            if is_flagged
            else f"No harmful content detected in course media and resources (spam_threshold: {spam_threshold}, toxic_threshold: {toxic_threshold})"
        )
        overall_details['flagged_count'] = len(flagged_content)
        result_status = ModerationStatus.FLAGGED.value if is_flagged else ModerationStatus.APPROVED.value

        log_entry = self.build_stage_log(
            stage=self.STAGE,
            step=step,
            result=result_status,
            reason=reason,
            confidence_score=confidence_score,
            flagged_content=flagged_content if is_flagged else None,
            details=overall_details,
            latency_ms=latency_ms,
            model_id=model_id
        )
        stage_logs.append(log_entry)
        return is_flagged, flagged_content, stage_logs
