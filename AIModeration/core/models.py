from pydantic import BaseModel, Field, model_validator
from typing import Optional, List, Dict, Any
from enum import Enum
from datetime import datetime

class ModerationStatus(str, Enum):
    """Moderation decision status."""
    APPROVED = "APPROVED"
    FLAGGED = "FLAGGED"
    MANUAL_AUDIT = "MANUAL_AUDIT"
    PENDING = "PENDING"
    SKIPPED = "SKIPPED"
    
class DuplicationStatus(str,Enum):
    MATCH_FOUND = "MATCH_FOUND"
    NO_MATCH = "NO_MATCH"

class HarmfulClassificationLabel(str,Enum):
    SAFE = "SAFE"
    TOXIC = "TOXIC"
    SPAM = "SPAM"


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
    manual_audit_fields: List[str] = Field(default_factory=list, validation_alias="manual_audit_content", serialization_alias="manual_audit_fields")
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

    @model_validator(mode="before")
    @classmethod
    def _normalize_keys(cls, data: Any) -> Any:
        if isinstance(data, dict):
            mapping = {
                "modelid": "model_id",
                "modelname": "model_name",
                "modeltype": "model_type",
                "modelprovider": "model_provider",
                "modelversion": "model_version",
                "modelstatus": "model_status",
                "description": "description",
                "modelcreatedat": "model_created_at",
                "modelupdatedat": "model_updated_at",
                "modelpath": "model_path",
                "processtype": "process_type"
            }
            new_data = {}
            for k, v in data.items():
                k_lower = k.lower()
                mapped_key = mapping.get(k_lower, k)
                new_data[mapped_key] = v
            return new_data
        return data

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
    embedding_id: int = Field(..., validation_alias="embeddingId", serialization_alias = "embedding_id")
    material_id: Optional[int] = Field(None, validation_alias="materialId", serialization_alias = "material_id")
    embedding: Optional[List[float]] = Field(None, validation_alias="embedding", serialization_alias = "embedding")
    embedding_type: Optional[str] = Field(None, validation_alias="embeddingType",serialization_alias = "embedding_type")

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
    metadata_id: Optional[int] = Field(0, alias="metadataId")
    material_id: Optional[int] = Field(0, alias="materialId")
    file_name: Optional[str] = Field(None, alias="fileName")
    file_size: Optional[int] = Field(None, alias="fileSize")
    file_extension: Optional[str] = Field(None, alias="fileExtension")
    duration_seconds: Optional[int] = Field(None, alias="durationSeconds")
    word_count: Optional[int] = Field(None, alias="wordCount")
    page_count: Optional[int] = Field(None, alias="pageCount")

    @model_validator(mode="before")
    @classmethod
    def _normalize_keys(cls, data: Any) -> Any:
        if isinstance(data, dict):
            mapping = {
                "metadataid": "metadata_id",
                "materialid": "material_id",
                "filename": "file_name",
                "filesize": "file_size",
                "fileextension": "file_extension",
                "durationseconds": "duration_seconds",
                "duration": "duration_seconds",
                "wordcount": "word_count",
                "pagecount": "page_count"
            }
            new_data = {}
            for k, v in data.items():
                k_lower = k.lower()
                mapped_key = mapping.get(k_lower, k)
                new_data[mapped_key] = v
            return new_data
        return data

    class Config:
        populate_by_name = True


class MaterialDto(BaseModel):
    """Mirrors CourseMarketplaceBE.Application.DTOs.MaterialResponse."""
    material_id: int = Field(..., alias="materialId")
    lesson_id: int = Field(..., alias="lessonId")
    material_title: str = Field(..., alias="materialTitle")
    material_type: Optional[str] = Field("text", alias="materialType")
    material_url: Optional[str] = Field(None, alias="materialUrl")
    material_description: Optional[str] = Field(None, alias="materialDescription")
    material_metadata: Optional[MaterialMetadataDto] = Field(None, alias="materialMetadata")

    @model_validator(mode="before")
    @classmethod
    def _normalize_keys(cls, data: Any) -> Any:
        if isinstance(data, dict):
            mapping = {
                "materialid": "material_id",
                "lessonid": "lesson_id",
                "materialtitle": "material_title",
                "title": "material_title",
                "materialtype": "material_type",
                "type": "material_type",
                "materialurl": "material_url",
                "url": "material_url",
                "materialdescription": "material_description",
                "description": "material_description",
                "materialmetadata": "material_metadata",
                "metadata": "material_metadata"
            }
            new_data = {}
            for k, v in data.items():
                k_lower = k.lower()
                mapped_key = mapping.get(k_lower, k)
                new_data[mapped_key] = v
            
            # Infer material_type from metadata if missing
            if not new_data.get("material_type"):
                metadata = new_data.get("material_metadata")
                if isinstance(metadata, dict):
                    ft = metadata.get("file_type") or metadata.get("fileType") or metadata.get("file_extension") or metadata.get("fileExtension")
                    if ft:
                        new_data["material_type"] = str(ft)
                
                # If still not found, try to infer from material_url
                if not new_data.get("material_type") and new_data.get("material_url"):
                    url = new_data["material_url"]
                    if url:
                        ext = url.split(".")[-1].lower()
                        if ext in ("mp4", "mov", "avi", "mkv", "webm"):
                            new_data["material_type"] = "video"
                        elif ext in ("jpg", "jpeg", "png", "webp", "gif"):
                            new_data["material_type"] = "image"
                        elif ext in ("pdf",):
                            new_data["material_type"] = "pdf"
                        elif ext in ("doc", "docx"):
                            new_data["material_type"] = "word"
                        else:
                            new_data["material_type"] = "text"
            
            # Fallback if still empty
            if not new_data.get("material_type"):
                new_data["material_type"] = "text"
                
            return new_data
        return data

    class Config:
        populate_by_name = True


