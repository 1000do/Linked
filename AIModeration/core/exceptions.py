"""Custom exceptions for moderation service."""


class ModerationException(Exception):
    """Base exception for all moderation-related errors."""
    
    def __init__(self, message: str, code: str = "MODERATION_ERROR", details: dict = None):
        self.message = message
        self.code = code
        self.details = details or {}
        super().__init__(self.message)


class CacheNotFoundException(ModerationException):
    """Raised when Redis cache lookup fails."""
    
    def __init__(self, key: str, details: dict = None):
        super().__init__(
            message=f"Cache key not found: {key}",
            code="CACHE_NOT_FOUND",
            details=details or {"key": key}
        )


class ModelInferenceException(ModerationException):
    """Raised when model inference fails."""
    
    def __init__(self, model_name: str, reason: str, details: dict = None):
        super().__init__(
            message=f"Model inference failed for {model_name}: {reason}",
            code="MODEL_INFERENCE_ERROR",
            details=details or {"model": model_name, "reason": reason}
        )


class DataValidationException(ModerationException):
    """Raised when input data validation fails."""
    
    def __init__(self, field: str, reason: str, details: dict = None):
        super().__init__(
            message=f"Data validation failed for {field}: {reason}",
            code="DATA_VALIDATION_ERROR",
            details=details or {"field": field, "reason": reason}
        )


class FileProcessingException(ModerationException):
    """Raised when file processing (OCR, Whisper, etc.) fails."""
    
    def __init__(self, file_type: str, reason: str, details: dict = None):
        super().__init__(
            message=f"File processing failed for {file_type}: {reason}",
            code="FILE_PROCESSING_ERROR",
            details=details or {"file_type": file_type, "reason": reason}
        )


class EmbeddingException(ModerationException):
    """Raised when embedding generation fails."""
    
    def __init__(self, material_type: str, reason: str, details: dict = None):
        super().__init__(
            message=f"Embedding generation failed for {material_type}: {reason}",
            code="EMBEDDING_ERROR",
            details=details or {"material_type": material_type, "reason": reason}
        )


class TimeoutException(ModerationException):
    """Raised when operation times out."""
    
    def __init__(self, operation: str, timeout_ms: int, details: dict = None):
        super().__init__(
            message=f"Operation '{operation}' timed out after {timeout_ms}ms",
            code="TIMEOUT_ERROR",
            details=details or {"operation": operation, "timeout_ms": timeout_ms}
        )
