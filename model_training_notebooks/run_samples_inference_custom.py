"""
Run inference on all samples from samples.py with toxic_threshold = 0.9 and spam_threshold = 0.95.
Output location: AIModeration/inference_test/inference_results_toxic0.9_spam0.95.csv
Columns: text, action, score, reason, raw_label, true_label
"""

import os
import csv
import sys
import logging

# Ensure current directory is in sys.path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from standalone_text_classifier import StandaloneTextClassifier

logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
logger = logging.getLogger("SamplesInferenceCustom")


def parse_samples(samples_file_path: str):
    """
    Parse samples.py line-by-line to extract text samples along with their true labels.
    Returns list of tuples: (text, true_label)
    """
    samples = []
    current_label = None
    in_string = False
    current_text_lines = []

    with open(samples_file_path, "r", encoding="utf-8") as f:
        for line in f:
            stripped = line.strip()

            # Check for label header comments
            if stripped.startswith("#"):
                if "- TOXIC" in stripped:
                    current_label = "TOXIC"
                elif "- SPAM" in stripped:
                    current_label = "SPAM"
                elif "- SAFE" in stripped:
                    current_label = "SAFE"
                continue

            # Check for docstring start/end
            if '"""' in stripped:
                if not in_string:
                    in_string = True
                    parts = line.split('"""', 1)
                    if len(parts) > 1 and parts[1].strip():
                        sub_parts = parts[1].split('"""', 1)
                        if len(sub_parts) > 1:
                            text = sub_parts[0].strip()
                            if text:
                                samples.append((text, current_label))
                            in_string = False
                        else:
                            current_text_lines.append(parts[1])
                else:
                    parts = line.split('"""', 1)
                    current_text_lines.append(parts[0])
                    full_text = "".join(current_text_lines).strip()
                    if full_text:
                        samples.append((full_text, current_label))
                    current_text_lines = []
                    in_string = False
            elif in_string:
                current_text_lines.append(line)

    return samples


def main():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    samples_path = os.path.join(script_dir, "samples.py")
    output_csv_path = os.path.join(script_dir, "inference_results_toxic0.9_spam0.95.csv")

    spam_model_path = os.path.abspath(os.path.join(script_dir, "../../ai_models/spam_1"))
    toxic_model_path = os.path.abspath(os.path.join(script_dir, "../../ai_models/toxic_3"))

    logger.info(f"Loading samples from: {samples_path}")
    parsed_samples = parse_samples(samples_path)
    logger.info(f"Found total {len(parsed_samples)} text samples.")

    toxic_thresh = 0.9
    spam_thresh = 0.95
    logger.info(f"Running inference with toxic_threshold = {toxic_thresh} and spam_threshold = {spam_thresh}...")

    classifier = StandaloneTextClassifier(
        spam_model_path=spam_model_path,
        toxic_model_path=toxic_model_path
    )
    classifier.load_models()

    results = []
    print("\n" + "=" * 80)
    print(f" RUNNING INFERENCE ON {len(parsed_samples)} SAMPLES (Toxic: {toxic_thresh}, Spam: {spam_thresh})")
    print("=" * 80)

    for idx, (text, true_label) in enumerate(parsed_samples, start=1):
        action, score, details = classifier.classify_text(
            text,
            spam_threshold=spam_thresh,
            toxic_threshold=toxic_thresh
        )
        reason = details.get("reason", "")
        raw_label = details.get("raw_label", "")

        results.append({
            "text": text,
            "action": action,
            "score": round(score, 6),
            "reason": reason,
            "raw_label": raw_label,
            "true_label": true_label
        })

        if idx % 20 == 0 or idx == len(parsed_samples):
            logger.info(f"Processed [{idx}/{len(parsed_samples)}] samples...")

    # Write to CSV
    fieldnames = ["text", "action", "score", "reason", "raw_label", "true_label"]
    with open(output_csv_path, "w", newline="", encoding="utf-8-sig") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(results)

    logger.info(f"✓ Inference results successfully saved to CSV: {output_csv_path}")

    # Metrics calculation
    fp_count = 0
    fn_count = 0
    fp_breakdown = {"TOXIC": 0, "SPAM": 0}
    fn_breakdown = {"TOXIC": 0, "SPAM": 0}

    action_fp = 0
    action_fn = 0

    total_safe = 0
    total_harmful = 0

    for row in results:
        t_lbl = row["true_label"]
        r_lbl = row["raw_label"]
        act = row["action"]

        if t_lbl == "SAFE":
            total_safe += 1
            if r_lbl in ("TOXIC", "SPAM"):
                fp_count += 1
                fp_breakdown[r_lbl] += 1
            if act != "APPROVED":
                action_fp += 1
        elif t_lbl in ("TOXIC", "SPAM"):
            total_harmful += 1
            if r_lbl == "SAFE":
                fn_count += 1
                fn_breakdown[t_lbl] += 1
            if act == "APPROVED":
                action_fn += 1

    print("\n" + "=" * 80)
    print(f" METRICS REPORT (Toxic Threshold = {toxic_thresh}, Spam Threshold = {spam_thresh})")
    print("=" * 80)
    print(f"1. true_label = SAFE but raw_label is TOXIC / SPAM (False Positives)")
    print(f"   • Total: {fp_count} out of {total_safe} SAFE samples.")
    print(f"     • raw_label = SPAM : {fp_breakdown['SPAM']}")
    print(f"     • raw_label = TOXIC : {fp_breakdown['TOXIC']}")
    print()
    print(f"2. true_label = TOXIC or SPAM but raw_label is SAFE (False Negatives)")
    print(f"   • Total: {fn_count} out of {total_harmful} harmful (TOXIC / SPAM) samples.")
    print(f"     • True SPAM predicted as SAFE : {fn_breakdown['SPAM']}")
    print(f"     • True TOXIC predicted as SAFE : {fn_breakdown['TOXIC']}")
    print()
    print(f"Additional Breakdown by Final Action (APPROVED vs FLAGGED / MANUAL_AUDIT)")
    print(f"   • true_label = SAFE but action != APPROVED (FLAGGED or MANUAL_AUDIT) : {action_fp}")
    print(f"   • true_label = TOXIC or SPAM but action == APPROVED : {action_fn}")
    print("=" * 80)


if __name__ == "__main__":
    main()
