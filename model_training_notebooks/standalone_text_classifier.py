"""
Standalone Text Classifier Script
--------------------------------
Isolated inference script for text toxicity and spam classification using fine-tuned DistilBERT models.
Can be executed directly via command line with hardcoded samples or custom text inputs.

Output folder: AIModeration/inference_test/standalone_text_classifier.py
"""

import argparse
import json
import logging
import os
import sys
import time
from enum import Enum
from typing import Any, Dict, List, Optional, Tuple, Union

import torch
import torch.nn.functional as F
from transformers import AutoModelForSequenceClassification, AutoTokenizer

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s"
)
logger = logging.getLogger("StandaloneTextClassifier")


# ============================================================================
# Isolated Enums & Data Structures
# ============================================================================

class ModerationStatus(str, Enum):
    """Moderation decision status."""
    APPROVED = "APPROVED"
    FLAGGED = "FLAGGED"
    MANUAL_AUDIT = "MANUAL_AUDIT"
    PENDING = "PENDING"
    SKIPPED = "SKIPPED"


class HarmfulClassificationLabel(str, Enum):
    """Classification label types."""
    SAFE = "SAFE"
    TOXIC = "TOXIC"
    SPAM = "SPAM"


# ============================================================================
# Standalone Model Provider & Inference Engine
# ============================================================================

