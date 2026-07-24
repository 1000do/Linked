import logging
import time
import numpy as np
from typing import Dict, List, Optional, Any, Tuple
from core.models import StageLog, CourseModerationResponse, ModerationStatus, HarmfulClassificationLabel
from core.exceptions import ModerationException
from services.base_service import BaseService
from services.text_classifier_service import TextClassifierService

logger = logging.getLogger(__name__)

class HarmfulReviewService(BaseService):
    """Handle toxicity & spam detection for review comments."""
    
    def __init__(
        self,
        text_classifier: TextClassifierService = None,
    ):
        super().__init__("HarmfulReviewService")
        self.text_classifier = text_classifier or TextClassifierService()

    async def check_review_harmful(
        self,
        review_comment: str,
        model_id: int,
        spam_threshold: float,
        toxic_threshold: float
    ) -> Tuple[str, float, str, Dict[str, Any]]:
        """
        Evaluate a single review comment.
        Returns:
            action (str)
            flagged_reason (Optional[str])
            details (Dict[str, Any])
            confidence_score (float)
        """
        if not review_comment or not str(review_comment).strip():
            return ModerationStatus.APPROVED.value, 1.0, "Empty review comment", {"status": ModerationStatus.SKIPPED.value, "reason": "Empty review comment."}

        try:
            details_map = {}

            action, score, details = self.text_classifier.classify_text(
                review_comment,
                spam_threshold=spam_threshold,
                toxic_threshold=toxic_threshold
            )
            
            details_map["review.comment"] = details
            raw_label = details.get("raw_label", HarmfulClassificationLabel.SAFE.value)
            reason = details.get("reason","Inference complete")
            if action == ModerationStatus.FLAGGED.value:
                display_reason = f"Harmful review comment detected. Reason: {reason}. Threat: {raw_label}"
            elif action == ModerationStatus.MANUAL_AUDIT.value:
                display_reason = f"Suspicious review comment detected. Reason: {reason}. Threat: {raw_label}"
            else:
                display_reason = "No harmful content detected in review comment."
            
            return action, score, display_reason, details_map

        except Exception as e:
            raise ModerationException(f"Error during review text classification: {e}")
