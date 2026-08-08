# Spam & Toxic Text Classifier Technical Documentation

This document provides a comprehensive technical overview of the dual-model spam and toxic text classification pipeline driving [harmful_handler.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/handlers/harmful_handler.py) and [text_classifier_service.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/services/text_classifier_service.py).

---

## 1. Base Model Specifications

- **Provider**: Hugging Face / Google (DistilBERT architecture)
- **Base Model Name**: `distilbert-base-multilingual-cased`
- **Architecture**: 6-layer Transformer encoder, 768 hidden dimensions, 12 attention heads, ~134M parameters.
- **Rationale**: Chosen for its lightweight footprint (40% fewer parameters than BERT base while retaining 95% of performance), multilingual capabilities (English & Vietnamese token support), and low inference latency suitable for CPU/GPU server deployments.

---

## 2. Training Algorithms, Strategies & Techniques

### **Techniques & Strategies**
- **Transfer Learning & Fine-Tuning**: 
  - *What*: Replaced the default pre-training head with a 2-class sequence classification head (`num_labels=2`, dropout=0.3) for binary toxicity (`SAFE` vs `TOXIC`) and spam classification (`SAFE` vs `SPAM`).
  - *Why*: Leverages pre-trained multilingual context representation while training downstream classifiers efficiently on specialized domains.
- **Custom Class Weighting (`WeightedTrainer` with `CrossEntropyLoss`)**:
  - *What*: Dynamically computes loss weights using `Weight = total_samples / (num_classes * class_samples)` and applies them to `nn.CrossEntropyLoss`.
  - *Why*: Solves severe class imbalance issues between safe content and toxic/spam samples, ensuring minority threat classes get proper loss gradients.
- **Cosine Annealing Learning Rate Schedule with Warmup**:
  - *What*: Linear warmup (`warmup_steps=0.2`) followed by Cosine Annealing decay with peak learning rate 1e-5 and `weight_decay=0.01`.
  - *Why*: Solves gradient instability early in training and smooths convergence without trapping the model in sharp local minima.
- **Mixed Precision Training (`fp16=True`)**:
  - *What*: Utilizes 16-bit floating-point execution on CUDA GPUs.
  - *Why*: Reduces GPU memory consumption by ~50% and doubles training/eval throughput without sacrificing model accuracy.
- **Gradient Accumulation (`gradient_accumulation_steps=2`)**:
  - *What*: Accumulates gradients over 2 micro-batches (`per_device_train_batch_size=16`) to achieve an effective batch size of 32.
  - *Why*: Allows large effective batch sizes on VRAM-constrained hardware without triggering Out-Of-Memory (OOM) exceptions.
- **Early Stopping & Regularization**:
  - *What*: `EarlyStoppingCallback(patience=3)`, gradient clipping (`max_grad_norm=1.0`), and dropout (0.3).
  - *Why*: Prevents overfitting on synthetic or domain-specific dataset patterns and selects the optimal checkpoint based on `eval_f1_macro`.

---

## 3. Dataset Specifications

### **Toxicity Dataset ([toxic-text-classifier.ipynb](file:///c:/Users/anhkc/Desktop/Linked/model_training_notebooks/toxic-text-classifier.ipynb))**
- **Total Dataset Size**: 199,995 items (180,000 Train / 9,996 Validation / 9,999 Test)
- **Sources & Distribution**:
  - **Vietnamese Data (`tarudesu/VOZ-HSD`)**: Stratified across toxicity probability buckets (Safe >0.995, Mild 0.5 - 0.7, Moderate 0.7 - 0.9, Severe max probability).
  - **English Data (`google/civil_comments`)**: Stratified across toxicity score thresholds (Safe toxicity = 0, Mild 0.1 - 0.4, Average 0.4 - 0.7, Severe >= 0.7).
- **Label Mapping**: `0: SAFE`, `1: TOXIC`

### **Spam Dataset ([spam-text-classifier.ipynb](file:///c:/Users/anhkc/Desktop/Linked/model_training_notebooks/spam-text-classifier.ipynb))**
- **Total Dataset Size**: 119,241 items (95,392 Train / 11,923 Validation / 11,926 Test)
- **Sources & Distribution**:
  - **Vietnamese Synthetic Data**: Generated using `Faker('vi_VN')` with templates including fake phishing URLs (`bit.ly`, `vn-xac-thuc.com`), realistic phone formats (`09x`, `03x`), bank/amount placeholders, and natural ham templates.
  - **English Data (`mshenoda/spam-messages`)**: Cleaned English ham/spam message dataset.
- **Label Mapping**: `0: SAFE`, `1: SPAM`

---

## 4. Model Evaluation & Training Results

### **Toxicity Model Results ([toxic-text-classifier.ipynb](file:///c:/Users/anhkc/Desktop/Linked/model_training_notebooks/toxic-text-classifier.ipynb))**
- **Training Duration**: 7 Epochs (Early stopped) | **Training Loss**: `0.4554`
- **Validation Metrics**:
  - **Loss**: `0.2334` | **Accuracy**: `90.80%` | **Precision**: `95.01%`
  - **Recall**: `91.67%` | **F1 Score**: `93.31%` | **Macro F1**: `89.29%` | **Weighted F1**: `90.90%`
