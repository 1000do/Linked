# Implementation Plan - AI Moderation Pipeline Refinement

This plan details the architecture and file-level modifications to refine the AI Moderation pipeline. The updates will improve semantic deduplication, streamline toxicity & spam classification, introduce a `handlers` layer in the FastAPI service, align the C# Entities/DTOs with `5.sql`, and integrate new database-configured AI models.

---

## User Review Required

> [!IMPORTANT]
> 1. **Schema Updates (process_type)**: We will add the `ProcessType` property to the C# `AiModel` entity and update `AppDbContext.cs` mappings to align with `5.sql`. We assume `5.sql` has been executed manually against the database.
> 2. **MaterialEmbedding Caching in Redis**: C# `PrepareMaterialEmbeddingsAsync` and `SaveMaterialEmbeddingsAsync` will use the newly introduced DTO `MaterialEmbeddingResponse` to map/cache data in Redis under the `material_embedding:` prefix.
> 3. **FastAPI Redis Integration**: The FastAPI application will run a robust scan query to retrieve all cached embeddings, parsing both raw float lists and C#-formatted JSON objects to ensure zero deserialization issues.

---

## Open Questions

> [!NOTE]
> None. The provided instructions in `to_do_2.md` are extremely clear, precise, and have been mapped directly to components.

---

## Proposed Changes

### Component 1: CourseMarketplaceBE (C# Backend)

---

#### [MODIFY] [ModerationDTOs.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/ModerationDTOs.cs)
- Create `MaterialEmbeddingResponse` with fields:
  ```csharp
  public class MaterialEmbeddingResponse
  {
      public int EmbeddingId { get; set; }
      public int? MaterialId { get; set; }
      public string? Embedding { get; set; }
  }
  ```
- Rename `AiModelResponse` to `AiModelDto` and add `ProcessType` property:
  ```csharp
  public class AiModelDto
  {
      public int ModelId { get; set; }
      public string ModelName { get; set; } = null!;
      public string? ModelType { get; set; }
      public string? ModelProvider { get; set; }
      public string? ModelVersion { get; set; }
      public string? ModelStatus { get; set; }
      public string? Description { get; set; }
      public string? ModelPath { get; set; }
      public string? ProcessType { get; set; }
  }
  ```
- Modify `SemanticDuplicationRequest` and `HarmfulCourseRequest` to add:
  ```csharp
  [JsonPropertyName("models")]
  public List<AiModelDto> Models { get; set; } = [];
  ```

#### [MODIFY] [AiModel.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/AiModel.cs)
- Add `public string? ProcessType { get; set; }` to the entity class.

#### [MODIFY] [AppDbContext.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Data/AppDbContext.cs)
- Under the `ai_models` entity configuration, map `ProcessType` to the column `process_type`:
  ```csharp
  entity.Property(e => e.ProcessType).HasMaxLength(255).HasColumnName("process_type");
  ```

#### [MODIFY] [IAiModelRepository.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/IRepositories/IAiModelRepository.cs)
- Declare `Task<AiModelDto?> GetByModelPathAsync(string modelPath);` using `CourseMarketplaceBE.Application.DTOs`.

#### [MODIFY] [AiModelRepository.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Repositories/AiModelRepository.cs)
- Implement `GetByModelPathAsync` returning `AiModelDto` mapped directly from matching models.

#### [MODIFY] [IAiModerationService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/IServices/IAiModerationService.cs)
- Rename references of `AiModelResponse` to `AiModelDto`.

#### [MODIFY] [AiModerationService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/Services/AiModerationService.cs)
- Rename `AiModelResponse` references to `AiModelDto` and map `ProcessType` and `ModelPath` properties.

#### [MODIFY] [ILessonService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/IServices/ILessonService.cs)
- Modify `GetAllMaterialEmbeddingsAsync` signature to return `Task<List<MaterialEmbeddingResponse>>`.

#### [MODIFY] [LessonService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/Services/LessonService.cs)
- Update `GetAllMaterialEmbeddingsAsync` signature and map the database entity `MaterialEmbedding` to `MaterialEmbeddingResponse`.

#### [MODIFY] [IModerationService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/IServices/IModerationService.cs)
- Update `AssignAIModeratorsToCourseAsync` signature:
  ```csharp
  Task<AssignAIModeratorsToCourseResult> AssignAIModeratorsToCourseAsync(int courseId, List<AiModelDto> models);
  ```

#### [MODIFY] [ModerationService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/Services/ModerationService.cs)
- Inject `ISystemConfigRepository` and `IAiModelRepository` into `ModerationService`.
- Update `PrepareMaterialEmbeddingsAsync` to fetch `MaterialEmbeddingResponse` and cache it.
- Update `AssignAIModeratorsToCourseAsync` to accept `List<AiModelDto> models`, map `Role` value as `$"{model.ModelType}_{model.ProcessType}".ToLower()`, and integrate moderators.
- Update `PrepareForCourseAIModeration`:
  - Query keys `course_harmful_text_classifier`, `course_text_embedding_generator`, `course_media_embedding_generator` from `system_configs`.
  - Fetch corresponding `AiModelDto` objects using `GetByModelPathAsync`.
  - Fetch course integrations. If empty, invoke `AssignAIModeratorsToCourseAsync(courseId, models)`.
  - Otherwise, update existing integrations by matching the role prefix (`modelType` and `processType`), updating matching integrations where the model IDs differ, and logging changes.
  - Return the processed `List<AiModelDto> models` as part of `PrepareForCourseAIModerationResult`.
- Update `HandleCourseModerationWithAIAsync` to feed `prep.Models` matching generators/classifiers into `SemanticDuplicationRequest` and `CourseHarmfulRequest`.

