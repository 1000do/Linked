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

        flagged_fields, aggregate_scores, overall_details = await self._evaluate_fields(
            self._generate_text_fields(course), spam_threshold, toxic_threshold
        )

        is_flagged, latency_ms, confidence_score, reason, result_status = self._compute_stage_metrics(
            start_time=start_time,
            flagged_content=flagged_fields,
            aggregate_scores=aggregate_scores,
            overall_details=overall_details,
            spam_threshold=spam_threshold,
            toxic_threshold=toxic_threshold,
            reason_flagged_prefix="Harmful content detected in fields: ",
            reason_clean="No harmful content detected in text fields"
        )

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

    def _generate_text_fields(self, course: CourseDetailDto):
        yield "course.title", course.title
        yield "course.description", course.description
        yield "course.what_you_will_learn", course.what_you_will_learn
        yield "course.requirements", course.requirements

        for lesson in course.lessons:
            yield f"lesson_{lesson.lesson_id}.title", lesson.title
            for mat in lesson.materials:
                yield f"material_{mat.material_id}.title", mat.material_title
                yield f"material_{mat.material_id}.description", mat.material_description

    async def _evaluate_fields(
        self, fields_iterator, spam_threshold: float, toxic_threshold: float
    ) -> Tuple[List[str], List[float], Dict[str, Any]]:
        flagged_fields = []
        aggregate_scores = []
        details_map = {}

        for f_name, f_val in fields_iterator:
            is_flagged, score, details = await self._classify_field_text(f_name, f_val, spam_threshold, toxic_threshold)
            if details is not None:
                details_map[f_name] = details
            if is_flagged:
                flagged_fields.append(f_name)
                if score is not None:
                    aggregate_scores.append(score)

        return flagged_fields, aggregate_scores, details_map

    async def _classify_field_text(
        self, field_name: str, val: Optional[str], spam_threshold: float, toxic_threshold: float
    ) -> Tuple[bool, Optional[float], Optional[Dict]]:
        if not val or not val.strip():
            return False, None, None
        try:
            res_action, conf_score, details = self.text_classifier.classify_text(
                val,
                spam_threshold=spam_threshold,
                toxic_threshold=toxic_threshold
            )
            is_flagged = res_action == ModerationStatus.FLAGGED.value
            return is_flagged, conf_score, details
        except Exception as e:
            self.logger.warning(f"Failed to classify field {field_name}: {e}")
            return False, None, None

    def _compute_stage_metrics(
        self,
        start_time: float,
        flagged_content: List[str],
        aggregate_scores: List[float],
        overall_details: Dict[str, Any],
        spam_threshold: float,
        toxic_threshold: float,
        reason_flagged_prefix: str,
        reason_clean: str
    ) -> Tuple[bool, float, float, str, str]:
        is_flagged = len(flagged_content) > 0
        latency_ms = (time.time() - start_time) * 1000
        confidence_score = float(np.mean(aggregate_scores)) if aggregate_scores else 1.0

        reason = (
            f"{reason_flagged_prefix}{', '.join(flagged_content)}"
            if is_flagged
            else f"{reason_clean} (spam_threshold: {spam_threshold}, toxic_threshold: {toxic_threshold})"
        )
        overall_details['flagged_count'] = len(flagged_content)
        result_status = ModerationStatus.FLAGGED.value if is_flagged else ModerationStatus.APPROVED.value

        return is_flagged, latency_ms, confidence_score, reason, result_status

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
            stage_logs.append(self._handle_empty_media_candidates(step, start_time, model_id))
            return False, [], stage_logs

        flagged_content, aggregate_scores, overall_details = await self._evaluate_media_candidates(
            candidates, spam_threshold, toxic_threshold
        )

        is_flagged, latency_ms, confidence_score, reason, result_status = self._compute_stage_metrics(
            start_time=start_time,
            flagged_content=flagged_content,
            aggregate_scores=aggregate_scores,
            overall_details=overall_details,
            spam_threshold=spam_threshold,
            toxic_threshold=toxic_threshold,
            reason_flagged_prefix="Harmful content detected in: ",
            reason_clean="No harmful content detected in course media and resources"
        )

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

    async def _evaluate_media_candidates(
        self, candidates: Dict[str, Any], spam_threshold: float, toxic_threshold: float
    ) -> Tuple[List[str], List[float], Dict[str, Any]]:
        flagged_content = []
        aggregate_scores = []
        overall_details = {}

        for alias, value in candidates.items():
            is_flagged, final_conf, final_details = await self._process_single_media_candidate(
                alias, value, spam_threshold, toxic_threshold
            )
            overall_details[alias] = final_details
            if is_flagged:
                flagged_content.append(alias)
                aggregate_scores.append(final_conf)

        return flagged_content, aggregate_scores, overall_details

    def _handle_empty_media_candidates(self, step: int, start_time: float, model_id: int) -> StageLog:
        latency_ms = (time.time() - start_time) * 1000
        return self.build_stage_log(
            stage=self.STAGE,
            step=step,
            result="SKIPPED",
            reason="No media candidates provided for extraction",
            confidence_score=1.0,
            latency_ms=latency_ms,
            model_id=model_id
        )

    async def _process_single_media_candidate(
        self, alias: str, value: Dict[str, Any], spam_threshold: float, toxic_threshold: float
    ) -> Tuple[bool, float, Dict[str, Any]]:
        file_type = value.get('file_type')
        file_bytes = value.get('file_bytes')
        logger.info(f'Checking media text on {alias} (file type: {file_type})...')

        if not file_bytes:
            return False, 1.0, {"status": "SKIPPED", "reason": "No file bytes provided"}

        try:
            return await self._process_media_content_batch(alias, file_bytes, file_type, spam_threshold, toxic_threshold)
        except Exception as e:
            self.logger.warning(f"Error extracting/classifying media {alias}: {e}")
            return False, 1.0, {"status": "ERROR", "reason": str(e)}

    async def _process_media_content_batch(
        self, alias: str, file_bytes: bytes, file_type: str, spam_threshold: float, toxic_threshold: float
    ) -> Tuple[bool, float, Dict[str, Any]]:
        chunk_texts, extraction_conf, extraction_log = await self.text_extractor.extract_generic_legacy(
            content=file_bytes,
            material_type=file_type
        )
        
        if not chunk_texts:
            return False, 1.0, {"status": "SKIPPED", "reason": "No text extracted"}
            
        result_action, conf, details = self.text_classifier.classify_text_list(
            chunk_texts,
            spam_threshold=spam_threshold,
            toxic_threshold=toxic_threshold
        )
        
        final_details = {
            "source": extraction_log.get("source", "unknown"),
            "text_length": sum(len(t) for t in chunk_texts),
            "classification": details
        }
        
        is_flagged = result_action == ModerationStatus.FLAGGED.value
        if is_flagged:
            logger.warning(f"Flagged media batch: {alias}. Source: {extraction_log.get('source', 'unknown')}. Conf: {conf}")
        
        return is_flagged, conf, final_details

    async def _process_media_segments(
        self, alias: str, file_bytes: bytes, file_type: str, spam_threshold: float, toxic_threshold: float
    ) -> Tuple[bool, float, Dict[str, Any]]:
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
            elif result_action == "MANUAL_AUDIT" and final_action != "FLAGGED":
                final_action = "MANUAL_AUDIT"
                final_conf = conf
                final_details = details

        if segment_count == 0:
            return False, 1.0, {"status": "SKIPPED", "reason": "No text extracted"}

        if final_action == "APPROVED":
            avg_conf = float(np.mean(all_segment_scores)) if all_segment_scores else 1.0
            overall_detail = {
                "action": "APPROVED",
                "score": avg_conf,
                "raw_label": "SAFE",
                "segments_processed": segment_count,
                "details": all_segment_details
            }
            return False, avg_conf, overall_detail
        
        is_flagged = final_action == ModerationStatus.FLAGGED.value
        return is_flagged, final_conf, final_details
