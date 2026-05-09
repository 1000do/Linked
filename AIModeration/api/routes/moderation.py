"""API routes for moderation endpoints."""

import logging
from fastapi import APIRouter, HTTPException, Depends, UploadFile, File, Form
from typing import List, Optional
import json

from core.models import (
    DuplicationRequest, ToxicityRequest, ModerationResponse, HealthResponse,
    EmbeddingGenerationRequest, EmbeddingGenerationResponse,
)
from core.exceptions import ModerationException
from services.duplication_service import DuplicationService
from services.toxicity_service import ToxicityService
from services.embedding_service import EmbeddingService
from repositories.cache_repository import CacheRepository
from config.settings import get_settings

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/moderation", tags=["moderation"])

# ============================================================================
# DEPENDENCY INJECTION
# ============================================================================

def get_cache_repository() -> CacheRepository:
    """Get cache repository instance."""
    return CacheRepository()

def get_duplication_service(cache_repo: CacheRepository = Depends(get_cache_repository)) -> DuplicationService:
    """Get duplication service instance."""
    return DuplicationService(cache_repository=cache_repo)

def get_toxicity_service() -> ToxicityService:
    """Get toxicity service instance."""
    return ToxicityService()

def get_embedding_service() -> EmbeddingService:
    """Get embedding service instance."""
    return EmbeddingService()

# ============================================================================
# STAGE 1: DUPLICATION DETECTION
# ============================================================================

@router.post("/stage1", response_model=ModerationResponse)
async def moderation_stage1(
    request: DuplicationRequest,
    service: DuplicationService = Depends(get_duplication_service),
):
    """
    Stage 1: Check for exact and semantic duplication.
    
    Returns ModerationResponse with full stage logs (Step 1 & Step 2).
    """
    try:
        logger.info(f"Processing Stage 1 for course {request.course_id}")
        
        response = await service.orchestrate_stage1(
            course_id=request.course_id,
            title_hash=request.title_hash,
            description_hash=request.description_hash,
            thumbnail_hash=request.thumbnail_hash,
            material_hashes=request.material_hashes,
            material_ids=list(range(len(request.material_embeddings or []))),
            material_embeddings=request.material_embeddings,
        )
        
        return response
    
    except ModerationException as e:
        logger.error(f"Moderation error in Stage 1: {e.message}")
        raise HTTPException(
            status_code=422,
            detail={
                "error": "moderation_error",
                "message": e.message,
                "code": e.code,
            }
        )
    except Exception as e:
        logger.error(f"Unexpected error in Stage 1: {e}")
        raise HTTPException(
            status_code=500,
            detail={
                "error": "internal_error",
                "message": str(e),
            }
        )

# ============================================================================
# STAGE 2: TOXICITY & SPAM DETECTION
# ============================================================================

@router.post("/stage2", response_model=ModerationResponse)
async def moderation_stage2(
    request: ToxicityRequest,
    service: ToxicityService = Depends(get_toxicity_service),
):
    """
    Stage 2: Check for toxicity and spam in text and media.
    
    Returns ModerationResponse with full stage logs (Step 1, Step 2, Step 3).
    """
    try:
        logger.info(f"Processing Stage 2 for course {request.course_id}")
        
        response = await service.orchestrate_stage2(
            course_id=request.course_id,
            title=request.title,
            description=request.description,
            lesson_texts=request.lesson_texts,
            material_list=request.material_list,
        )
        
        return response
    
    except ModerationException as e:
        logger.error(f"Moderation error in Stage 2: {e.message}")
        raise HTTPException(
            status_code=422,
            detail={
                "error": "moderation_error",
                "message": e.message,
                "code": e.code,
            }
        )
    except Exception as e:
        logger.error(f"Unexpected error in Stage 2: {e}")
        raise HTTPException(
            status_code=500,
            detail={
                "error": "internal_error",
                "message": str(e),
            }
        )

# ============================================================================
# EMBEDDING GENERATION
# ============================================================================

