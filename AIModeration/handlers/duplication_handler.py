import time
import httpx
import logging
import copy
import numpy as np
from typing import List, Optional
from core.models import (
    SemanticDuplicationRequest,
    CourseModerationResponse,
    StageLog,
    ModerationStatus,
    MaterialEmbeddingResponse,
    EmbeddingGenerationCommand,
    AiModelDto
)
from handlers.base_handler import BaseHandler
from services.duplication_service import DuplicationService
from services.embedding_service import EmbeddingService
from services.redis_service import RedisService
from services.text_extraction_service import TextExtractionService

logger = logging.getLogger(__name__)

class DuplicationHandler(BaseHandler):
    def __init__(
        self,
        duplication_service: Optional[DuplicationService] = None,
        embedding_service: Optional[EmbeddingService] = None,
        redis_service: Optional[RedisService] = None,
        text_extraction_service: Optional[TextExtractionService] = None
    ):
        super().__init__("DuplicationHandler")
        self.duplication_service = duplication_service or DuplicationService()
        self.embedding_service = embedding_service or EmbeddingService()
        self.redis_service = redis_service or RedisService()
        self.text_extraction_service = text_extraction_service or TextExtractionService()

    async def orchestrate_stage1(self, request: SemanticDuplicationRequest) -> CourseModerationResponse:
        """
        Orchestrate Stage 1: semantic deduplication checking.
        """
        stage_start = time.time()
        course_id = request.course_id
        material_ids = request.material_ids
        threshold = request.similarity_score_threshold
        
        self.logger.info(f"Starting Stage 1 orchestration for Course ID {course_id} with Similiarity Threshold {threshold}")

        # Process the models provided in request
        generators: List[AiModelDto] = self.process_request_models(request.models)
        all_stage_logs: List[StageLog] = []

        # 1. Fetch existing cached embeddings
        existing_embeddings = self.redis_service.get_all_existing_embeddings()

        # 2. Fetch course details
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

        # 3. Find matching materials from course details matching request material_ids
        material_id_set = set(material_ids)
        matches = []
        for lesson in course.lessons:
            for material in lesson.materials:
                if material.material_id in material_id_set:
                    matches.append(material)

        new_embeddings: List[MaterialEmbeddingResponse] = []
        last_model_id = 0

        # Try to resolve embedding generator model_id from generators list
        emb_model = None
        for g in generators:
            if g.model_type and "generator" in g.model_type.lower():
                emb_model = g
                break
        
        model_id = emb_model.model_id if emb_model else 0

        # Refined Plan: before the loop, drop new_embeddings and initialize new_embeddings_dict
        new_embeddings_dict = {}
        for g in generators:
            new_embeddings_dict[f"model_{g.model_id}"] = []

        # 4. Generate embeddings for each material
        async with httpx.AsyncClient() as client:
            for material in matches:
                try:
                    if not material.material_url:
                        self.logger.warning(f"Skipping material {material.material_id} as materialUrl is null or empty.")
                        continue

                    # Download bytes from cloudinary/URL
                    self.logger.info(f"Downloading material {material.material_id} from {material.material_url}")
                    resp = await client.get(material.material_url, timeout=30.0)
                    if resp.status_code != 200:
                        self.logger.error(f"Failed to download material {material.material_id}, status code {resp.status_code}")
                        continue
                    
                    file_bytes = resp.content
                    
                    # Resolve process type/file type mapping
                    file_ext = ""
                    if material.material_metadata and material.material_metadata.file_extension:
                        file_ext = material.material_metadata.file_extension
                    
                    # Mapping should follow allowed types in embedding_service, not text_extraction_service
                    mapped_type = self.embedding_service.get_file_type_for_embedding(file_ext)
                    
                    # Resolve embedding_type: "media" for image/video, "text" for text/pdf/word/powerpoint/excel
                    emb_type = "media" if mapped_type in ("image", "video") else "text"
                    
                    # Map generators based on process type if possible
                    selected_model_id = model_id
                    for g in generators:
                        if g.process_type and g.process_type.lower() in emb_type.lower():
                            selected_model_id = g.model_id
                            break

                    cmd = EmbeddingGenerationCommand(
                        material_id=material.material_id,
                        content=file_bytes,
                        model_id=selected_model_id,
                        material_type=mapped_type
                    )
                    
                    emb_res = await self.embedding_service.embed_generic(cmd)
                    self.logger.info(f"Embedding generated for material {emb_res.material_id}: {emb_res.embedding[:50]} \n ... First 50 coordinates")
                    target_key = f"model_{selected_model_id}"
                    if target_key not in new_embeddings_dict:
                        new_embeddings_dict[target_key] = []

                    new_embeddings_dict[target_key].append(MaterialEmbeddingResponse(
                        embedding_id=0,
                        material_id=emb_res.material_id,
                        embedding=emb_res.embedding,
                        embedding_type=emb_type
                    ))
                    
                    self.logger.info(f"Appended {len(new_embeddings_dict[target_key])} MaterialEmbeddingResponse for {target_key}")
                except Exception as e:
                    self.logger.error(f"Error processing material embedding for material {material.material_id}: {e}")
                    continue

        # 5. Perform semantic duplication check
        is_duplicate, matched_ids, similarity_scores, stage_logs = await self.duplication_service.check_semantic_duplication(
            new_embeddings_dict=new_embeddings_dict,
            existing_embeddings=existing_embeddings,
            threshold=threshold
        )
        
        logger.info(f"Similarity score(s): {[score for score in similarity_scores]}")
        
        all_stage_logs.extend(stage_logs)
        overall_confidence_score = float(np.mean(similarity_scores)) if similarity_scores else 1.0
        logger.info(f"Mean score: {overall_confidence_score}")
        total_latency = (time.time() - stage_start) * 1000

        if is_duplicate:
            self.logger.warning(f"Duplication detected for Course ID {course_id}. Flagged materials: {[f'material_{mat_id}' for mat_id in matched_ids]}")
            
            # Cache embeddings: ALL new embeddings go to PENDING cache awaiting human approval
            for key, lst in new_embeddings_dict.items():
                for new_emb in lst:
                    self.logger.info(f"Setting PENDING embeddings to cache for material {new_emb.material_id}")
                    self.redis_service.set_pending_material_embedding(new_emb)

            return CourseModerationResponse(
                course_id=course_id,
                moderation_status=ModerationStatus.FLAGGED.value,
                flagged_fields=[f"duplicate_of_material_{mid}" for mid in matched_ids],
                overall_confidence_score=overall_confidence_score,
                total_latency_ms=total_latency,
                stage_logs=all_stage_logs
            )

        # 6. If not duplicate, cache all new embeddings in Redis as PENDING
        for key, lst in new_embeddings_dict.items():
            for new_emb in lst:
                self.logger.info(f"Setting PENDING embeddings to cache {new_emb.embedding[:50]} ... for material {new_emb.material_id}")
                self.redis_service.set_pending_material_embedding(new_emb)

        self.logger.info(f"Stage 1 approved for Course ID {course_id}")
        return CourseModerationResponse(
            course_id=course_id,
            moderation_status=ModerationStatus.APPROVED.value,
            flagged_fields=[],
            overall_confidence_score=overall_confidence_score,
            total_latency_ms=total_latency,
            stage_logs=all_stage_logs
        )