class LessonDto(BaseModel):
    """Mirrors CourseMarketplaceBE.Application.DTOs.LessonResponse."""
    lesson_id: int = Field(..., alias="lessonId")
    course_id: int = Field(..., alias="courseId")
    title: str = Field(..., alias="title")
    materials: List[MaterialDto] = Field(default_factory=list, alias="materials")

    @model_validator(mode="before")
    @classmethod
    def _normalize_keys(cls, data: Any) -> Any:
        if isinstance(data, dict):
            mapping = {
                "lessonid": "lesson_id",
                "courseid": "course_id",
                "title": "title",
                "materials": "materials",
                "learningmaterials": "materials"
            }
            new_data = {}
            for k, v in data.items():
                k_lower = k.lower()
                mapped_key = mapping.get(k_lower, k)
                new_data[mapped_key] = v
            return new_data
        return data

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

    @model_validator(mode="before")
    @classmethod
    def _normalize_keys(cls, data: Any) -> Any:
        if isinstance(data, dict):
            mapping = {
                "courseid": "course_id",
                "title": "title",
                "description": "description",
                "whatyouwilllearn": "what_you_will_learn",
                "requirements": "requirements",
                "thumbnailurl": "thumbnail_url",
                "coursethumbnailurl": "thumbnail_url",
                "lessons": "lessons"
            }
            new_data = {}
            for k, v in data.items():
                k_lower = k.lower()
                mapped_key = mapping.get(k_lower, k)
                new_data[mapped_key] = v
            return new_data
        return data

    class Config:
        populate_by_name = True


# ============================================================================
# UNIFIED MODERATION RESPONSE
# ============================================================================

class CourseModerationResponse(BaseModel):
    """Unified response from moderation pipeline."""
    course_id: int = Field(..., validation_alias="courseId", serialization_alias="course_id")
    moderation_status: str = Field(..., validation_alias="moderationStatus", serialization_alias="moderation_status")
    flagged_fields: List[str] = Field(default_factory=list, validation_alias="flaggedFields", serialization_alias="flagged_fields")
    manual_audit_fields: List[str] = Field(default_factory=list, validation_alias="manualAuditFields", serialization_alias="manual_audit_fields")
    overall_confidence_score: float = Field(..., validation_alias="overallConfidenceScore", serialization_alias="overall_confidence_score")
    total_latency_ms: float = Field(..., validation_alias="totalLatencyMs", serialization_alias="total_latency_ms")
    stage_logs: List[StageLog] = Field(default_factory=list, validation_alias="stageLogs", serialization_alias="stage_logs")

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


# ============================================================================
# REVIEW MODERATION REQUESTS/RESPONSES
# ============================================================================

class ReviewAiModerationRequest(BaseModel):
    review_comment: str = Field(..., alias="reviewComment")
    spam_score_threshold: float = Field(..., alias="spamScoreThreshold")
    toxic_score_threshold: float = Field(..., alias="toxicScoreThreshold")
    classification_model: AiModelDto = Field(..., alias="classificationModel")
    
    class Config:
        populate_by_name = True


class ReviewAiModerationResponse(BaseModel):
    moderation_status: str = Field(..., validation_alias="moderationStatus", serialization_alias="moderation_status")
    confidence_score: float = Field(..., validation_alias="confidenceScore", serialization_alias="confidence_score")
    reason: str = Field(...)
    latency_ms: float = Field(..., validation_alias="latencyMs", serialization_alias="latency_ms")
    timestamp: datetime = Field(default_factory=datetime.utcnow)
    details: Optional[Dict[str, Any]] = Field(default_factory=dict)
    model_id: int = Field(..., validation_alias="modelId", serialization_alias="model_id")
    
    class Config:
        populate_by_name = True
