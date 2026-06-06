# Implementation Plan - Pipeline Bug Fixes

We will systematically resolve the three pipeline errors encountered during full pipeline moderation.

## Proposed Changes

### 1. Robust Feature Extraction in CLIP/Vision Embeddings (Easy)
We will make the vision features extraction in `services/embedding_service.py` extremely robust to accommodate transformer models returning `BaseModelOutputWithPooling` or standard raw PyTorch tensors.

#### [MODIFY] [embedding_service.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/services/embedding_service.py)
* Refactor feature tensor retrieval in `embed_image` and `embed_video_frame` to safely check for `pooler_output` or `last_hidden_state` before calling `.cpu().numpy()`.

---

### 2. URL Null-Guard in Duplication Handler (Easy)
We will guard the downloader loop in `DuplicationHandler.orchestrate_stage1` to bypass materials with empty or `None` URLs.

#### [MODIFY] [duplication_handler.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/handlers/duplication_handler.py)
* Check if `material.material_url` is present. If it is empty or `null`, log a warning and skip downloading/embedding generation for that material.

---

### 3. Log Attribute Lookup Safety (Easy)
We will fix key/attribute extraction for pipeline Stage Logs, making it fully compatible with both Pydantic models and raw dictionary objects.

#### [MODIFY] [harmful_handler.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/handlers/harmful_handler.py)
* Replace direct attribute access `step1_logs[0].confidence_score` with safe `.get("confidence_score")` dict lookup.

#### [MODIFY] [moderation.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/api/routes/moderation.py)
* Implement a helper to retrieve `confidence_score` and `result` from stage logs safely regardless of whether they are Pydantic objects or dictionaries.

---

## Verification Plan

### Automated Tests
- Run Python verification script to ensure all modules load successfully:
  ```bash
  C:\Users\anhkc\anaconda3\envs\mlenv\python.exe -c "import sys; sys.path.append('.'); import services.embedding_service; import handlers.duplication_handler; import handlers.harmful_handler; import api.routes.moderation; print('SUCCESS')"
  ```
