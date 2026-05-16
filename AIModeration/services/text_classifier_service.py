"""Service for text toxicity and spam classification using pre-trained models."""

import logging
import sys,os
import time
import torch
from typing import Tuple, Dict, Any
from transformers import AutoTokenizer, AutoModelForSequenceClassification
import torch.nn.functional as F
import numpy as np

from config.settings import Settings, get_settings
from core.exceptions import ModelInferenceException, TimeoutException
from services.base_service import BaseService

logger = logging.getLogger(__name__)


class TextClassifierService(BaseService):
    """Classify text for toxicity and spam using fine-tuned DistilBert ensemble."""
    
    # Class-level model cache (singleton pattern)
    _spam_model = None
    _spam_tokenizer = None
    _toxic_model = None
    _toxic_tokenizer = None
    _models_loaded = False
    
    def __init__(self, settings: Settings = None):
        """Initialize text classifier service."""
        super().__init__("TextClassifierService")
        self.settings = settings or get_settings()
        self.device = torch.device(self.settings.DEVICE)
        
        if not TextClassifierService._models_loaded:
            self._load_models()
    
    @classmethod
    def _load_models(cls):
        """Load models once (class-level cache)."""
        if cls._models_loaded:
            return
        
        logger.info("Loading text classification models...")
        
        try:
            settings = get_settings()
            device = torch.device(settings.DEVICE)
            
            # Load spam model
            spam_path = os.path.abspath(settings.SPAM_MODEL_PATH)
            
            logger.info(f"Loading spam model from {spam_path}...")
            
            cls._spam_tokenizer = AutoTokenizer.from_pretrained(spam_path, local_files_only=True)
            cls._spam_model = AutoModelForSequenceClassification.from_pretrained(spam_path, local_files_only=True)
            cls._spam_model.to(device)
            cls._spam_model.eval()
            logger.info("✓ Spam model loaded")
            
            # Load toxicity model
            toxic_path = os.path.abspath(settings.TOXIC_MODEL_PATH)
            logger.info(f"Loading toxicity model from {toxic_path}...")
            
            cls._toxic_tokenizer = AutoTokenizer.from_pretrained(toxic_path, local_files_only=True)
            cls._toxic_model = AutoModelForSequenceClassification.from_pretrained(toxic_path, local_files_only=True)
            cls._toxic_model.to(device)
            cls._toxic_model.eval()
            logger.info("✓ Toxicity model loaded")
            
            cls._models_loaded = True
            logger.info("✓ All text classification models loaded successfully")
        
        except Exception as e:
            logger.error(f"Failed to load text classification models: {e}")
            raise ModelInferenceException(
                "text_classifier",
                f"Model loading failed: {e}"
            )
    
    async def classify_text(self, text: str, timeout_ms: int = None) -> Tuple[str, float, Dict[str, Any]]:
        """
        Classify text for toxicity and spam.
        
        Uses ensemble of spam and toxicity models with aggregation logic:
        - High confidence threats → FLAGGED
        - Low confidence threat → MANUAL_AUDIT
        - Low confidence safe (confused) → MANUAL_AUDIT
        - All safe & confident → APPROVED
        
        Args:
            text: Text to classify
            timeout_ms: Timeout in milliseconds (optional)
            
        Returns:
            (classification_result, confidence_score, details_dict)
            - classification_result: "APPROVED", "FLAGGED", or "MANUAL_AUDIT"
            - confidence_score: float 0-1
            - details_dict: {spam_score, toxic_score, spam_label, toxic_label, etc.}
        """
        if not text or not text.strip():
            raise ValueError("Empty text provided")
        
        timeout_seconds = (timeout_ms / 1000) if timeout_ms else self.settings.INFERENCE_TIMEOUT
        
        try:
            with self.time_operation("classify_text", text_length=len(text)):
                start_time = time.time()
                
                # Handle very long texts with sliding window
                if len(text) > 512:
                    text = self._sliding_window_aggregate(text)
                
                # Tokenize
                spam_inputs = self._spam_tokenizer(
                    text,
                    return_tensors="pt",
                    truncation=True,
                    max_length=128,
                    padding="max_length",
                )
                spam_inputs = {k: v.to(self.device) for k, v in spam_inputs.items()}
                
                toxic_inputs = self._toxic_tokenizer(
                    text,
                    return_tensors="pt",
                    truncation=True,
                    max_length=128,
                    padding="max_length",
                )
                toxic_inputs = {k: v.to(self.device) for k, v in toxic_inputs.items()}
                
                # Get predictions (parallel inference)
                with torch.no_grad():
                    spam_outputs = self._spam_model(**spam_inputs)
                    toxic_outputs = self._toxic_model(**toxic_inputs)
                
                # Convert to probabilities
                spam_probs = F.softmax(spam_outputs.logits, dim=-1).cpu().numpy()[0]
                toxic_probs = F.softmax(toxic_outputs.logits, dim=-1).cpu().numpy()[0]
                
                # Get predictions
                spam_pred = np.argmax(spam_probs)
                toxic_pred = np.argmax(toxic_probs)
                
                spam_label = self._spam_model.config.id2label.get(int(spam_pred), "UNKNOWN")
                toxic_label = self._toxic_model.config.id2label.get(int(toxic_pred), "UNKNOWN")
                
                spam_conf = float(spam_probs[spam_pred])
                toxic_conf = float(toxic_probs[toxic_pred])
                
                # Aggregate results
                result, conf = self._aggregate_predictions(
                    spam_label=spam_label,
                    spam_conf=spam_conf,
                    toxic_label=toxic_label,
                    toxic_conf=toxic_conf,
                )
                
                elapsed_ms = (time.time() - start_time) * 1000
                
                if elapsed_ms > timeout_seconds * 1000:
                    raise TimeoutException("text_classification", int(timeout_seconds * 1000))
                
                details = {
                    "spam_label": spam_label,
                    "spam_confidence": spam_conf,
                    "toxic_label": toxic_label,
                    "toxic_confidence": toxic_conf,
                    "aggregated_result": result,
                    "final_confidence": conf,
                    "latency_ms": elapsed_ms,
                }
                
                self.logger.debug(f"Classification result: {result} (conf: {conf:.2f})")
                
                return result, conf, details
        
        except TimeoutException:
            raise
        except Exception as e:
            logger.error(f"Text classification failed: {e}")
            raise ModelInferenceException("text_classifier", str(e))
    
    def _sliding_window_aggregate(self, text: str, window_size: int = 128, stride: int = 64) -> str:
        """
        For very long texts, use sliding window approach.
        Extract key sentences and aggregate.
        
        Args:
            text: Long text
            window_size: Characters per window
            stride: Characters to stride
            
        Returns:
            Aggregated representative text
        """
        # Simple approach: take first and last segments
        if len(text) <= window_size:
            return text
        
        first_window = text[:window_size]
        last_window = text[-window_size:]
        
        # Return concatenation (with limit to avoid overflow)
        combined = f"{first_window} ... {last_window}"
        return combined[:512]  # Keep within token limit
    
    def _aggregate_predictions(
        self,
        spam_label: str,
        spam_conf: float,
        toxic_label: str,
        toxic_conf: float,
        high_conf_threshold: float = 0.9,
        low_conf_threshold: float = 0.5,
    ) -> Tuple[str, float]:
        """
        Aggregate spam and toxicity predictions.
        
        4-tier decision system:
        1. High-confidence threats → FLAGGED
        2. Low-confidence threats → MANUAL_AUDIT
        3. Low-confidence safes (confused) → MANUAL_AUDIT
        4. High-confidence safes → APPROVED
        
        Args:
            spam_label: Spam classification label
            spam_conf: Spam confidence
            toxic_label: Toxicity classification label
            toxic_conf: Toxicity confidence
            high_conf_threshold: High confidence threshold (default 0.9)
            low_conf_threshold: Low confidence threshold (default 0.5)
            
        Returns:
            (aggregated_result, confidence)
        """
        # Determine if spam/toxic (check labels)
        is_spam_threat = "positive" in spam_label.lower() or "toxic" in spam_label.lower() or "spam" in spam_label.lower()
        is_toxic_threat = "positive" in toxic_label.lower() or "toxic" in toxic_label.lower()
        
        # Tier 1: High-confidence threats
        if (is_spam_threat and spam_conf >= high_conf_threshold) or \
           (is_toxic_threat and toxic_conf >= high_conf_threshold):
            combined_conf = max(spam_conf, toxic_conf)
            return "FLAGGED", combined_conf
        
        # Tier 2: Low-confidence threats
        if (is_spam_threat and spam_conf >= low_conf_threshold) or \
           (is_toxic_threat and toxic_conf >= low_conf_threshold):
            combined_conf = max(spam_conf, toxic_conf)
            return "MANUAL_AUDIT", combined_conf
        
        # Tier 3: Low-confidence safes (model confused)
        if (not is_spam_threat and spam_conf < low_conf_threshold) or \
           (not is_toxic_threat and toxic_conf < low_conf_threshold):
            combined_conf = min(spam_conf, toxic_conf)
            return "MANUAL_AUDIT", combined_conf
        
        # Tier 4: High-confidence safes
        combined_conf = min(spam_conf, toxic_conf)
        return "APPROVED", combined_conf
