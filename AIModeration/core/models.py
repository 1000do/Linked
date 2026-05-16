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
    flaggedFields: List[str] = Field(default_factory=list)
    details: Dict[str, Any] = Field(default_factory=dict)
    latency_ms: float = Field(0.0)
    confidence_score: float = Field(..., ge=0.0, le=1.0, description="Confidence in this decision")


# ============================================================================
# STAGE 1: DUPLICATION REQUESTS/RESPONSES
# ============================================================================


class SemanticDuplicationRequest(BaseModel):
    """Request for Stage 1: Semantic Duplication checking."""
    course_id: int
    material_ids: List[int]
    similarity_score_threshold: float


class DuplicationCheckResult(BaseModel):
    """Result of a single duplication check."""
    is_duplicate: bool
    matched_ids: List[int] = Field(default_factory=list)
    similarity_scores: Optional[List[float]] = Field(None)
    stageLogs: List[StageLog] = Field(default_factory=list)


# ============================================================================
# STAGE 2: TOXICITY REQUESTS/RESPONSES
# ============================================================================

class CourseHarmfulRequest(BaseModel):
    """Request for Stage 2: Toxicity & Spam checking."""
    course_id: int
    spam_score_threshold: float
    toxic_score_threshold: float


class ToxicityCheckResult(BaseModel):
    """Result of toxicity check."""
    is_flagged: bool
    flaggedFields: List[str] = Field(default_factory=list)
    stageLogs: List[StageLog] = Field(default_factory=list)


# ============================================================================
# EMBEDDING GENERATION
# ============================================================================

class EmbeddingGenerationRequest(BaseModel):
    """Request for generating embeddings."""
    material_id: int = Field(..., description="Material identifier")
    material_type: str = Field(..., description="Type: text, image, video, pdf, word")
    file_path: str = Field(..., description="URL or local path to file")


class EmbeddingGenerationResponse(BaseModel):
    """Response with generated embedding."""
    material_id: int
    embedding: List[float] = Field(..., description="768-dim embedding vector")
    timestamp: datetime = Field(default_factory=datetime.utcnow)
    success: bool = True


# ============================================================================
# UNIFIED MODERATION RESPONSE
# ============================================================================

class CourseModerationResponse(BaseModel):
    """Unified response from moderation pipeline."""
    CourseId: int
    ModerationStatus: str
    flaggedFields: List[str] = Field(default_factory=list)
    overall_confidence_score: float
    total_latency_ms: float
    stageLogs: List[StageLog] = Field(default_factory=list)

    class Config:
        populate_by_name = True


# ============================================================================
# HEALTH CHECK
# ============================================================================

class HealthResponse(BaseModel):
    """Health check response."""
    status: str = Field(..., description="healthy, degraded, unhealthy")
    version: str
    timestamp: datetime = Field(default_factory=datetime.utcnow)
    details: Optional[Dict[str, str]] = Field(None)
