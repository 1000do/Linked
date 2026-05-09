from pydantic import BaseModel, Field
from typing import Optional, List, Dict, Any
from enum import Enum
from datetime import datetime


class ModerationStatus(str, Enum):
    """Moderation decision status."""
    APPROVED = "APPROVED"
    FLAGGED = "FLAGGED"
    MANUAL_AUDIT = "MANUAL_AUDIT"
    PENDING = "PENDING"


class ModerationStage(int, Enum):
    """Pipeline stages."""
    DUPLICATION = 1
    TOXICITY = 2
    VISION = 3


class StageLog(BaseModel):
    """Log entry for each stage/step in pipeline."""
    stage: int = Field(..., description="Stage number (1, 2, 3)")
    step: int = Field(..., description="Step within stage (1, 2, 3)")
    timestamp: datetime = Field(default_factory=datetime.utcnow)
    result: str = Field(..., description="Result: NO_MATCH, MATCH_FOUND, FLAGGED, APPROVED, etc.")
    reason: str = Field(..., description="Explanation of result")
    flagged_content: Optional[List[str]] = Field(None, description="What triggered the flag")
    details: Optional[Dict[str, Any]] = Field(None, description="Additional details (matches, scores, etc.)")
    confidence_score: float = Field(..., ge=0.0, le=1.0, description="Confidence in this decision")


# ============================================================================
# STAGE 1: DUPLICATION REQUESTS/RESPONSES
# ============================================================================

class DuplicationRequest(BaseModel):
    """Request for Stage 1: Duplication checking."""
    course_id: int = Field(..., description="Course to check")
    title_hash: str = Field(..., description="MD5 hash of course title")
    description_hash: str = Field(..., description="MD5 hash of course description")
    thumbnail_hash: Optional[str] = Field(None, description="MD5 hash of course thumbnail")
    material_hashes: List[str] = Field(default_factory=list, description="MD5 hashes of learning materials")
    material_embeddings: Optional[List[List[float]]] = Field(None, description="768-dim embeddings of materials")


class DuplicationCheckResult(BaseModel):
    """Result of a single duplication check."""
    is_duplicate: bool
    matched_ids: List[int] = Field(default_factory=list)
    similarity_scores: Optional[List[float]] = Field(None)
    stage_logs: List[StageLog] = Field(default_factory=list)


# ============================================================================
# STAGE 2: TOXICITY REQUESTS/RESPONSES
# ============================================================================

class ToxicityRequest(BaseModel):
    """Request for Stage 2: Toxicity & Spam checking."""
    course_id: int = Field(..., description="Course to check")
    title: str = Field(..., description="Course title text")
    description: str = Field(..., description="Course description text")
    lesson_texts: List[str] = Field(default_factory=list, description="Lesson titles and descriptions")
    material_list: Optional[List[Dict[str, Any]]] = Field(
        None,
        description="Materials with paths and types: [{material_id, file_path, file_type}, ...]"
    )


class ToxicityCheckResult(BaseModel):
    """Result of toxicity check."""
    is_flagged: bool
    flagged_fields: List[str] = Field(default_factory=list, description="Which fields triggered flags")
    stage_logs: List[StageLog] = Field(default_factory=list)


# ============================================================================
# EMBEDDING GENERATION
# ============================================================================

class EmbeddingGenerationRequest(BaseModel):
    """Request for generating embeddings."""
    material_id: int = Field(..., description="Material identifier")
    material_type: str = Field(..., description="Type: text, image, video, pdf, word")
    content: bytes = Field(..., description="File content or text as bytes")


class EmbeddingGenerationResponse(BaseModel):
    """Response with generated embedding."""
    material_id: int
    embedding: List[float] = Field(..., description="768-dim embedding vector")
    timestamp: datetime = Field(default_factory=datetime.utcnow)
    success: bool = True


# ============================================================================
# UNIFIED MODERATION RESPONSE
# ============================================================================

class ModerationResponse(BaseModel):
    """Unified response from moderation pipeline."""
    course_id: int = Field(..., description="Course that was moderated")
    status: ModerationStatus = Field(..., description="Final decision: APPROVED, FLAGGED, MANUAL_AUDIT, etc.")
    stage_logs: List[StageLog] = Field(default_factory=list, description="Full log trace from entry point")
    flagged_content: List[str] = Field(default_factory=list, description="What was flagged")
    confidence_score: float = Field(..., ge=0.0, le=1.0, description="Overall confidence in decision")
    total_latency_ms: float = Field(default=0.0, description="Total time in milliseconds")
    
    class Config:
        json_schema_extra = {
            "example": {
                "course_id": 123,
                "status": "FLAGGED",
                "flagged_content": ["course_description"],
                "confidence_score": 0.96,
                "total_latency_ms": 1250.5,
                "stage_logs": [
                    {
                        "stage": 1,
                        "step": 1,
                        "result": "NO_MATCH",
                        "reason": "No exact duplicates found",
                        "confidence_score": 1.0
                    }
                ]
            }
        }


# ============================================================================
# HEALTH CHECK
# ============================================================================

class HealthResponse(BaseModel):
    """Health check response."""
    status: str = Field(..., description="healthy, degraded, unhealthy")
    version: str
    timestamp: datetime = Field(default_factory=datetime.utcnow)
    details: Optional[Dict[str, str]] = Field(None)