- **Test Set Metrics**:
  - **Loss**: `0.2369` | **Accuracy**: `91.06%` | **Precision**: `94.76%`
  - **Recall**: `92.33%` | **F1 Score**: `93.53%` | **Macro F1**: `89.53%` | **Weighted F1**: `91.13%`
  - **Per-Class Breakout**:
    - `SAFE`: Precision `0.83`, Recall `0.88`, F1 `0.86` (Support: 3,000)
    - `TOXIC`: Precision `0.95`, Recall `0.92`, F1 `0.94` (Support: 6,999)

### **Spam Model Results ([spam-text-classifier.ipynb](file:///c:/Users/anhkc/Desktop/Linked/model_training_notebooks/spam-text-classifier.ipynb))**
- **Training Duration**: 10 Epochs | **Training Loss**: `0.1013`
- **Validation Metrics**:
  - **Loss**: `0.0570` | **Accuracy**: `99.15%` | **Precision**: `99.11%`
  - **Recall**: `98.80%` | **F1 Score**: `98.96%` | **Macro F1**: `99.12%` | **Weighted F1**: `99.15%`
- **Test Set Metrics**:
  - **Loss**: `0.0582` | **Accuracy**: `99.07%` | **Precision**: `99.13%`
  - **Recall**: `98.52%` | **F1 Score**: `98.82%` | **Macro F1**: `99.03%` | **Weighted F1**: `99.07%`
  - **Per-Class Breakout**:
    - `SAFE`: Precision `0.99`, Recall `0.99`, F1 `0.99` (Support: 7,195)
    - `SPAM`: Precision `0.99`, Recall `0.99`, F1 `0.99` (Support: 4,731)

---

## 5. Inference Algorithms, Strategies & Techniques

Implemented in [text_classifier_service.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/services/text_classifier_service.py) and [standalone_text_classifier.py](file:///c:/Users/anhkc/Desktop/Linked/model_training_notebooks/standalone_text_classifier.py):

- **Dual-Model Parallel Ensemble**:
  - *What*: Evaluates incoming text simultaneously through both fine-tuned models (`spam_model` and `toxic_model`).
  - *Why*: Separates toxicity detection from spam detection into dedicated domain models, yielding higher accuracy than multi-task heads.
- **Robust Sliding Window Chunking (`window_size=128`, `stride=64`)**:
  - *What*: Chunks long course descriptions/documents into overlapping 128-token windows with 64-token stride, preserving special tokens (`[CLS]`, `[SEP]`).
  - *Why*: Solves the transformer max token length truncation limitation. Ensures toxic or spam sentences hidden deep in long text are never clipped or missed.
- **Dynamic Batched Inference (`batch_size=16`)**:
  - *What*: Batches chunks from multiple extracted texts into tensor batches for vectorized GPU/CPU forward passes.
  - *Why*: Reduces overall latency when processing full courses with dozens of extracted text fields and media transcripts.
- **Hierarchical Priority Aggregation Logic**:
  - *What*: Aggregates chunk-level predictions across 4 tier decisions:
    1. **High-Confidence Threat (>= threshold)** -> `FLAGGED` (Reason: *Severe Threat*)
    2. **Low-Confidence Threat (< threshold but labelled non-safe)** -> `MANUAL_AUDIT` (Reason: *Probable Threat*)
    3. **Low-Confidence Safe (< threshold safe prediction)** -> `MANUAL_AUDIT` (Reason: *Ambiguous Content*)
    4. **All Clear** -> `APPROVED` (Reason: *No Threat Found*)
  - *Why*: Solves false-negative risks by routing border-line predictions to human moderators while instantly flagging high-confidence policy violations.

---

## Validation Checklist

- [x] **Base Model Details**: Documented provider (`Hugging Face/Google`) and exact base model name (`distilbert-base-multilingual-cased`).
- [x] **Algorithms, Strategies & Techniques**: Explained fine-tuning, custom weighted loss, cosine annealing, mixed precision, gradient accumulation, and regularization along with *why* they were used.
- [x] **Dataset Details**: Detailed dataset compositions, sample counts, sources (VOZ-HSD, Civil Comments, Faker synthetic Viet-Spam, mshenoda spam), and train/val/test splits.
- [x] **Training Results**: Detailed loss, accuracy, precision, recall, and F1 scores (binary, macro, weighted) for both training, validation, and testing splits.
- [x] **Inference Strategy**: Explained dual-model ensemble, sliding window chunking (128/64), micro-batching (16), and 4-tier threat aggregation logic.
- [x] **Format & Style**: Straight-to-the-point bullet points in plain markdown without unparsed LaTeX symbols.
- [x] **Cleanup Verification**: All temporary scratch/dump files deleted.