class StandaloneTextClassifier:
    """
    Self-contained text classifier logic extracted from TextClassifierService.
    Manages model loading and inference without external project dependencies.
    """

    def __init__(
        self,
        spam_model_path: Optional[str] = None,
        toxic_model_path: Optional[str] = None,
        device: Optional[str] = None
    ):
        """
        Initialize classifier and resolve model paths.
        """
        self.device = torch.device(
            device if device else ("cuda" if torch.cuda.is_available() else "cpu")
        )
        logger.info(f"Using compute device: {self.device}")

        # Resolve model paths
        self.spam_model_path = self._resolve_model_path(
            spam_model_path, "SPAM_MODEL_PATH", ["ai_models/spam_1", "../ai_models/spam_1", "app/models/spam_1", "models/spam_1"]
        )
        self.toxic_model_path = self._resolve_model_path(
            toxic_model_path, "TOXIC_MODEL_PATH", ["ai_models/toxic_3", "../ai_models/toxic_3", "app/models/toxic_3", "models/toxic_3"]
        )

        logger.info(f"Resolved Spam Model Path: {self.spam_model_path}")
        logger.info(f"Resolved Toxicity Model Path: {self.toxic_model_path}")

        # Model cache
        self.spam_tokenizer = None
        self.spam_model = None
        self.toxic_tokenizer = None
        self.toxic_model = None

    def _resolve_model_path(self, explicit_path: Optional[str], env_var: str, candidates: List[str]) -> str:
        """Resolve model path from explicit argument, env var, or local search candidates."""
        if explicit_path and os.path.exists(explicit_path):
            return os.path.abspath(explicit_path)

        env_path = os.getenv(env_var)
        if env_path and os.path.exists(env_path):
            return os.path.abspath(env_path)

        # Try relative paths from current file / working directory
        base_dirs = [
            os.getcwd(),
            os.path.dirname(os.path.abspath(__file__)),
            os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
            os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
        ]

        for base in base_dirs:
            for rel in candidates:
                cand_path = os.path.abspath(os.path.join(base, rel))
                if os.path.exists(cand_path):
                    return cand_path

        # Return default if not found (transformers will give explicit error if missing)
        return explicit_path or candidates[0]

    def load_models(self):
        """Load and cache spam and toxicity tokenizers and models."""
        if self.spam_model is None or self.spam_tokenizer is None:
            logger.info(f"Loading Spam Model from: {self.spam_model_path}...")
            self.spam_tokenizer = AutoTokenizer.from_pretrained(self.spam_model_path, local_files_only=True)
            self.spam_model = AutoModelForSequenceClassification.from_pretrained(self.spam_model_path, local_files_only=True)
            self.spam_model.to(self.device)
            self.spam_model.eval()
            logger.info("✓ Spam Model loaded successfully")

        if self.toxic_model is None or self.toxic_tokenizer is None:
            logger.info(f"Loading Toxicity Model from: {self.toxic_model_path}...")
            self.toxic_tokenizer = AutoTokenizer.from_pretrained(self.toxic_model_path, local_files_only=True)
            self.toxic_model = AutoModelForSequenceClassification.from_pretrained(self.toxic_model_path, local_files_only=True)
            self.toxic_model.to(self.device)
            self.toxic_model.eval()
            logger.info("✓ Toxicity Model loaded successfully")

    def check_course_description(self, text: str) -> Dict[str, Any]:
        """Single-pass classification for short texts (up to 128 max tokens)."""
        self.load_models()
        tokenizer = self.spam_tokenizer
        spam_model = self.spam_model
        toxic_model = self.toxic_model

        inputs = tokenizer(
            text,
            return_tensors="pt",
            truncation=True,
            padding="max_length",
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

        return {
            "text": text,
            "spam_score": spam_conf_score,
            "spam_label": spam_label,
            "toxic_score": toxic_conf_score,
            "toxic_label": toxic_label
        }

    def robust_sliding_window(self, text: str, window_size: int = 128, stride: int = 64) -> List[Dict[str, Any]]:
        """Chunk text using a sliding window and classify each segment."""
        self.load_models()
        tokenizer = self.spam_tokenizer
        spam_model = self.spam_model
        toxic_model = self.toxic_model

        tokens = tokenizer.encode(text, add_special_tokens=False)
        length = len(tokens)

        # If text is short, perform single pass
        if length <= window_size - 2:
            return [self.check_course_description(text)]

        chunk_results = []

        for i in range(0, length, stride):
            # Reserve 2 spots for [CLS] and [SEP]
            chunk = tokens[i : i + window_size - 2]
            input_ids = [tokenizer.cls_token_id] + chunk + [tokenizer.sep_token_id]
            attention_mask = [1] * len(input_ids)

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

    def aggregation_logic(
        self,
        chunk_results: List[Dict[str, Any]],
        spam_threshold: float = 0.85,
        toxic_threshold: float = 0.85
    ) -> Dict[str, Any]:
        """Aggregate chunk-level classification results into a final decision."""
        candidates = []

        # 1. Identify High-Confidence Threats (Severe)
        high_conf_spams = [
            r for r in chunk_results
            if (r["spam_label"] != HarmfulClassificationLabel.SAFE.value and r["spam_score"] >= spam_threshold)
        ]
        high_conf_toxics = [
            r for r in chunk_results
            if (r["toxic_label"] != HarmfulClassificationLabel.SAFE.value and r["toxic_score"] >= toxic_threshold)
        ]

        if high_conf_spams or high_conf_toxics:
            for threat in high_conf_spams:
                candidates.append({
                    "text": threat["text"],
                    "score": threat["spam_score"],
                    "difference": abs(threat["spam_score"] - spam_threshold),
                    "label": threat["spam_label"]
                })
            for threat in high_conf_toxics:
                candidates.append({
                    "text": threat["text"],
                    "score": threat["toxic_score"],
                    "difference": abs(threat["toxic_score"] - toxic_threshold),
                    "label": threat["toxic_label"]
                })

            most_severe_threat = max(candidates, key=lambda x: x["difference"])
            return {
                "text": most_severe_threat["text"],
                "action": ModerationStatus.FLAGGED.value,
                "reason": "Severe Threat",
                "score": most_severe_threat["score"],
                "raw_label": most_severe_threat["label"]
            }

        # 2. Identify Low-Confidence Threats (Probable Threat -> MANUAL_AUDIT)
        low_conf_spams = [
            r for r in chunk_results
            if (r["spam_label"] != HarmfulClassificationLabel.SAFE.value and r["spam_score"] < spam_threshold)
        ]
        low_conf_toxics = [
            r for r in chunk_results
            if (r["toxic_label"] != HarmfulClassificationLabel.SAFE.value and r["toxic_score"] < toxic_threshold)
        ]

        if low_conf_spams or low_conf_toxics:
            for threat in low_conf_spams:
                candidates.append({
                    "text": threat["text"],
                    "score": threat["spam_score"],
                    "difference": abs(threat["spam_score"] - spam_threshold),
                    "label": threat["spam_label"]
                })
            for threat in low_conf_toxics:
                candidates.append({
                    "text": threat["text"],
                    "score": threat["toxic_score"],
                    "difference": abs(threat["toxic_score"] - toxic_threshold),
                    "label": threat["toxic_label"]
                })

            most_suspicious_threat = min(candidates, key=lambda x: x["difference"])
            return {
                "text": most_suspicious_threat["text"],
                "action": ModerationStatus.MANUAL_AUDIT.value,
                "reason": "Probable Threat",
                "score": most_suspicious_threat["score"],
                "raw_label": most_suspicious_threat["label"]
            }

        # 3. Identify Low-Confidence Safes (Ambiguous Content -> MANUAL_AUDIT)
        low_conf_non_spams = [
            r for r in chunk_results
            if (r["spam_label"] == HarmfulClassificationLabel.SAFE.value and r["spam_score"] < spam_threshold)
        ]
        low_conf_non_toxics = [
            r for r in chunk_results
            if (r["toxic_label"] == HarmfulClassificationLabel.SAFE.value and r["toxic_score"] < toxic_threshold)
        ]

        if low_conf_non_spams or low_conf_non_toxics:
            for threat in low_conf_non_spams:
                candidates.append({
                    "text": threat["text"],
                    "score": threat["spam_score"],
                    "difference": abs(threat["spam_score"] - spam_threshold),
                    "label": threat["spam_label"]
                })
            for threat in low_conf_non_toxics:
                candidates.append({
                    "text": threat["text"],
                    "score": threat["toxic_score"],
                    "difference": abs(threat["toxic_score"] - toxic_threshold),
                    "label": threat["toxic_label"]
                })

            most_confused_safe = max(candidates, key=lambda x: x["difference"])
            return {
                "text": most_confused_safe["text"],
                "action": ModerationStatus.MANUAL_AUDIT.value,
                "reason": "Ambiguous Content",
                "score": most_confused_safe["score"],
                "raw_label": most_confused_safe["label"]
            }

        # 4. APPROVED (No Threat Found)
        avg_score = sum(r["spam_score"] + r["toxic_score"] for r in chunk_results) / (2 * len(chunk_results))
        return {
            "action": ModerationStatus.APPROVED.value,
            "reason": "No Threat Found",
            "score": avg_score,
            "raw_label": HarmfulClassificationLabel.SAFE.value
        }

    def _get_empty_details(self) -> Dict[str, Any]:
        return {
            "text": "",
            "score": 1.0,
            "raw_label": HarmfulClassificationLabel.SAFE.value,
            "latency_ms": 0.0,
            "reason": "Empty text"
        }

    def classify_text(
        self,
        text: str,
        spam_threshold: float = 0.85,
        toxic_threshold: float = 0.85,
        window_size: int = 128,
        stride: int = 64
    ) -> Tuple[str, float, Dict[str, Any]]:
        """Classify single text string through robust sliding window pipeline."""
        if not text or not text.strip():
            return ModerationStatus.APPROVED.value, 1.0, self._get_empty_details()

        start_time = time.time()
        chunk_results = self.robust_sliding_window(text, window_size=window_size, stride=stride)
        agg = self.aggregation_logic(chunk_results, spam_threshold=spam_threshold, toxic_threshold=toxic_threshold)
        elapsed_ms = (time.time() - start_time) * 1000

        action = agg.get("action", ModerationStatus.APPROVED.value)
        score = agg.get("score", 1.0)

        details = {
            "text": agg.get("text", text[:60] + "..."),
            "score": score,
            "raw_label": agg.get("raw_label", HarmfulClassificationLabel.SAFE.value),
            "latency_ms": round(elapsed_ms, 2),
            "reason": agg.get("reason", "Inference complete"),
            "chunk_count": len(chunk_results),
            "chunk_results": chunk_results
        }
        return action, score, details

    def classify_text_list(
        self,
        texts: List[str],
        spam_threshold: float = 0.85,
        toxic_threshold: float = 0.85,
        window_size: int = 128
    ) -> Tuple[str, float, Dict[str, Any]]:
        """Classify a list of text strings using batched sliding window inference."""
        if not texts:
            return ModerationStatus.APPROVED.value, 1.0, self._get_empty_details()

        start_time = time.time()
        self.load_models()
        tokenizer = self.spam_tokenizer
        spam_model = self.spam_model
        toxic_model = self.toxic_model

        all_chunks = []
        stride = 64

        for text in texts:
            if not text.strip():
                continue
            tokens = tokenizer.encode(text, add_special_tokens=False)
            length = len(tokens)

            if length <= window_size - 2:
                all_chunks.append(tokens)
            else:
                for i in range(0, length, stride):
                    chunk = tokens[i : i + window_size - 2]
                    all_chunks.append(chunk)
                    if i + window_size - 2 >= length:
                        break

        if not all_chunks:
            return ModerationStatus.APPROVED.value, 1.0, self._get_empty_details()

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

            for j in range(len(batch_chunks)):
                chunk_results.append({
                    "text": tokenizer.decode(batch_chunks[j]),
                    "spam_score": spam_confs[j].item(),
                    "spam_label": spam_model.config.id2label[spam_preds[j].item()],
                    "toxic_score": toxic_confs[j].item(),
                    "toxic_label": toxic_model.config.id2label[toxic_preds[j].item()],
                })

        agg = self.aggregation_logic(chunk_results, spam_threshold=spam_threshold, toxic_threshold=toxic_threshold)
        elapsed_ms = (time.time() - start_time) * 1000

        action = agg.get("action", ModerationStatus.APPROVED.value)
        score = agg.get("score", 1.0)

        display_text = " | ".join(texts)
        if len(display_text) > 60:
            display_text = display_text[:60] + "..."

        details = {
            "text": agg.get("text", display_text),
            "score": score,
            "raw_label": agg.get("raw_label", HarmfulClassificationLabel.SAFE.value),
            "latency_ms": round(elapsed_ms, 2),
            "reason": agg.get("reason", "Inference complete"),
            "total_texts": len(texts),
            "total_chunks": len(chunk_results)
        }
        return action, score, details


# ============================================================================
# CMD / Hardcoded Execution Logic
# ============================================================================

HARDCODED_SAMPLES = [
    {
        "name": "Sample 1: Clean Course Description (Safe)",
        "text": "Welcome to Complete Python Masterclass. In this course, you will learn Python programming from scratch to advanced level with real-world projects and clean coding practices."
    },
    {
        "name": "Sample 2: Spam Text Sample",
        "text": "BUY CHEAP FOLLOWER INSTANT DISCOUNT 90% OFF! Click http://scam-link-discount-bonus.biz NOW to claim free crypto money and 100% guaranteed profit instantly!!!"
    },
    {
        "name": "Sample 3: Toxic / Offensive Text Sample",
        "text": "You are completely stupid, worthless, and ugly! Nobody likes you, get out of here right now you idiot!"
    },
    {
        "name": "Sample 4: Long Mixed Text Sample (Testing Sliding Window)",
        "text": (
            "In this section, we cover data structures, algorithms, and system design principles. "
            "Students are encouraged to participate in code reviews and practice daily coding exercises. "
            "However, if you don't submit your homework, you are a complete idiot and piece of trash! "
            "Also visit http://cheap-crypto-scam.com for 99% off discount vouchers today!"
        )
    }
]


def run_cli():
    parser = argparse.ArgumentParser(description="Standalone Text Toxicity and Spam Classifier")
    parser.add_argument("--text", type=str, help="Custom text sample to classify")
    parser.add_argument("--file", type=str, help="Path to text file to classify")
    parser.add_argument("--spam-path", type=str, help="Custom directory path to Spam model")
    parser.add_argument("--toxic-path", type=str, help="Custom directory path to Toxicity model")
    parser.add_argument("--spam-threshold", type=float, default=0.85, help="Threshold score for Spam detection (default: 0.85)")
    parser.add_argument("--toxic-threshold", type=float, default=0.85, help="Threshold score for Toxicity detection (default: 0.85)")
    parser.add_argument("--window-size", type=int, default=128, help="Sliding window token size (default: 128)")
    parser.add_argument("--stride", type=int, default=64, help="Sliding window stride size (default: 64)")
    parser.add_argument("--device", type=str, help="Compute device override (e.g. 'cpu', 'cuda')")

    args = parser.parse_args()

    classifier = StandaloneTextClassifier(
        spam_model_path=args.spam_path,
        toxic_model_path=args.toxic_path,
        device=args.device
    )

    print("=" * 80)
    print(" STANDALONE TEXT CLASSIFIER INFERENCE TEST")
    print("=" * 80)

    # 1. Custom Single Text / File input
    if args.text or args.file:
        input_text = args.text
        if args.file:
            with open(args.file, "r", encoding="utf-8") as f:
                input_text = f.read()

        print(f"\n[+] Classifying Input Text (Length: {len(input_text)} chars)...")
        action, score, details = classifier.classify_text(
            text=input_text,
            spam_threshold=args.spam_threshold,
            toxic_threshold=args.toxic_threshold,
            window_size=args.window_size,
            stride=args.stride
        )
        print("\n--- INFERENCE RESULT ---")
        print(f"Action:      {action}")
        print(f"Confidence:  {score:.4f}")
        print(f"Details:\n{json.dumps(details, indent=2, ensure_ascii=False)}")

    # 2. Default: Run Hardcoded Test Suite
    else:
        print("\n[*] No custom text/file provided. Running hardcoded test suite...\n")

        for idx, sample in enumerate(HARDCODED_SAMPLES, start=1):
            print("-" * 80)
            print(f"[{idx}/{len(HARDCODED_SAMPLES)}] {sample['name']}")
            print(f"Input Text: \"{sample['text']}\"")

            action, score, details = classifier.classify_text(
                text=sample['text'],
                spam_threshold=args.spam_threshold,
                toxic_threshold=args.toxic_threshold,
                window_size=args.window_size,
                stride=args.stride
            )

            print(f"Result Action:     [{action}]")
            print(f"Confidence Score:  {score:.4f}")
            print(f"Reason:            {details.get('reason')}")
            print(f"Raw Label:         {details.get('raw_label')}")
            print(f"Latency:           {details.get('latency_ms')} ms")
            if "chunk_results" in details:
                print(f"Chunks Processed:  {len(details['chunk_results'])}")
                for c_idx, chunk in enumerate(details["chunk_results"]):
                    print(f"  |- Chunk {c_idx+1}: Spam={chunk['spam_label']} ({chunk['spam_score']:.4f}), Toxic={chunk['toxic_label']} ({chunk['toxic_score']:.4f})")

        # Also test list classification
        print("\n" + "=" * 80)
        print(" TESTING BATCHED LIST CLASSIFICATION (classify_text_list)")
        print("=" * 80)
        sample_list = [s["text"] for s in HARDCODED_SAMPLES]
        action, score, details = classifier.classify_text_list(
            texts=sample_list,
            spam_threshold=args.spam_threshold,
            toxic_threshold=args.toxic_threshold,
            window_size=args.window_size
        )
        print(f"Batch Action:      [{action}]")
        print(f"Batch Confidence:  {score:.4f}")
        print(f"Batch Details:\n{json.dumps(details, indent=2, ensure_ascii=False)}")

    print("\n" + "=" * 80)
    print(" INFERENCE TEST COMPLETED SUCCESSFULLY")
    print("=" * 80)


if __name__ == "__main__":
    run_cli()
