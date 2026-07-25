import time
import httpx
import logging
from typing import List, Optional, Dict, Any
from core.models import (
    HarmfulCourseRequest,
    CourseModerationResponse,
    StageLog,
    ModerationStatus,
    AiModelDto
)
from handlers.base_handler import BaseHandler
from services.harmful_service import HarmfulService
from services.redis_service import RedisService
from services.text_extraction_service import TextExtractionService

logger = logging.getLogger(__name__)

class HarmfulHandler(BaseHandler):
    def __init__(
        self,
        harmful_service: Optional[HarmfulService] = None,
        redis_service: Optional[RedisService] = None,
        text_extraction_service: Optional[TextExtractionService] = None
    ):
        super().__init__("HarmfulHandler")
        self.harmful_service = harmful_service or HarmfulService()
        self.redis_service = redis_service or RedisService()
        self.text_extraction_service = text_extraction_service or TextExtractionService()

    def _map_result_to_priority(self, result: str) -> int:
        priority = {
            ModerationStatus.APPROVED.value: 1,
            ModerationStatus.PENDING.value: 2,
            ModerationStatus.MANUAL_AUDIT.value: 3,
            ModerationStatus.FLAGGED.value: 4,
        }
        return priority.get(result, 0)

    def _compute_overall_conf(self, res1: str, res2: str, conf1: float, conf2: float) -> float:
        prior1 = self._map_result_to_priority(res1)
        prior2 = self._map_result_to_priority(res2)
        if prior1 == prior2:
            return (conf1 + conf2) / 2
        elif prior1 > prior2:
            return conf1
        else:
            return conf2

    def _aggregate_result(self, res1: str, res2: str) -> str:
        prior1 = self._map_result_to_priority(res1)
        prior2 = self._map_result_to_priority(res2)
        if prior1 >= prior2:
            return res1
        else:
            return res2

    async def orchestrate_stage2(self, request: HarmfulCourseRequest) -> CourseModerationResponse:
        """
        Orchestrate Stage 2 harmful (toxicity & spam) detection.
        """
        stage_start = time.time()
        course_id = request.course_id
        spam_threshold = request.spam_score_threshold
        toxic_threshold = request.toxic_score_threshold

        self.logger.info(f"Starting Stage 2 orchestration for Course ID {course_id} with Spam Threshold {spam_threshold} and Toxic Threshold {toxic_threshold}")

        # Process the models provided in request
        classifiers: List[AiModelDto] = self.process_request_models(request.models)
        all_stage_logs: List[StageLog] = []

        # Find model ID for harmful text classifier
        model_id = 0
        for m in classifiers:
            if m.model_name and 'harmful_text_classifier' in m.model_name.lower():
                model_id = m.model_id
                break
        if not model_id and classifiers:
            model_id = classifiers[0].model_id

        # 1. Fetch course details
        course = self.redis_service.get_course_details(course_id)
        if not course:
            self.logger.warning(f"Course details not found in cache for Course ID {course_id}")
            total_latency = (time.time() - stage_start) * 1000
            return CourseModerationResponse(
                course_id=course_id,
                moderation_status=ModerationStatus.MANUAL_AUDIT.value,
                flagged_fields=[],
                manual_audit_fields=["all"],
                overall_confidence_score=0.0,
                total_latency_ms=total_latency,
                stage_logs=[]
            )

        all_flagged_fields = []
        all_manual_audit_fields = []

        # 2. Check Text Harmful (Step 1)
        step1_result, step1_conf, text_flagged_fields, text_manual_audit_fields, step1_logs = await self.harmful_service.check_text_harmful(
            course=course,
            model_id=model_id,
            spam_threshold=spam_threshold,
            toxic_threshold=toxic_threshold
        )
        
        all_stage_logs.extend(step1_logs)
        all_flagged_fields.extend(text_flagged_fields)
        all_manual_audit_fields.extend(text_manual_audit_fields)

        text_flagged = len(text_flagged_fields) > 0
        text_manual_audit = len(text_manual_audit_fields) > 0

        flagged_log_string = f"Harmful text content detected for Course ID {course_id}. Flagged: {text_flagged_fields}" if text_flagged else ""
        manual_log_string = f"Suspicious text content detected for Course ID {course_id}. Fields: {text_manual_audit_fields}" if text_manual_audit else ""
        
        if text_flagged:
            self.logger.warning(f"{flagged_log_string}. {manual_log_string}")
        elif text_manual_audit:
            self.logger.warning(manual_log_string)

        # 3. Process candidates for media checking (Step 2)
        step2_candidates: Dict[str, Any] = {}
        async with httpx.AsyncClient() as client:
            # Course Thumbnail
            if course.thumbnail_url:
                try:
                    self.logger.info(f"Downloading course thumbnail: {course.thumbnail_url}")
                    resp = await client.get(course.thumbnail_url, timeout=30.0)
                    if resp.status_code == 200:
                        step2_candidates['course.thumbnail'] = {
                            'file_type': 'image',
                            'file_bytes': resp.content
                        }
                except Exception as e:
                    self.logger.error(f"Failed to download course thumbnail: {e}")

            # Learning Materials
            for lesson in course.lessons:
                for material in lesson.materials:
                    if material.material_url:
                        try:
                            self.logger.info(f"Downloading material {material.material_id}: {material.material_url}")
                            resp = await client.get(material.material_url, timeout=30.0)
                            if resp.status_code == 200:
                                file_ext = ""
                                if material.material_metadata and material.material_metadata.file_extension:
                                    file_ext = material.material_metadata.file_extension
                                mapped_type = self.text_extraction_service.get_file_type_for_text_extraction(file_ext)
                                
                                step2_candidates[f"material_{material.material_id}"] = {
                                    'file_type': mapped_type,
                                    'file_bytes': resp.content
                                }
                        except Exception as e:
                            self.logger.error(f"Failed to download material {material.material_id} for harmful check: {e}")

        # Check Media Harmful (Step 2)
        step2_result, step2_conf, media_flagged_fields, media_manual_audit_fields, step2_logs = await self.harmful_service.check_media_text_harmful(
            candidates=step2_candidates,
            model_id=model_id,
            spam_threshold=spam_threshold,
            toxic_threshold=toxic_threshold
        )
        
        all_stage_logs.extend(step2_logs)
        all_flagged_fields.extend(media_flagged_fields)
        all_manual_audit_fields.extend(media_manual_audit_fields)
        total_latency = (time.time() - stage_start) * 1000

        media_flagged = len(media_flagged_fields) > 0
        media_manual_audit = len(media_manual_audit_fields) > 0

        flagged_log_string = f"Harmful media content detected for Course ID {course_id}. Flagged: {media_flagged_fields}" if media_flagged else ""
        manual_log_string = f"Suspicious media content detected for Course ID {course_id}. Files: {media_manual_audit_fields}" if media_manual_audit else ""
        
        if media_flagged:
            self.logger.warning(f"{flagged_log_string}. {manual_log_string}")
        elif media_manual_audit:
            self.logger.warning(manual_log_string)

        overall_conf = self._compute_overall_conf(step1_result, step2_result, step1_conf, step2_conf)
        moderation_status = self._aggregate_result(step1_result, step2_result)

        if moderation_status == ModerationStatus.APPROVED.value:
            self.logger.info(f"Stage 2 approved for Course ID {course_id}")

        return CourseModerationResponse(
            course_id=course_id,
            moderation_status=moderation_status,
            flagged_fields=all_flagged_fields,
            manual_audit_fields=all_manual_audit_fields,
            overall_confidence_score=overall_conf,
            total_latency_ms=total_latency,
            stage_logs=all_stage_logs
        )
