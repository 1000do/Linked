# Implementation Plan - Separate Material Embeddings into Text and Media Tables (Revised)

We are splitting the `material_embeddings` table into two separate tables to support different vector dimensions:
- `text_embeddings` for DistilBERT text embeddings (768 dimensions)
- `media_embeddings` for CLIP image/video embeddings (512 dimensions)

This resolves the Postgres exception (`expected 768 dimensions, not 512`) when inserting CLIP image/video embeddings into the 768-dimensional table.

## User Review Required

> [!IMPORTANT]
> - **Clean Architecture Constraint:** The C# Domain Entities (`TextEmbedding` and `MediaEmbedding`) will continue to use `List<float>?` for embedding properties. Bypassing pgvector types in the Domain layer is achieved by using EF Core Value Converters to `Pgvector.Vector` in the Infrastructure layer (`AppDbContext.cs`).
> - **Consistent DTO Types & Redis Cache Schema:** We will change `MaterialEmbeddingResponse.Embedding` in C# from `string?` to `List<float>?`. This ensures that Redis cache values are consistently structured as JSON objects containing `{material_id, embedding, embedding_type}`, avoiding raw float arrays in Redis and simplifying the serialization/deserialization logic on both the Python and C# sides.
> - **FastAPI Resolution:** FastAPI's orchestration will determine `embedding_type` based on file extensions. This type will be passed to `set_material_embedding` and cached in Redis.
> - **C# Persistence Resolution:** The C# backend will retrieve `MaterialEmbeddingResponse` DTOs from Redis. It will use `dto.EmbeddingType` to choose the correct table for saving new embeddings, only falling back to vector length inspection if `EmbeddingType` is missing or invalid.

## Open Questions

None.

## Proposed Changes

### Database Migration

#### [NEW] SQL Script
Execute SQL commands to drop the old table and create two new ones:
```sql
DROP TABLE IF EXISTS material_embeddings CASCADE;

CREATE TABLE text_embeddings (
    text_embedding_id SERIAL PRIMARY KEY,
    material_id INT REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    text_embedding vector(768),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE media_embeddings (
    media_embedding_id SERIAL PRIMARY KEY,
    material_id INT REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    media_embedding vector(512),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

---

### Core / Domain (C# Backend)

#### [DELETE] [MaterialEmbedding.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/MaterialEmbedding.cs)
Remove the obsolete single-table entity.

#### [NEW] [TextEmbedding.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/TextEmbedding.cs)
Create the entity matching `text_embeddings` table using `List<float>?` for the `Embedding` property.

#### [NEW] [MediaEmbedding.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/MediaEmbedding.cs)
Create the entity matching `media_embeddings` table using `List<float>?` for the `Embedding` property.

#### [MODIFY] [LearningMaterial.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/LearningMaterial.cs)
Replace navigation property `ICollection<MaterialEmbedding> MaterialEmbeddings` with:
- `ICollection<TextEmbedding> TextEmbeddings`
- `ICollection<MediaEmbedding> MediaEmbeddings`

---

### Infrastructure / DB & Repositories (C# Backend)

#### [MODIFY] [AppDbContext.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Data/AppDbContext.cs)
- Replace `DbSet<MaterialEmbedding>` with `DbSet<TextEmbedding>` and `DbSet<MediaEmbedding>`.
- Replace the Fluent API configuration for `MaterialEmbedding` with configurations for `TextEmbedding` (vector 768) and `MediaEmbedding` (vector 512).

#### [DELETE] [IMaterialEmbeddingRepository.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/IRepositories/IMaterialEmbeddingRepository.cs)
Remove the obsolete interface.

#### [DELETE] [MaterialEmbeddingRepository.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Repositories/MaterialEmbeddingRepository.cs)
Remove the obsolete repository implementation.

#### [NEW] [ITextEmbeddingRepository.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/IRepositories/ITextEmbeddingRepository.cs)
Define the repository interface for `TextEmbedding`.

#### [NEW] [TextEmbeddingRepository.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Repositories/TextEmbeddingRepository.cs)
Implement database operations for `TextEmbedding`.

#### [NEW] [IMediaEmbeddingRepository.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/IRepositories/IMediaEmbeddingRepository.cs)
Define the repository interface for `MediaEmbedding`.

#### [NEW] [MediaEmbeddingRepository.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Repositories/MediaEmbeddingRepository.cs)
Implement database operations for `MediaEmbedding`.

#### [MODIFY] [Program.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Program.cs)
Remove `IMaterialEmbeddingRepository` DI registration. Register `ITextEmbeddingRepository` and `IMediaEmbeddingRepository`.

---

### Application Layer & Services (C# Backend)

#### [MODIFY] [ModerationDTOs.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/ModerationDTOs.cs)
Change `Embedding` property type in `MaterialEmbeddingResponse` from `string?` to `List<float>?`. Add `EmbeddingType` string property.

#### [MODIFY] [ILessonService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/IServices/ILessonService.cs) & [LessonService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/Services/LessonService.cs)
- Update `GetAllMaterialEmbeddingsAsync` to query both text and media embedding repositories, map to `MaterialEmbeddingResponse` directly (without converting to string), and set the `EmbeddingType` property ("text" or "media").
- Update `SaveMaterialEmbeddingsAsync` to accept `embeddingType` and write to the correct repository (Text or Media).

#### [MODIFY] [ModerationService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/Services/ModerationService.cs)
- In `ResolveCourseAIModerationResult`, retrieve the cache as `MaterialEmbeddingResponse` DTO. If the DTO contains the embedding list, determine the database table based on `dto.EmbeddingType`. Fall back to checking the embedding list size (`Count`) only if `dto.EmbeddingType` is null or invalid.

---

### FastAPI AI Moderation Service (Python)

#### [MODIFY] [models.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/core/models.py)
Update `MaterialEmbeddingResponse` to include `embedding_type: Optional[str] = Field(None, alias="embeddingType")`.

#### [MODIFY] [cache_repository.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/repositories/cache_repository.py)
Update `set_material_embedding` to accept `embedding_type: str`. Store the cached value as a JSON object containing `material_id`, `embedding`, and `embedding_type`.

#### [MODIFY] [redis_service.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/services/redis_service.py)
- Update `set_material_embedding` to pass `embedding_type` to cache repository.
- Update `get_all_existing_embeddings` to parse `embedding_type` from the cached DTO JSON object, or fall back to checking vector dimensions if missing or invalid.

#### [MODIFY] [duplication_handler.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/handlers/duplication_handler.py)
- Resolve `embedding_type` using the material loop/file extension mapping. Pass it to the cached `MaterialEmbeddingResponse`.
- Use this `embedding_type` when calling `set_material_embedding`.

#### [MODIFY] [duplication_service.py](file:///c:/Users/anhkc/Desktop/Linked/AIModeration/services/duplication_service.py)
In `similarity_search`, only compare embeddings if they have matching `embedding_type` to avoid shape mismatch exceptions during calculation.

---

## Verification Plan

### Automated Tests
1. Compile backend using `dotnet build`.
2. Trigger moderation checking endpoint. Verify that:
   - Data is correctly cached in Redis.
   - Moderation runs to completion.
   - Text embeddings are written to `text_embeddings`.
   - Media embeddings are written to `media_embeddings`.

### Manual Verification
1. Inspect database tables `text_embeddings` and `media_embeddings` directly via SQL to verify saved vector values.
