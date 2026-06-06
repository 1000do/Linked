"""Base service class with common utilities."""

import logging
import time
from typing import Dict, Any
from datetime import datetime

logger = logging.getLogger(__name__)


class BaseService:
    """Base service with common logging and timing utilities."""
    
    def __init__(self, service_name: str):
        """
        Initialize base service.
        
        Args:
            service_name: Name of the service for logging
        """
        self.service_name = service_name
        self.logger = logging.getLogger(f"{__name__}.{service_name}")
    
    def log_stage_entry(self, stage: int, course_id: int, **kwargs):
        """Log entry to a stage."""
        details = ", ".join(f"{k}={v}" for k, v in kwargs.items())
        self.logger.info(f"[Stage {stage}] Processing course {course_id}: {details}")
    
    def log_step_entry(self, stage: int, step: int, **kwargs):
        """Log entry to a step within a stage."""
        details = ", ".join(f"{k}={v}" for k, v in kwargs.items())
        self.logger.debug(f"[Stage {stage}.{step}] {details}")
    
    def log_result(self, stage: int, step: int, result: str, reason: str, **kwargs):
        """Log step result."""
        details = ", ".join(f"{k}={v}" for k, v in kwargs.items())
        self.logger.info(f"[Stage {stage}.{step}] Result: {result} - {reason} ({details})")
    
    def log_error(self, stage: int, step: int, error: str, **kwargs):
        """Log error in a step."""
        details = ", ".join(f"{k}={v}" for k, v in kwargs.items())
        self.logger.error(f"[Stage {stage}.{step}] Error: {error} ({details})")
    
    def time_operation(self, operation_name: str, **kwargs):
        """Context manager for timing operations."""
        class Timer:
            def __init__(self, service, name, details):
                self.service = service
                self.name = name
                self.details = details
                self.start_time = None
                self.elapsed_ms = None
            
            def __enter__(self):
                self.start_time = time.time()
                return self
            
            def __exit__(self, exc_type, exc_val, exc_tb):
                self.elapsed_ms = (time.time() - self.start_time) * 1000
                if exc_type:
                    self.service.logger.error(
                        f"Operation '{self.name}' failed after {self.elapsed_ms:.2f}ms: {exc_val}"
                    )
                else:
                    self.service.logger.debug(
                        f"Operation '{self.name}' completed in {self.elapsed_ms:.2f}ms"
                    )
        
        return Timer(self, operation_name, kwargs)
    
    def build_stage_log(
        self,
        stage: int,
        step: int,
        result: str,
        reason: str,
        confidence_score: float,
        flagged_content: list = None,
        details: dict = None,
        latency_ms: float = 0.0,
        model_id: int = 0
    ) -> Dict[str, Any]:
        """
        Build a stage log entry.
        
        Args:
            stage: Stage number
            step: Step within stage
            result: Result status
            reason: Explanation of result
            confidence_score: Confidence (0-1)
            flagged_content: List of flagged content if any
            details: Additional details dict
            latency_ms: Execution time in milliseconds
            
        Returns:
            Stage log dict
        """
        return {
            "stage": stage,
            "step": step,
            "timestamp": datetime.utcnow().isoformat(),
            "result": result,
            "reason": reason,
            "flagged_content": flagged_content or [],
            "details": details or {},
            "confidence_score": float(confidence_score),
            "latency_ms": float(latency_ms),
            "model_id": model_id
        }
