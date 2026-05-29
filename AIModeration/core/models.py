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
    flagged_fields: List[str] = Field(default_factory=list, validation_alias="flagged_content", serialization_alias="flagged_fields")
    details: Dict[str, Any] = Field(default_factory=dict)
    latency_ms: float = Field(0.0)
    confidence_score: float = Field(..., ge=0.0, le=1.0, description="Confidence in this decision")
    model_id: int = Field(0, description="ID of the model used for this stage")


# ============================================================================
# STAGE 1: DUPLICATION REQUESTS/RESPONSES
# ============================================================================

class AiModelDto(BaseModel):
    """DTO for AI model metadata."""
    model_id: int = Field(..., alias="modelId")
    model_name: str = Field(..., alias="modelName")
    model_type: Optional[str] = Field(None, alias="modelType")
    model_provider: Optional[str] = Field(None, alias="modelProvider")
    model_version: Optional[str] = Field(None, alias="modelVersion")
    model_status: Optional[str] = Field(None, alias="modelStatus")
    description: Optional[str] = None
    model_created_at: Optional[datetime] = Field(None, alias="modelCreatedAt")
    model_updated_at: Optional[datetime] = Field(None, alias="modelUpdatedAt")
    model_path: Optional[str] = Field(None, alias="modelPath")
    process_type: Optional[str] = Field(None, alias="processType")

    class Config:
        populate_by_name = True


class SemanticDuplicationRequest(BaseModel):
    """Request for Stage 1: Semantic Duplication checking."""
    course_id: int = Field(..., alias="courseId")
    material_ids: List[int] = Field(..., alias="materialIds")
    similarity_score_threshold: float = Field(..., alias="similarityScoreThreshold")
    models: List[AiModelDto] = Field(default_factory=list, alias="models")

    class Config:
        populate_by_name = True


# ============================================================================
# STAGE 2: TOXICITY REQUESTS/RESPONSES
# ============================================================================

class HarmfulCourseRequest(BaseModel):
    """Request for Stage 2: Toxicity & Spam checking."""
    course_id: int = Field(..., alias="courseId")
    spam_score_threshold: float = Field(..., alias="spamScoreThreshold")
    toxic_score_threshold: float = Field(..., alias="toxicScoreThreshold")
    models: List[AiModelDto] = Field(default_factory=list, alias="models")

    class Config:
        populate_by_name = True


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
# NEW COMPREHENSIVE DTOs MATCHING C# MODELS
# ============================================================================

class MaterialEmbeddingResponse(BaseModel):
    """DTO representing MaterialEmbeddingResponse from C#."""
    embedding_id: int = Field(..., alias="embeddingId")
    material_id: Optional[int] = Field(None, alias="materialId")
    embedding: Optional[List[float]] = Field(None, alias="embedding")

    class Config:
        populate_by_name = True


class EmbeddingGenerationCommand(BaseModel):
    """Embedding generation command for FastAPI internally."""
    material_id: int
    content: bytes
    model_id: int
    material_type: str


class EmbeddingGenerationResult(BaseModel):
    """Embedding generation result for FastAPI internally."""
    material_id: int
    model_id: int
    embedding: List[float]


class MaterialMetadataDto(BaseModel):
    """Mirrors CourseMarketplaceBE.Domain.Entities.MaterialMetadata."""
    metadata_id: int = Field(..., alias="metadataId")
    material_id: int = Field(..., alias="materialId")
    file_name: Optional[str] = Field(None, alias="fileName")
    file_size: Optional[int] = Field(None, alias="fileSize")
    file_extension: Optional[str] = Field(None, alias="fileExtension")
    duration_seconds: Optional[int] = Field(None, alias="durationSeconds")
    word_count: Optional[int] = Field(None, alias="wordCount")
    page_count: Optional[int] = Field(None, alias="pageCount")

    class Config:
        populate_by_name = True


class MaterialDto(BaseModel):
    """Mirrors CourseMarketplaceBE.Application.DTOs.MaterialResponse."""
    material_id: int = Field(..., alias="materialId")
    lesson_id: int = Field(..., alias="lessonId")
    material_title: str = Field(..., alias="materialTitle")
    material_type: str = Field(..., alias="materialType")
    material_url: str = Field(..., alias="materialUrl")
    material_description: Optional[str] = Field(None, alias="materialDescription")
    material_metadata: Optional[MaterialMetadataDto] = Field(None, alias="materialMetadata")

    class Config:
        populate_by_name = True


class LessonDto(BaseModel):
    """Mirrors CourseMarketplaceBE.Application.DTOs.LessonResponse."""
    lesson_id: int = Field(..., alias="lessonId")
    course_id: int = Field(..., alias="courseId")
    title: str = Field(..., alias="title")
    materials: List[MaterialDto] = Field(default_factory=list, alias="materials")

    class Config:
        populate_by_name = True


class CourseDetailDto(BaseModel):
    """Mirrors CourseMarketplaceBE.Application.DTOs.CourseDetailResponse."""
    course_id: int = Field(..., alias="courseId")
    title: str = Field(..., alias="title")
    description: Optional[str] = Field(None, alias="description")
    what_you_will_learn: Optional[str] = Field(None, alias="whatYouWillLearn")
    requirements: Optional[str] = Field(None, alias="requirements")
    thumbnail_url: Optional[str] = Field(None, alias="thumbnailUrl")
    lessons: List[LessonDto] = Field(default_factory=list, alias="lessons")

    class Config:
        populate_by_name = True


# ============================================================================
# UNIFIED MODERATION RESPONSE
# ============================================================================

class CourseModerationResponse(BaseModel):
    """Unified response from moderation pipeline."""
    course_id: int = Field(..., alias="courseId")
    moderation_status: str = Field(..., alias="moderationStatus")
    flagged_fields: List[str] = Field(default_factory=list, alias="flaggedFields")
    overall_confidence_score: float = Field(..., alias="overallConfidenceScore")
    total_latency_ms: float = Field(..., alias="totalLatencyMs")
    stage_logs: List[StageLog] = Field(default_factory=list, alias="stageLogs")

    class Config:
        populate_by_name = True


class FullModerationPipelineRequest(BaseModel):
    """Unified request for full AI moderation pipeline."""
    semantic: SemanticDuplicationRequest
    harmful: HarmfulCourseRequest


# ============================================================================
# HEALTH CHECK
# ============================================================================

class HealthResponse(BaseModel):
    """Health check response."""
    status: str = Field(..., description="healthy, degraded, unhealthy")
    version: str
    timestamp: datetime = Field(default_factory=datetime.utcnow)
    details: Optional[Dict[str, str]] = Field(None)
