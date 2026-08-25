import time
import json
import logging
from typing import Optional, Dict, Any
from core.models import (
    ReviewAiModerationRequest,
    ReviewAiModerationResponse,
    StageLog,
    ModerationStatus
)
from handlers.base_handler import BaseHandler
from services.harmful_review_service import HarmfulReviewService

logger = logging.getLogger(__name__)

class HarmfulReviewHandler(BaseHandler):
    def __init__(
        self,
        harmful_review_service: Optional[HarmfulReviewService] = None
    ):
        super().__init__("HarmfulReviewHandler")
        self.harmful_review_service = harmful_review_service or HarmfulReviewService()

    

    async def orchestrate_review_moderation(self, request: ReviewAiModerationRequest) -> ReviewAiModerationResponse:
        """
        Orchestrate harmful (toxicity & spam) detection for a single review comment.
        """
        stage_start = time.time()
        model = request.classification_model

        # print("Classification model details:")
        # print(json.dumps(vars(model), indent=4, default=str))

        model_status = model.model_status if model else None
        if model and model_status and model_status.lower() == 'inactive':
            self.logger.warning("Classification model is inactive. Early exit with manual audit.")
            total_latency = (time.time() - stage_start) * 1000
            return ReviewAiModerationResponse(
                moderation_status=ModerationStatus.MANUAL_AUDIT.value,
                confidence_score=0.0,
                reason="Classification model is inactive",
                latency_ms=total_latency,
                details = {},
                model_id=request.classification_model.model_id
            )

        # Use provided thresholds
        spam_threshold = request.spam_score_threshold
        toxic_threshold = request.toxic_score_threshold
        
        # Use provided model details
        model_id = request.classification_model.model_id if request.classification_model else 0

        self.logger.info(f"Starting review moderation orchestration with Spam Threshold {spam_threshold} and Toxic Threshold {toxic_threshold}")

        # Check Text Harmful
        action, score, reason, details_map = await self.harmful_review_service.check_review_harmful(
            review_comment=request.review_comment,
            model_id=model_id,
            spam_threshold=spam_threshold,
            toxic_threshold=toxic_threshold
        )
        
        total_latency = (time.time() - stage_start) * 1000

        if action != ModerationStatus.APPROVED.value:
            self.logger.warning(reason)
        else:
            self.logger.info(f"{reason}. Review comment approved by AI moderation.")

        return ReviewAiModerationResponse(
            moderation_status=action,
            confidence_score=score,
            reason=reason,
            latency_ms=total_latency,
            details=details_map,
            model_id=model_id
        )
