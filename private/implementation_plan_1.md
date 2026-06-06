# AI Course Moderation Pipeline Refinement

This plan outlines the steps to refine the AI-driven course moderation pipeline, focusing on architectural integrity, robust orchestration, and improved data persistence.

## User Review Required

> [!IMPORTANT]
> **Instructor ID Handling**: We are dropping `instructorId` from the frontend `ModerateCourse` call. The backend will now retrieve it directly from the authenticated user's claims to enhance security and simplify the API.

> [!NOTE]
> **CourseExt Renaming**: `CourseExt` will be renamed to `CourseExtDto` to clearly distinguish it as a Data Transfer Object and avoid confusion with Domain Entities.

## Proposed Changes

### [Backend] DTOs & Contracts

#### [MODIFY] [ModerationDTOs.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/ModerationDTOs.cs)
- Rename `CourseExt` class to `CourseExtDto`.
- Ensure `ExactDuplicationCommand` uses `CourseExtDto`.
- Verify all field casings match the expected integration contract (snake_case for hashes, camelCase for status logs).

---

### [Backend] Repositories

#### [MODIFY] [ICourseAiIntegrationRepository.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Domain/IRepositories/ICourseAiIntegrationRepository.cs)
- Add `Task<CourseAiIntegration?> GetByModelAndCourseAsync(int modelId, int courseId);`.

#### [MODIFY] [CourseAiIntegrationRepository.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Repositories/CourseAiIntegrationRepository.cs)
- Implement `GetByModelAndCourseAsync`.

---

### [Backend] Application Services

#### [MODIFY] [ICourseService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/IServices/ICourseService.cs)
- Add `Task<bool> FlagCourseAsync(int courseId, string reason);`.
- Add `Task<bool> RejectCourseDetailedAsync(RejectCourseDetailedRequest request);`.

#### [MODIFY] [CourseService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/Services/CourseService.cs)
- Implement `FlagCourseAsync` and `RejectCourseDetailedAsync` (migrating logic from `ModerationService`).
- Ensure these methods handle status transitions, feedback aggregation, and automatic archiving after multiple flags.

#### [MODIFY] [ModerationService.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Application/Services/ModerationService.cs)
- **`HandleCourseModerationAsync`**: 
    - Update exact duplication rejection message to include specific `DupFields`.
- **`PrepareMaterialEmbeddingsAsync`**: 
    - Implement Redis-first check. If cache exists for the course, skip re-loading/re-caching.
- **`AssignAIModeratorsToCourseAsync`**: 
    - Change model type query to "moderator".
- **`ResolveCourseAIModerationResult`**:
    - Replace direct repository calls with `_courseService.GetCourseWithDetailsAsync`.
    - Retrieve `material_ids` from the course details.
    - Fetch generated embeddings from Redis (mapped by FastAPI) and persist them using `IMaterialEmbeddingRepository`.
    - Map `flaggedFields` to `RejectCourseItemDto` for granular feedback.
    - Remove redundant manual notification at the end (as `CourseService` methods will handle it).
- **`LogCourseAiModerationResult`**:
    - Set `interaction_type = "moderation"`.
    - Distill `input_json` (requests) and `output_json` (pipeline logs) for audit trails.
- **`HandleCourseModerationWithAIAsync`**:
    - Fix `linkAction` for admin notifications.
    - Refine `SemanticDuplicationRequest` and `CourseHarmfulRequest` preparation logic.

---

### [Backend] Presentation Layer

#### [MODIFY] [CourseController.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceBE/CourseMarketplaceBE/Presentation/Controllers/CourseController.cs)
- Update `ModerateCourse` to retrieve `instructorId` from `User` claims.
- Remove `InstructorId` from the input DTO requirement (or ignore it if present).

---

### [Frontend] Controller & Views

#### [MODIFY] [InstructorCourseController.cs](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceFE/CourseMarketplaceFE/Controllers/InstructorCourseController.cs)
- Update `ModerateCourse` (FE) to stop sending `instructorId` in the request body.

#### [MODIFY] [Editor.cshtml](file:///c:/Users/anhkc/Desktop/Linked/CourseMarketplaceFE/CourseMarketplaceFE/Views/InstructorCourse/Editor.cshtml)
- Update `submitCourseForReview` JS function to remove the `instructorId` parameter from the AJAX call.

## Verification Plan

### Automated Tests
- Run unit tests for `ModerationService` to verify duplication logic and status transitions.
- Verify Redis cache keys for material embeddings before and after moderation.

### Manual Verification
1. **Trigger Moderation**: Ensure the process starts from the UI without needing manual instructor ID passing.
2. **Duplication Feedback**: Verify that if a course is an exact duplicate, the instructor receives a clear message about which fields are identical.
3. **AI Rejection/Flagging**: Verify that flagged fields from AI result in detailed rejection items in the course editor.
4. **Embedding Persistence**: Check the `material_embeddings` table after a successful AI moderation to ensure new embeddings were saved from Redis.
