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
                overall_confidence_score=0.0,
                total_latency_ms=total_latency,
                stage_logs=[]
            )

        # 2. Check Text Harmful (Step 1)
        text_flagged, text_flagged_fields, step1_logs = await self.harmful_service.check_text_harmful(
            course=course,
            model_id=model_id,
            spam_threshold=spam_threshold,
            toxic_threshold=toxic_threshold
        )
        
        all_stage_logs.extend(step1_logs)
        all_flagged_fields = []
        all_flagged_fields.extend(text_flagged_fields)

        if text_flagged:
            self.logger.warning(f"Harmful text content detected for Course ID {course_id}. Flagged: {text_flagged_fields}")

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
        media_flagged, media_flagged_fields, step2_logs = await self.harmful_service.check_media_text_harmful(
            candidates=step2_candidates,
            model_id=model_id,
            spam_threshold=spam_threshold,
            toxic_threshold=toxic_threshold
        )
        
        all_stage_logs.extend(step2_logs)
        all_flagged_fields.extend(media_flagged_fields)
        total_latency = (time.time() - stage_start) * 1000

        if media_flagged:
            self.logger.warning(f"Harmful media content detected for Course ID {course_id}. Flagged: {media_flagged_fields}")
            
        if text_flagged or media_flagged:
            conf1 = step1_logs[0].get("confidence_score", 0.9) if (step1_logs and isinstance(step1_logs[0], dict)) else (step1_logs[0].confidence_score if step1_logs else 0.9)
            conf2 = step2_logs[0].get("confidence_score", 0.85) if (step2_logs and isinstance(step2_logs[0], dict)) else (step2_logs[0].confidence_score if step2_logs else 0.85)
            
            if text_flagged and media_flagged:
                overall_conf = max(conf1, conf2)
            elif text_flagged:
                overall_conf = conf1
            else:
                overall_conf = conf2

            return CourseModerationResponse(
                course_id=course_id,
                moderation_status=ModerationStatus.FLAGGED.value,
                flagged_fields=all_flagged_fields,
                overall_confidence_score=overall_conf,
                total_latency_ms=total_latency,
                stage_logs=all_stage_logs
            )

        self.logger.info(f"Stage 2 approved for Course ID {course_id}")
        return CourseModerationResponse(
            course_id=course_id,
            moderation_status=ModerationStatus.APPROVED.value,
            flagged_fields=[],
            overall_confidence_score=1.0,
            total_latency_ms=total_latency,
            stage_logs=all_stage_logs
        )
