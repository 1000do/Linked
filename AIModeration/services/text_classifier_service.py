"""Service for text toxicity and spam classification using pre-trained models."""

import json
import logging
import os
import time
import torch
from typing import Tuple, Dict, List, Any
from transformers import AutoTokenizer, AutoModelForSequenceClassification
import torch.nn.functional as F

from config.settings import Settings, get_settings
from core.exceptions import ModelInferenceException
from services.base_service import BaseService

logger = logging.getLogger(__name__)


class TextClassifierService(BaseService):
    """Classify text for toxicity and spam using fine-tuned DistilBert ensemble."""
    
    # Class-level model cache (singleton pattern)
    _spam_model = None
    _spam_tokenizer = None
    _toxic_model = None
    _toxic_tokenizer = None
    _spam_path = None
    _toxic_path = None
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
            cls._spam_path = spam_path
            logger.info("✓ Spam model loaded")
            
            # Load toxicity model
            toxic_path = os.path.abspath(settings.TOXIC_MODEL_PATH)
            logger.info(f"Loading toxicity model from {toxic_path}...")
            
            cls._toxic_tokenizer = AutoTokenizer.from_pretrained(toxic_path, local_files_only=True)
            cls._toxic_model = AutoModelForSequenceClassification.from_pretrained(toxic_path, local_files_only=True)
            cls._toxic_model.to(device)
            cls._toxic_model.eval()
            cls._toxic_path = toxic_path
            logger.info("✓ Toxicity model loaded")
            
            cls._models_loaded = True
            logger.info("✓ All text classification models loaded successfully")
        
        except Exception as e:
            logger.error(f"Failed to load text classification models: {e}")
            raise ModelInferenceException(
                "text_classifier",
                f"Model loading failed: {e}"
            )

    def robust_sliding_window(self, text: str, window_size: int = 128, stride: int = 64) -> List[Dict[str, Any]]:
        spam_model = self._spam_model
        toxic_model = self._toxic_model
        spam_model.eval()
        toxic_model.eval()
        tokenizer = self._spam_tokenizer
        
        tokens = tokenizer.encode(text, add_special_tokens=False)
        length = len(tokens)
        
        # If text is short, just do a normal pass (Minus 2 special tokens for safety)
        if length <= window_size - 2:
            return [self.check_course_description(text)]

        chunk_results = []

        for i in range(0, length, stride):
            # Leave 2 spaces for [CLS] and [SEP]
            chunk = tokens[i : i + window_size - 2]
            
            # Construct sequence with special tokens
            input_ids = [tokenizer.cls_token_id] + chunk + [tokenizer.sep_token_id]
            attention_mask = [1] * len(input_ids)
            
            # Pad to window_size
            pad_len = window_size - len(input_ids)
            if pad_len > 0:
                input_ids.extend([tokenizer.pad_token_id] * pad_len)
                attention_mask.extend([0] * pad_len)
            
            encoded_chunk = {
                "input_ids": torch.tensor([input_ids], device=self.device),
                "attention_mask": torch.tensor([attention_mask], device=self.device)
            }
            
            with torch.no_grad():
                spam_outputs = spam_model(**encoded_chunk)
                toxic_outputs = toxic_model(**encoded_chunk)
                
                spam_probs = F.softmax(spam_outputs.logits, dim=-1)
                toxic_probs = F.softmax(toxic_outputs.logits, dim=-1)
            
            spam_conf, spam_pred = torch.max(spam_probs, dim=1)
            toxic_conf, toxic_pred = torch.max(toxic_probs, dim=1)
            
            chunk_results.append({
                "text": tokenizer.decode(chunk),
                "spam_score": spam_conf.item(),
                "spam_label": spam_model.config.id2label[spam_pred.item()],
                "toxic_score": toxic_conf.item(),
                "toxic_label": toxic_model.config.id2label[toxic_pred.item()],
            })
            
            if i + window_size >= length:
                break

        return chunk_results

    def check_course_description(self, text: str) -> Dict[str, Any]:
        spam_model = self._spam_model
        toxic_model = self._toxic_model
        tokenizer = self._spam_tokenizer
        spam_model.eval()
        toxic_model.eval()
        
        inputs = tokenizer(
            text,
            return_tensors="pt",
            truncation=True,
            padding='max_length',
            max_length=128
        ).to(self.device)
        
        with torch.no_grad():
            spam_outputs = spam_model(**inputs)
            toxic_outputs = toxic_model(**inputs)
            spam_probs = F.softmax(spam_outputs.logits, dim=-1)
            toxic_probs = F.softmax(toxic_outputs.logits, dim=-1)
        
        spam_conf_score, spam_prediction = torch.max(spam_probs, dim=1)
        spam_conf_score = spam_conf_score.item()
        spam_prediction = spam_prediction.item()    
        
        toxic_conf_score, toxic_prediction = torch.max(toxic_probs, dim=1)
        toxic_conf_score = toxic_conf_score.item()
        toxic_prediction = toxic_prediction.item()
        
        spam_label = spam_model.config.id2label[spam_prediction]
        toxic_label = toxic_model.config.id2label[toxic_prediction]
        
        res = {
            "text": text, 
            "spam_score": spam_conf_score,
            "spam_label": spam_label,
            "toxic_score": toxic_conf_score,
            "toxic_label": toxic_label
        }
        return res

    def aggregation_logic(self, chunk_results: List[Dict[str, Any]], spam_threshold: float = 0.85, toxic_threshold: float = 0.85) -> Dict[str, Any]:
        # 1. Identify High-Confidence Threats (The most dangerous)
        candidates = []
       
        high_conf_spams = [
            r 
            for r in chunk_results
            if (r["spam_label"] != "SAFE" and r['spam_score'] >= spam_threshold)
        ]
        
        high_conf_toxics = [
            r 
            for r in chunk_results
            if (r["toxic_label"] != 'SAFE' and r['toxic_score'] >= toxic_threshold)
        ]
        
        
        if high_conf_spams or high_conf_toxics:
            self.logger.info(f"High confidence threats:\n {json.dumps(high_conf_spams + high_conf_toxics, indent= 4,ensure_ascii=False)}")
            
            for threat in high_conf_spams:
                candidates.append(
                    {
                        'text':threat['text'],
                        'score': threat['spam_score'],
                        'difference': abs(threat['spam_score'] - spam_threshold),
                        'label': threat['spam_label']
                    }
                )
                
            for threat in high_conf_toxics:
                candidates.append(
                    {
                        'text':threat['text'],
                        'score': threat['toxic_score'],
                        'difference': abs(threat['toxic_score'] - toxic_threshold),
                        'label': threat['toxic_label']
                    }
                )
            
            most_severe_threat = max(candidates, key=lambda x : x['difference'])
            
            return {
                'text': most_severe_threat['text'],
                'action': 'FLAGGED',
                'score': most_severe_threat['score'],
                'raw_label': most_severe_threat['label']
            }

        # 2. Identify Low-Confidence Threats (Model is suspicious)
        low_conf_spams = [
            r 
            for r in chunk_results
            if (r["spam_label"] != "SAFE" and r['spam_score'] < spam_threshold)
            
        ]
        low_conf_toxics = [
            r 
            for r in chunk_results
            if (r["toxic_label"] != 'SAFE' and r['toxic_score'] < toxic_threshold)
        ]
        
        if low_conf_spams or low_conf_toxics:
            self.logger.info(f"Low confidence threats:\n {json.dumps(low_conf_toxics + low_conf_spams, indent= 4,ensure_ascii=False)}")
            for threat in low_conf_spams:
                candidates.append(
                    {
                        'text':threat['text'],
                        'score': threat['spam_score'],
                        'difference': abs(threat['spam_score'] - spam_threshold),
                        'label': threat['spam_label']
                    }
                )
                
            for threat in low_conf_toxics:
                candidates.append(
                    {
                        'text':threat['text'],
                        'score': threat['toxic_score'],
                        'difference': abs(threat['toxic_score'] - toxic_threshold),
                        'label': threat['toxic_label']
                    }
                )        
            
            most_suspicious_threat = min(candidates, key=lambda x: x['difference'])
            
            return {
                "text": most_suspicious_threat['text'],
                "action": "MANUAL_AUDIT", 
                "reason": "Probable Threat (Low Confidence)", 
                "score": most_suspicious_threat['score'], 
                "raw_label": most_suspicious_threat['label']
            }

        # 3. Identify Low-Confidence Safes (Model is confused)
        low_conf_non_spams = [
            r 
            for r in chunk_results
            if (r["spam_label"] == "SAFE" and r['spam_score'] < spam_threshold)
        ]
        low_conf_non_toxics = [
            r 
            for r in chunk_results
            if (r["toxic_label"] == 'SAFE' and r['toxic_score'] < toxic_threshold)
        ]
        if low_conf_non_spams or low_conf_non_toxics:
            self.logger.info(f"Low confidence safe:\n {json.dumps(low_conf_non_spams + low_conf_non_toxics, indent= 4,ensure_ascii=False)}")
            
            for threat in low_conf_non_spams:
                candidates.append(
                    {
                        'text':threat['text'],
                        'score': threat['spam_score'],
                        'difference': abs(threat['spam_score'] - spam_threshold),
                        'label': threat['spam_label']
                    }
                )
                
            for threat in low_conf_non_toxics:
                candidates.append(
                    {
                        'text':threat['text'],
                        'score': threat['toxic_score'],
                        'difference': abs(threat['toxic_score'] - toxic_threshold),
                        'label': threat['toxic_label']
                    }
                )        
            
            most_confused_safe = max(candidates,key=lambda x:x['difference'])
                
            return {
                "text": most_confused_safe['text'],
                "action": "MANUAL_AUDIT", 
                "reason": "Ambiguous Content (Low Confidence Safe)", 
                "score": most_confused_safe['score'], 
                "raw_label": most_confused_safe['label']
            }

        # 4. APPROVED (Safest average)
        avg_score = sum(r["spam_score"] + r['toxic_score'] for r in chunk_results) / (2 * len(chunk_results))
        return {"action": "APPROVED", "score": avg_score, "raw_label": "SAFE"}

    def classify_text(self, text: str, spam_threshold: float = 0.85, toxic_threshold: float = 0.85, window_size: int = 128, stride: int = 64) -> Tuple[str, float, Dict[str, Any]]:
        """
        Classify text for toxicity and spam using sliding windows.
        """
        if not text or not text.strip():
            return "APPROVED", 1.0, {"action": "APPROVED", "score": 1.0, "raw_label": "SAFE"}

        start_time = time.time()
        try:
            chunk_results = self.robust_sliding_window(text, window_size=window_size, stride=stride)
            logger.info(f"Aggregating harfum detection result with spam threshold {spam_threshold} and toxic threshold {toxic_threshold}")
            agg = self.aggregation_logic(chunk_results, spam_threshold=spam_threshold, toxic_threshold=toxic_threshold)
            self.logger.info(f"Aggregated results:\n{json.dumps(agg, indent=4,ensure_ascii=False)}")
            elapsed_ms = (time.time() - start_time) * 1000
            
            # Map aggregated action to return tuple format
            action = agg.get("action", "APPROVED")
            score = agg.get("score", 1.0)
            
            details = {
                "text": agg.get("text", text[:60] + "..."),
                "score": score,
                "raw_label": agg.get("raw_label", "SAFE"),
                "latency_ms": elapsed_ms,
                "reason": agg.get("reason", "Inference complete")
            }
            return action, score, details

        except Exception as e:
            logger.error(f"Text classification robust pipeline failed: {e}")
            raise ModelInferenceException("text_classifier", str(e))

    def classify_text_list(self, texts: List[str], spam_threshold: float = 0.85, toxic_threshold: float = 0.85, window_size: int = 128) -> Tuple[str, float, Dict[str, Any]]:
        """
        Classify a list of extracted texts for toxicity and spam using batched inference.
        """
        if not texts:
            return "APPROVED", 1.0, {"action": "APPROVED", "score": 1.0, "raw_label": "SAFE"}

        start_time = time.time()
        try:
            spam_model = self._spam_model
            toxic_model = self._toxic_model
            spam_model.eval()
            toxic_model.eval()
            tokenizer = self._spam_tokenizer
            
            # 1. Tokenize and apply sliding window to all texts
            all_chunks = []
            stride = 64
            
            for text in texts:
                if not text.strip():
                    continue
                    
                tokens = tokenizer.encode(text, add_special_tokens=False)
                length = len(tokens)
                
                # If text is short, add it as a single chunk
                if length <= window_size - 2:
                    all_chunks.append(tokens)
                else:
                    # If text is long, use sliding window to retain context without losing data
                    for i in range(0, length, stride):
                        chunk = tokens[i : i + window_size - 2]
                        all_chunks.append(chunk)
                        if i + window_size - 2 >= length:
                            break
                            
            if not all_chunks:
                return "APPROVED", 1.0, {"action": "APPROVED", "score": 1.0, "raw_label": "SAFE"}

            # 2. Process in batches to maximize CPU/GPU efficiency
            batch_size = 16
            chunk_results = []
            
            for i in range(0, len(all_chunks), batch_size):
                batch_chunks = all_chunks[i : i + batch_size]
                
                input_ids_batch = []
                attention_mask_batch = []
                
                for chunk in batch_chunks:
                    input_ids = [tokenizer.cls_token_id] + chunk + [tokenizer.sep_token_id]
                    pad_len = window_size - len(input_ids)
                    
                    attention_mask = [1] * len(input_ids) + [0] * pad_len
                    if pad_len > 0:
                        input_ids.extend([tokenizer.pad_token_id] * pad_len)
                        
                    input_ids_batch.append(input_ids)
                    attention_mask_batch.append(attention_mask)
                
                encoded_batch = {
                    "input_ids": torch.tensor(input_ids_batch, device=self.device),
                    "attention_mask": torch.tensor(attention_mask_batch, device=self.device)
                }
                
                with torch.no_grad():
                    spam_outputs = spam_model(**encoded_batch)
                    toxic_outputs = toxic_model(**encoded_batch)
                    
                    spam_probs = F.softmax(spam_outputs.logits, dim=-1)
                    toxic_probs = F.softmax(toxic_outputs.logits, dim=-1)
                
                spam_confs, spam_preds = torch.max(spam_probs, dim=1)
                toxic_confs, toxic_preds = torch.max(toxic_probs, dim=1)
                
                # Unpack the batch results
                for j in range(len(batch_chunks)):
                    chunk_results.append({
                        "text": tokenizer.decode(batch_chunks[j]),
                        "spam_score": spam_confs[j].item(),
                        "spam_label": spam_model.config.id2label[spam_preds[j].item()],
                        "toxic_score": toxic_confs[j].item(),
                        "toxic_label": toxic_model.config.id2label[toxic_preds[j].item()],
                    })

            # 3. Aggregate all results using the standard logic
            logger.info(f"Aggregating harmful detection result with spam threshold {spam_threshold} and toxic threshold {toxic_threshold}")
            agg = self.aggregation_logic(chunk_results, spam_threshold=spam_threshold, toxic_threshold=toxic_threshold)
            self.logger.info(f"Aggregated results:\n{json.dumps(agg, indent=4,ensure_ascii=False)}")
            elapsed_ms = (time.time() - start_time) * 1000
            
            action = agg.get("action", "APPROVED")
            score = agg.get("score", 1.0)
            
            # Combine up to a reasonable length for display
            display_text = " | ".join(texts)
            if len(display_text) > 60:
                display_text = display_text[:60] + "..."
                
            details = {
                "text": agg.get("text", display_text),
                "score": score,
                "raw_label": agg.get("raw_label", "SAFE"),
                "latency_ms": elapsed_ms,
                "reason": agg.get("reason", "Inference complete")
            }
            return action, score, details

        except Exception as e:
            logger.error(f"Text list classification robust pipeline failed: {e}")
            raise ModelInferenceException("text_classifier", str(e))
