"""API routes for moderation endpoints."""

import logging
from fastapi import APIRouter, HTTPException, Depends, UploadFile, File, Form
from typing import List, Optional
import json

from core.models import (
    SemanticDuplicationRequest, HarmfulCourseRequest, CourseModerationResponse, HealthResponse,
    EmbeddingGenerationRequest, EmbeddingGenerationResponse, FullModerationPipelineRequest,
)
from core.exceptions import ModerationException
from handlers.duplication_handler import DuplicationHandler
from handlers.harmful_handler import HarmfulHandler
from services.embedding_service import EmbeddingService
from repositories.cache_repository import CacheRepository
from config.settings import get_settings

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/moderation", tags=["moderation"])

# ============================================================================
# DEPENDENCY INJECTION / HANDLER CONTEXTS
# ============================================================================

def get_duplication_handler() -> DuplicationHandler:
    return DuplicationHandler()

def get_harmful_handler() -> HarmfulHandler:
    return HarmfulHandler()

def get_embedding_service() -> EmbeddingService:
    return EmbeddingService()

# ============================================================================
# STAGE 1: DUPLICATION DETECTION
# ============================================================================

@router.post("/stage1", response_model=CourseModerationResponse)
async def moderation_stage1(
    request: SemanticDuplicationRequest,
    handler: DuplicationHandler = Depends(get_duplication_handler),
):
    """
    Stage 1: Check for semantic duplication.
    """
    try:
        logger.info(f"Processing Stage 1 via DuplicationHandler for course {request.course_id}")
        return await handler.orchestrate_stage1(request)
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
# STAGE 2: HARMFUL CONTENT DETECTION (TOXICITY & SPAM)
# ============================================================================

@router.post("/stage2", response_model=CourseModerationResponse)
async def moderation_stage2(
    request: HarmfulCourseRequest,
    handler: HarmfulHandler = Depends(get_harmful_handler),
):
    """
    Stage 2: Check for harmful content in text and media.
    """
    try:
        logger.info(f"Processing Stage 2 via HarmfulHandler for course {request.course_id}")
        return await handler.orchestrate_stage2(request)
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
# FULL PIPELINE (Stages 1 + 2)
# ============================================================================

def _get_log_attr(log, attr, default=None):
    """Retrieve attribute from stage log safely regardless of dict or Pydantic model."""
    if isinstance(log, dict):
        return log.get(attr, default)
    return getattr(log, attr, default)

@router.post("/full-pipeline", response_model=CourseModerationResponse)
async def full_moderation_pipeline(
    request: FullModerationPipelineRequest,
    dup_handler: DuplicationHandler = Depends(get_duplication_handler),
    harmful_handler: HarmfulHandler = Depends(get_harmful_handler),
):
    """
    Run full moderation pipeline (Stage 1 + Stage 2).
    """
    try:
        course_id = request.semantic.course_id
        logger.info(f"Processing full pipeline for course {course_id}")
        
        # Stage 1
        stage1_response = await dup_handler.orchestrate_stage1(request.semantic)
        
        # If Stage 1 flags, return immediately
        if stage1_response.moderation_status == "FLAGGED":
            logger.warning(f"Course {course_id} FLAGGED in Stage 1")
            return stage1_response
        
        # Stage 2
        stage2_response = await harmful_handler.orchestrate_stage2(request.harmful)
        
        # Combine logs from both stages
        combined_logs = stage1_response.stage_logs + stage2_response.stage_logs
        
        # Determine final status and confidence
        final_status = stage2_response.moderation_status
        if stage1_response.moderation_status == "FLAGGED" or stage2_response.moderation_status == "FLAGGED":
            final_status = "FLAGGED"
        
        # Final confidence
        if final_status == "FLAGGED":
            confidences = [
                _get_log_attr(log, "confidence_score", 0.9669)
                for log in combined_logs
                if _get_log_attr(log, "result") == 'FLAGGED'
            ] if combined_logs else [0.9669]
            if not confidences:
                confidences = [0.9669]
        else:
            confidences = [
                _get_log_attr(log, "confidence_score", 1.0)
                for log in combined_logs
                if _get_log_attr(log, "result") not in ["ERROR", "PENDING_MODEL"]
            ] if combined_logs else [1.0]
            if not confidences:
                confidences = [1.0]
            
        final_confidence = sum(confidences) / len(confidences)
  
  
        
        
        combined_flagged_content = stage1_response.flagged_fields + stage2_response.flagged_fields
        
        return CourseModerationResponse(
            course_id=course_id,
            moderation_status=final_status,
            stage_logs=combined_logs,
            flagged_fields=combined_flagged_content,
            overall_confidence_score=final_confidence,
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
