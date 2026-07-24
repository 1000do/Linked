"""Service for Stage 2: Toxicity & Spam Detection (Harmful)."""

import logging
import time
import numpy as np
from typing import Dict, List, Optional, Any, Tuple
from core.models import StageLog, CourseModerationResponse, ModerationStatus, HarmfulClassificationLabel, CourseDetailDto
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
    ) -> Tuple[str, float, List[str], List[str], List[StageLog]]:
        """
        Step 1: Check course.title, course.description, course.what_you_will_learn, course.requirements,
        lesson.title, learning_material.title, learning_material.description.
        """
        step = 1
        start_time = time.time()
        stage_logs = []

        flagged_fields, manual_audit_fields, aggregate_scores, overall_details = await self._evaluate_fields(
            self._generate_text_fields(course), spam_threshold, toxic_threshold
        )

        step_result, reason, confidence_score, overall_details, latency_ms = self._compute_stage_metrics(
            start_time=start_time,
            flagged_fields=flagged_fields,
            manual_audit_fields=manual_audit_fields,
            aggregate_scores=aggregate_scores,
            overall_details=overall_details,
            spam_threshold=spam_threshold,
            toxic_threshold=toxic_threshold,
            reason_flagged_prefix="Harmful content detected in fields: ",
            reason_manual_audit_prefix="Suspicious content detected in fields: ",
            reason_clean="No harmful content detected in text fields"
        )

        log_entry = self.build_stage_log(
            stage=self.STAGE,
            step=step,
            result=step_result,
            reason=reason,
            confidence_score=confidence_score,
            flagged_content=flagged_fields if len(flagged_fields) > 0 else None,
            manual_audit_content=manual_audit_fields if len(manual_audit_fields) > 0 else None,
            details=overall_details,
            latency_ms=latency_ms,
            model_id=model_id
        )
        stage_logs.append(log_entry)
        return step_result, confidence_score, flagged_fields, manual_audit_fields, stage_logs

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
    ) -> Tuple[List[str], List[str], List[float], Dict[str, Any]]:
        flagged_fields = []
        manual_audit_fields = []
        flagged_scores = []
        manual_audit_scores = []
        approved_scores = []
        details_map = {}

        for f_name, f_val in fields_iterator:
            res_action, score, details = await self._classify_field_text(f_name, f_val, spam_threshold, toxic_threshold)
            if details is not None:
                details_map[f_name] = details
            
            if res_action == ModerationStatus.FLAGGED.value:
                flagged_fields.append(f_name)
                if score is not None:
                    flagged_scores.append(score)
            elif res_action == ModerationStatus.MANUAL_AUDIT.value:
                manual_audit_fields.append(f_name)
                if score is not None:
                    manual_audit_scores.append(score)
            elif res_action == ModerationStatus.APPROVED.value:
                if score is not None:
                    approved_scores.append(score)

        if len(flagged_fields) > 0:
            aggregate_scores = flagged_scores
        elif len(manual_audit_fields) > 0:
            aggregate_scores = manual_audit_scores
        else:
            aggregate_scores = approved_scores

        return flagged_fields, manual_audit_fields, aggregate_scores, details_map

    async def _classify_field_text(
        self, field_name: str, val: Optional[str], spam_threshold: float, toxic_threshold: float
    ) -> Tuple[str, Optional[float], Optional[Dict]]:
        if not val or not val.strip():
            return ModerationStatus.APPROVED.value, 1.0, None
        try:
            res_action, conf_score, details = self.text_classifier.classify_text(
                val,
                spam_threshold=spam_threshold,
                toxic_threshold=toxic_threshold
            )
            return res_action, conf_score, details
        except Exception as e:
            self.logger.warning(f"Failed to classify field {field_name}: {e}")
            return ModerationStatus.APPROVED.value, 1.0, None

    def _compute_stage_metrics(
        self,
        start_time: float,
        flagged_fields: List[str],
        manual_audit_fields: List[str],
        aggregate_scores: List[float],
        overall_details: Dict[str, Any],
        spam_threshold: float,
        toxic_threshold: float,
        reason_flagged_prefix: str,
        reason_manual_audit_prefix: str,
        reason_clean: str
    ) -> Tuple[str, str, float, Dict[str, Any], float]:
        len_flagged = len(flagged_fields)
        len_manual = len(manual_audit_fields)
        latency_ms = (time.time() - start_time) * 1000
        confidence_score = float(np.mean(aggregate_scores)) if aggregate_scores else 1.0

        if len_flagged > 0:
            step_result = ModerationStatus.FLAGGED.value
            reason = f"{reason_flagged_prefix}{', '.join(flagged_fields)}"
            if len_manual > 0:
                reason += f"\n{reason_manual_audit_prefix}{', '.join(manual_audit_fields)}"
        elif len_manual > 0:
            step_result = ModerationStatus.MANUAL_AUDIT.value
            reason = f"{reason_manual_audit_prefix}{', '.join(manual_audit_fields)}"
        else:
            step_result = ModerationStatus.APPROVED.value
            reason = f"{reason_clean} (spam_threshold: {spam_threshold}, toxic_threshold: {toxic_threshold})"

        overall_details['flagged_count'] = len_flagged
        overall_details['manual_audit_count'] = len_manual

        return step_result, reason, confidence_score, overall_details, latency_ms

    async def check_media_text_harmful(
        self,
        candidates: Dict[str, Any],
        model_id: int,
        spam_threshold: float,
        toxic_threshold: float
    ) -> Tuple[str, float, List[str], List[str], List[StageLog]]:
        """
        Step 2: Extract text from course media / resources and perform harmful classification.
        """
        step = 2
        start_time = time.time()
        stage_logs = []

        if not candidates:
            stage_logs.append(self._handle_empty_media_candidates(step, start_time, model_id))
            return ModerationStatus.SKIPPED.value, 1.0, [], [], stage_logs

        flagged_fields, manual_audit_fields, aggregate_scores, overall_details = await self._evaluate_media_candidates(
            candidates, spam_threshold, toxic_threshold
        )

        step_result, reason, confidence_score, overall_details, latency_ms = self._compute_stage_metrics(
            start_time=start_time,
            flagged_fields=flagged_fields,
            manual_audit_fields=manual_audit_fields,
            aggregate_scores=aggregate_scores,
            overall_details=overall_details,
            spam_threshold=spam_threshold,
            toxic_threshold=toxic_threshold,
            reason_flagged_prefix="Harmful media content detected in: ",
            reason_manual_audit_prefix="Suspicious media content detected in: ",
            reason_clean="No harmful content detected in course media and resources"
        )

        log_entry = self.build_stage_log(
            stage=self.STAGE,
            step=step,
            result=step_result,
            reason=reason,
            confidence_score=confidence_score,
            flagged_content=flagged_fields if len(flagged_fields) > 0 else None,
            manual_audit_content=manual_audit_fields if len(manual_audit_fields) > 0 else None,
            details=overall_details,
            latency_ms=latency_ms,
            model_id=model_id
        )
        stage_logs.append(log_entry)
        return step_result, confidence_score, flagged_fields, manual_audit_fields, stage_logs

    async def _evaluate_media_candidates(
        self, candidates: Dict[str, Any], spam_threshold: float, toxic_threshold: float
    ) -> Tuple[List[str], List[str], List[float], Dict[str, Any]]:
        flagged_fields = []
        manual_audit_fields = []
        flagged_scores = []
        manual_audit_scores = []
        approved_scores = []
        overall_details = {}

        for alias, value in candidates.items():
            res_action, final_conf, final_details = await self._process_single_media_candidate(
                alias, value, spam_threshold, toxic_threshold
            )
            overall_details[alias] = final_details
            
            if res_action == ModerationStatus.FLAGGED.value:
                flagged_fields.append(alias)
                if final_conf is not None:
                    flagged_scores.append(final_conf)
            elif res_action == ModerationStatus.MANUAL_AUDIT.value:
                manual_audit_fields.append(alias)
                if final_conf is not None:
                    manual_audit_scores.append(final_conf)
            elif res_action == ModerationStatus.APPROVED.value:
                if final_conf is not None:
                    approved_scores.append(final_conf)

        if len(flagged_fields) > 0:
            aggregate_scores = flagged_scores
        elif len(manual_audit_fields) > 0:
            aggregate_scores = manual_audit_scores
        else:
            aggregate_scores = approved_scores

        return flagged_fields, manual_audit_fields, aggregate_scores, overall_details

    def _handle_empty_media_candidates(self, step: int, start_time: float, model_id: int) -> StageLog:
        latency_ms = (time.time() - start_time) * 1000
        return self.build_stage_log(
            stage=self.STAGE,
            step=step,
            result=ModerationStatus.SKIPPED.value,
            reason="No media candidates provided for extraction",
            confidence_score=1.0,
            latency_ms=latency_ms,
            model_id=model_id
        )

    async def _process_single_media_candidate(
        self, alias: str, value: Dict[str, Any], spam_threshold: float, toxic_threshold: float
    ) -> Tuple[str, float, Dict[str, Any]]:
        file_type = value.get('file_type')
        file_bytes = value.get('file_bytes')
        logger.info(f'Checking media text on {alias} (file type: {file_type})...')

        if not file_bytes:
            return ModerationStatus.MANUAL_AUDIT.value, 1.0, {"status": ModerationStatus.SKIPPED.value, "reason": "No file bytes provided"}

        try:
            return await self._process_media_content_batch(alias, file_bytes, file_type, spam_threshold, toxic_threshold)
        except Exception as e:
            self.logger.warning(f"Error extracting/classifying media {alias}: {e}")
            return ModerationStatus.MANUAL_AUDIT.value, 1.0, {"status": "ERROR", "reason": str(e)}

    async def _process_media_content_batch(
        self, alias: str, file_bytes: bytes, file_type: str, spam_threshold: float, toxic_threshold: float
    ) -> Tuple[str, float, Dict[str, Any]]:
        chunk_texts, extraction_conf, extraction_log = await self.text_extractor.extract_generic_legacy(
            content=file_bytes,
            material_type=file_type
        )
        
        if not chunk_texts:
            return ModerationStatus.APPROVED.value, 1.0, {"status": ModerationStatus.SKIPPED.value, "reason": "No text extracted"}
            
        result_action, conf, details = self.text_classifier.classify_text_list(
            chunk_texts,
            spam_threshold=spam_threshold,
            toxic_threshold=toxic_threshold
        )
        
        final_conf = conf
        final_details = {
            "extraction": extraction_log,
            "classification": details
        }
        
        if result_action == ModerationStatus.FLAGGED.value:
            self.logger.warning(f"Media {alias} flagged! Action: {result_action}, Conf: {final_conf:.2f}")
            
        return result_action, final_conf, final_details

    async def _process_media_segments(
        self, alias: str, file_bytes: bytes, file_type: str, spam_threshold: float, toxic_threshold: float
    ) -> Tuple[bool, float, Dict[str, Any]]:
        final_action = ModerationStatus.APPROVED.value
        final_conf = 1.0
        final_details = {"status": ModerationStatus.APPROVED.value, "reason": "No segments processed"}
        
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
                final_action = ModerationStatus.FLAGGED.value
                final_conf = conf
                final_details = details
                logger.warning(f'Fail-fast triggered for {console_log}')
                break
            elif result_action == ModerationStatus.MANUAL_AUDIT.value and final_action != ModerationStatus.FLAGGED.value:
                final_action = ModerationStatus.MANUAL_AUDIT.value
                final_conf = conf
                final_details = details

        if segment_count == 0:
            return False, 1.0, {"status": ModerationStatus.SKIPPED.value, "reason": "No text extracted"}

        if final_action == ModerationStatus.APPROVED.value:
            avg_conf = float(np.mean(all_segment_scores)) if all_segment_scores else 1.0
            overall_detail = {
                "action": ModerationStatus.APPROVED.value,
                "score": avg_conf,
                "raw_label": HarmfulClassificationLabel.SAFE.value,
                "segments_processed": segment_count,
                "details": all_segment_details
            }
            return False, avg_conf, overall_detail
        
        is_flagged = final_action == ModerationStatus.FLAGGED.value
        return is_flagged, final_conf, final_details