@router.post("/embeddings/generate", response_model=EmbeddingGenerationResponse)
async def generate_embedding(
    request: EmbeddingGenerationRequest,
    service: EmbeddingService = Depends(get_embedding_service),
):
    """
    Generate embedding for material.
    
    Used by C# backend during course creation to pre-compute embeddings
    for semantic deduplication checks.
    
    Supports:
    - text: Direct text embedding
    - image: CLIP vision embedding
    - video: CLIP embedding of sampled frames
    - pdf: DistilBert embedding of extracted text
    - word: DistilBert embedding of extracted text
    """
    try:
        logger.info(f"Generating embedding for material {request.material_id} ({request.material_type})")
        
        embedding = await service.embed_generic(
            content=request.content,
            material_type=request.material_type,
        )
        
        return EmbeddingGenerationResponse(
            material_id=request.material_id,
            embedding=embedding,
            success=True,
        )
    
    except Exception as e:
        logger.error(f"Embedding generation failed for material {request.material_id}: {e}")
        raise HTTPException(
            status_code=422,
            detail={
                "error": "embedding_generation_error",
                "message": str(e),
                "material_id": request.material_id,
            }
        )

# ============================================================================
# FULL PIPELINE (Stages 1 + 2)
# ============================================================================

@router.post("/full-pipeline", response_model=ModerationResponse)
async def full_moderation_pipeline(
    course_id: int,
    duplication_request: DuplicationRequest,
    toxicity_request: ToxicityRequest,
    dup_service: DuplicationService = Depends(get_duplication_service),
    tox_service: ToxicityService = Depends(get_toxicity_service),
):
    """
    Run full moderation pipeline (Stage 1 + Stage 2).
    
    Returns combined ModerationResponse with all logs.
    """
    try:
        logger.info(f"Processing full pipeline for course {course_id}")
        
        # Stage 1
        stage1_response = await dup_service.orchestrate_stage1(
            course_id=duplication_request.course_id,
            title_hash=duplication_request.title_hash,
            description_hash=duplication_request.description_hash,
            thumbnail_hash=duplication_request.thumbnail_hash,
            material_hashes=duplication_request.material_hashes,
            material_embeddings=duplication_request.material_embeddings,
        )
        
        # If Stage 1 flags, return immediately
        if stage1_response.status.value == "FLAGGED":
            logger.warning(f"Course {course_id} FLAGGED in Stage 1")
            return stage1_response
        
        # Stage 2
        stage2_response = await tox_service.orchestrate_stage2(
            course_id=toxicity_request.course_id,
            title=toxicity_request.title,
            description=toxicity_request.description,
            lesson_texts=toxicity_request.lesson_texts,
            material_list=toxicity_request.material_list,
        )
        
        # Combine logs from both stages
    
        combined_logs =  stage1_response.stage_logs + stage2_response.stage_logs
        
        
        # Determine final status and confidence
        final_status = stage2_response.status
        if stage1_response.status.value == "FLAGGED" or stage2_response.status.value == "FLAGGED":
            final_status = "FLAGGED"
        
        # Final confidence: use flagging step confidence if flagged, else average all steps
        if final_status == "FLAGGED":
            # Find confidence of flagging step
            final_confidence = next(
                (log.confidence_score for log in combined_logs if log.result == "FLAGGED"),
                max([log.confidence_score for log in combined_logs]) if combined_logs else 0.9
            )
        else:
            confidences = [log.confidence_score for log in combined_logs if log.result not in ["ERROR", "PENDING_MODEL"]]
            final_confidence = sum(confidences) / len(confidences) if confidences else 1.0
        
        combined_flagged_content = stage1_response.flagged_content + stage2_response.flagged_content
        
        return ModerationResponse(
            course_id=course_id,
            status=final_status,
            stage_logs=combined_logs,
            flagged_content=combined_flagged_content,
            confidence_score=final_confidence,
            total_latency_ms=stage1_response.total_latency_ms + stage2_response.total_latency_ms,
        )
    
    except ModerationException as e:
        logger.error(f"Moderation error in pipeline: {e.message}")
        raise HTTPException(
            status_code=422,
            detail={
                "error": "moderation_error",
                "message": e.message,
                "code": e.code,
            }
        )
    except Exception as e:
        logger.error(f"Unexpected error in pipeline: {e}")
        raise HTTPException(
            status_code=500,
            detail={
                "error": "internal_error",
                "message": str(e),
            }
        )
