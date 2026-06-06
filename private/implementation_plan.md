# Course AI Moderation and Exact Duplication Check Integration Plan

This plan details the steps to move the exact duplication check to the C# Backend, modify the database schema for the AI moderation pipeline, update the FastAPI service, and introduce the AI moderation pipeline for courses.

## User Review Required
> [!IMPORTANT]
> - Dropping columns and tables (e.g., `lessons_exts`, `material_hash`) will result in data loss for those columns. Please confirm this is acceptable in your environment.
> - Renaming `ai_models_courses` to `courses_ai_integrations` will require an EF Core migration.

## Proposed Changes

---

### Database Schema Updates

Modifications to the EF Core Entities and DbContext to support the new AI moderation features.

#### [MODIFY] [CourseAiUsageLog.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/CourseAiUsageLog.cs)
- Drop `log_status` and `ai_model_course_id` columns.
- Add `integration_id` (foreign key to `CourseAiIntegration`).

#### [MODIFY] [MessageModerationLog.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/MessageModerationLog.cs)
- Drop `log_status` column.

#### [MODIFY] [CourseReviewModerationLog.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/CourseReviewModerationLog.cs)
- Drop `log_status` column.

#### [MODIFY] [LessonReviewModerationLog.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/LessonReviewModerationLog.cs)
- Drop `log_status` column.

#### [MODIFY] [CourseExt.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/CourseExt.cs)
- Add `what_you_will_learn_hash` and `requirements_hash`.

#### [DELETE] [LessonExt.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/LessonExt.cs)
- Remove `LessonExt` entity.

#### [MODIFY] [LearningMaterial.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/LearningMaterial.cs)
- Drop `material_hash` column.

#### [NEW] [CourseAiIntegration.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/CourseAiIntegration.cs)
- Rename from the existing `ai_models_courses` entity.

#### [MODIFY] [AppDbContext.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Data/AppDbContext.cs)
- Apply the entity changes: add new entities, rename `ai_models_courses` to `courses_ai_integrations`, remove `lessons_exts`, and update columns.

---

### C# Backend: DTOs & Models

Adding the required DTOs to support exact duplication and AI moderation integration.

#### [NEW] Application\DTOs\ExactDuplicationCommand.cs
- Contains `CourseExt` and `List<CourseExt>`.

#### [NEW] Application\DTOs\ExactDuplicationResult.cs
- Contains `CourseId`, `IsDup`, `DupFields`.

#### [NEW] Application\DTOs\CourseAIIntegrationCommand.cs
- Contains properties for configuring course AI integration.

#### [NEW] Application\DTOs\SemanticDuplicationRequest.cs
- Contains properties for semantic duplication check.

#### [NEW] Application\DTOs\CourseHarmfulRequest.cs
- Contains properties for harmful content check.

#### [NEW] Application\DTOs\SaveCourseHashesCommand.cs
- Contains hashes for title, description, what you will learn, requirements, and thumbnail.

#### [NEW] Application\DTOs\CourseModerationRequest.cs
- Contains `CourseId` and `InstructorId`.

#### [NEW] Application\DTOs\SaveCourseAiUsageLogCommand.cs
- Contains log properties like input/output JSON, tokens, latency, etc.

#### [NEW] Application\DTOs\StageLog.cs
- Stage details and metadata.

#### [NEW] Application\DTOs\CourseAIModerationResult.cs
- Result wrapper for AI Moderation containing `StageLog` list.

---

### C# Backend: Interfaces and Implementations

Updates to core services and repositories.

#### [MODIFY] [IContentHashService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/IServices/IContentHashService.cs) & ContentHashService.cs
- Add `NormalizeText`, `ComputeCourseHashAsync`, `ComputeFileHashAsync`.
- Add `SaveCourseHashesAsync`, `GetCourseHashesAsync`, `GetAllCourseHashesAsync`.

#### [MODIFY] [ICourseService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/IServices/ICourseService.cs) & CourseService.cs
- Update `CreateCourseAsync` to download thumbnails, compute hashes, and save them via `IContentHashService`.
- Add `IntegrateAItoCourseAsync`.

#### [MODIFY] [IModerationService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/IServices/IModerationService.cs) & ModerationService.cs
- Implement `GetExactDuplicationResult` and `CheckExactDuplication`.
- Add `NotifyAdminAsync`.
- Add `PrepareMaterialEmbeddingsAsync`.
- Add `AssignAIModeratorsToCourseAsync`.
- Add `PrepareForCourseAIModeration`.
- Add `ResolveCourseAIModerationResult`.
- Add `LogCourseAiModerationResult`.
- Add `HandleCourseModerationWithAIAsync` (calls `AiModerationService.ModerateCourseFullPipelineAsync`).
- Implement `HandleCourseModerationAsync`.

#### [MODIFY] [IUserRepository.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/IRepositories/IUserRepository.cs) & UserRepository.cs
- Add `GetAdminIdAsync`.

#### [MODIFY] [IAuthService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/IServices/IAuthService.cs) & AuthService.cs
- Add `GetAdminIdAsync` (delegates to repository).

#### [MODIFY] [ILessonService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/IServices/ILessonService.cs) & LessonService.cs
- Add `GetAllMaterialEmbeddingsAsync`.

#### [MODIFY] [IAiModerationService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/IServices/IAiModerationService.cs) & AiModerationService.cs
- Add `GetScoreThresholdConfigAsync`.
- Add `GetModelIdsByType`.
- Add `ModerateCourseFullPipelineAsync`.
- Add `SaveCourseAiUsageLog`.

#### [NEW] Repositories (Domain & Infrastructure)
- `CourseExtRepository`
- `MaterialEmbeddingRepository`
- `SystemConfigRepository`
- `AiModelRepository`
- `CourseAIIntegrationRepository`
- `CourseAiUsageLogRepository`
*(Includes adding both Interfaces and Implementations for each)*

---

### Frontend and Backend Controllers

#### [MODIFY] CourseController.cs (C# Backend)
- Add `ModerateCourse` endpoint that accepts `CourseModerationRequest`.

#### [MODIFY] InstructorCourseController.cs (Frontend)
- Add `ModerateCourse` endpoint that delegates to the C# Backend `ModerateCourse` endpoint.

#### [MODIFY] Editor.cshtml (Frontend)
- Update "Submit for Review" button to call the new `ModerateCourse` endpoint instead of updating course status directly.

---

### FastAPI Changes

#### [MODIFY] FastAPI\core\models.py (or corresponding models file)
- Rename/Modify `DuplicationRequest` -> `SemanticDuplicationRequest`.
- Rename/Modify `ToxicityRequest` -> `CourseHarmfulRequest`.
- Rename `ModerationResponse` -> `CourseModerationResponse`.
- Modify `StageLog` structure to align with the C# `StageLog` DTO.

## Verification Plan

### Automated Tests
- Validate exact duplication logic with unit tests for `ModerationService.GetExactDuplicationResult`.
- Check database migrations successfully apply and columns/tables are created/deleted.

### Manual Verification
- Go to frontend `Editor.cshtml`, fill in course details that are an exact duplicate of an existing course, click "Submit for Review" and verify the exact duplicate error is returned.
- Verify that valid courses trigger the AI moderation full pipeline via FastAPI and save logs in `course_ai_usage_logs`.