---

### Component 2: AIModeration (FastAPI/Python Service)

---

#### [MODIFY] [models.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/core/models.py)
- Rename `CourseHarmfulRequest` to `HarmfulCourseRequest` to align with the specification.
- Define new Pydantic Models for transfers and mappings with attribute fields in snake_case:
  - `AiModelDto`
  - `EmbeddingGenerationCommand`
  - `EmbeddingGenerationResult`
  - `MaterialEmbeddingDto`
  - `MaterialMetadataDto`
  - `MaterialDto`
  - `LessonDto`
  - `CourseDto`
  - `CourseDetailDto`
- Add `models: List[AiModelDto]` to both `SemanticDuplicationRequest` and `HarmfulCourseRequest`.

#### [MODIFY] [cache_repository.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/repositories/cache_repository.py)
- Add prefix constants:
  - `AI_MODEL_PREFIX = 'ai_models'`
  - `EMBEDDINGS_INITIALIZED = 'material_embedding:initialized'`
- Remove `find_similar_embeddings` method.
- Add `get_existing_embeddings(self, key_prefix: str = 'material_embedding:')` using Redis cursor scan.
- Add `get_ai_models(self, key_suffix: str)` to fetch system models from key `{AI_MODEL_PREFIX}:{key_suffix}`.

#### [NEW] [redis_service.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/services/redis_service.py)
- Implement `get_course_details(course_id)`: fetches cache, maps lessons and materials, and returns `CourseDetailDto`.
- Implement `get_ai_models(key_suffix)`: fetches models, parses JSON array, and returns `List[AiModelDto]`.
- Implement `set_material_embedding(dto, ttl)`: extracts details and caches.
- Implement `get_all_existing_embeddings()`: scans all keys under `material_embedding:`, robustly parses JSON array lists of floats or serialized DTO models from C#, and returns `List[MaterialEmbeddingDto]`.

#### [MODIFY] [duplication_service.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/services/duplication_service.py)
- Move `cosine_similarity` method here.
- Remove `check_exact_duplication` (C# BE now handles this via exact content MD5 hashes).
- Implement `similarity_search(new_embedding, existing_embeddings, threshold)` returning dict with `material_id` and `similarity_score`.
- Modify `check_semantic_duplication` to compare new embeddings against `existing_embeddings` using the `similarity_search` utility, calculate confidence scores, build stage logs, and return status flags.

#### [MODIFY] [text_classifier_service.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/services/text_classifier_service.py)
- Drop legacy methods `classify_text`, `_sliding_window_aggregate`, `_aggregate_predictions`.
- Add standard `classify_text(self, text, spam_threshold = 0.85, toxic_threshold = 0.85, window_size = 128, stride = 64)`.
- Add `robust_sliding_window` chunking tokenized sentences dynamically and evaluating them against fine-tuned spam & toxic models using PyTorch.
- Add `check_course_description(self, text)` evaluating standard short length descriptions.
- Add `aggregation_logic` sorting chunk outcomes to classify text states into `FLAGGED`, `MANUAL_AUDIT`, or `APPROVED`.

#### [RENAME] [toxicity_service.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/services/toxicity_service.py) -> [harmful_service.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/services/harmful_service.py)
- Rename `ToxicityService` class to `HarmfulService`.
- Remove `check_visual_toxicity`.
- Rename check_text_toxicity -> check_text_harmful. Evaluate all course fields: title, description, what_you_will_learn, requirements, lesson titles, material titles, and material descriptions.
- Rename `check_media_text_toxicity` -> `check_media_text_harmful`. Process downloaded candidatos media, run text extraction, and classify them using the harmful classification service.

#### [MODIFY] [embedding_service.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/services/embedding_service.py)
- Modify `embed_generic` method signature and internals to accept `EmbeddingGenerationCommand` and return `EmbeddingGenerationResult`.

#### [MODIFY] [text_extraction_service.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/services/text_extraction_service.py)
- Add utility `get_file_type_for_text_extraction(file_extension: str) -> str` to map extensions to core categories (video, image, pdf, word, ppt).

#### [NEW] [base_handler.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/handlers/base_handler.py)
- Implement `BaseHandler` initialized with `logger` and `handler_name`.
- Implement robust `process_request_models(models: List[AiModelDto])` to handle comma-separated model paths, load models dynamically, compare with local environments, and fall back safely if loader fails.

#### [NEW] [duplication_handler.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/handlers/duplication_handler.py)
- Extends `BaseHandler`.
- Houses `orchestrate_stage1` which retrieves existing embeddings from Redis, downloads materials, extracts process types, generates embeddings, checks semantic duplication, and saves embeddings upon approval.

#### [NEW] [harmful_handler.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/handlers/harmful_handler.py)
- Extends `BaseHandler`.
- Houses `orchestrate_stage2` checking text properties first, downloading course thumbnails and resource media, extracting frame text/embedded document content, running classification pipelines, and outputting combined stage logs.

#### [MODIFY] [moderation.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/api/routes/moderation.py)
- Import `DuplicationHandler` and `HarmfulHandler`.
- Rename dependency helpers to route requests through the new handler layer.
- Update route post methods `stage1`, `stage2`, and `full-pipeline` to invoke the handler orchestrations.

---

## Verification Plan

### Automated Tests
- Build and compile C# BE: `dotnet build` from `CourseMarketplaceBE` directory to verify compiling and type safety.
- Run Python unit checks or FastAPI local tests: `pytest` or basic import tests to confirm syntax.

### Manual Verification
- Deploy and verify AI moderation trigger: Run the C# project and AIModeration service, trigger a moderation pipeline from C#, and check stdout/logging trace to verify full integration.
