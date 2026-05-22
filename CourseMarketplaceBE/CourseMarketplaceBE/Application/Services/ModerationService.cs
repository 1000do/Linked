using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Application.Services
{
    public class ModerationService : IModerationService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IChatRepository _chatRepository;
        private readonly INotificationService _notificationService;
        private readonly IMaterialRepository _materialRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICheckoutRepository _checkoutRepository;
        private readonly IRedisService _redisService;
        private readonly IAiModerationService _aiModerationService;
        private readonly IContentHashService _contentHashService;
        private readonly ICourseAiIntegrationRepository _aiIntegrationRepository;
        private readonly IMaterialEmbeddingRepository _embeddingRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICourseService _courseService;
        private readonly ILessonService _lessonService;
        private readonly ILogger<ModerationService> _logger;


        public ModerationService(
            ICourseRepository courseRepository,

            IChatRepository chatRepository,

            INotificationService notificationService,
            IMaterialRepository materialRepository,
            ILessonRepository lessonRepository,
            ICheckoutRepository checkoutRepository,
            IRedisService redisService,
            IAiModerationService aiModerationService,
            IContentHashService contentHashService,
            ICourseAiIntegrationRepository aiIntegrationRepository,
            IMaterialEmbeddingRepository embeddingRepository,
            IUserRepository userRepository,
            ILessonService lessonService,
            ICourseService courseService,
            ILogger<ModerationService> logger)
        {
            _courseRepository = courseRepository;
            _chatRepository = chatRepository;
            _notificationService = notificationService;
            _materialRepository = materialRepository;
            _lessonRepository = lessonRepository;
            _checkoutRepository = checkoutRepository;
            _redisService = redisService;
            _aiModerationService = aiModerationService;
            _contentHashService = contentHashService;
            _aiIntegrationRepository = aiIntegrationRepository;
            _embeddingRepository = embeddingRepository;
            _userRepository = userRepository;
            _lessonService = lessonService;
            _courseService = courseService;
            _logger = logger;
        }

        public async Task<List<CourseModerationDto>> GetPendingCoursesAsync(ModerationFilterDto filter)
        {
            return await _courseRepository.GetPendingCoursesModerationAsync(filter);
        }

        public async Task<bool> ApproveCourseAsync(int courseId, string? feedback)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return false;

            course.CourseStatus = "published";
            course.ModerationFeedback = null; // Clear old course feedback
            course.UpdatedAt = DateTime.Now;
            course.LastApprovedAt = DateTime.Now;

            // Clear moderation feedback for all materials in this course
            var materials = await _materialRepository.GetByCourseIdAsync(courseId);
            if (materials != null && materials.Any())
            {
                foreach (var material in materials)
                {
                    material.ModerationFeedback = null;
                    material.LearningStatus = "active"; // Ensure materials are active
                    _materialRepository.Update(material);
                }
            }

            // ALSO: Clear status for all lessons in this course
            var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
            if (lessons != null && lessons.Any())
            {
                foreach (var lesson in lessons)
                {
                    lesson.LessonStatus = "active";
                    _lessonRepository.Update(lesson);
                }
            }

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();

            if (course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Course Approved",
                    $"Your course '{course.Title}' has been approved and is now published in the store. {feedback}",
                    $"/Course/Details/{courseId}"
                );

                // NEW: Thông báo cho học viên đã tham gia nếu khóa học có cập nhật
                var enrolledUserIds = await _checkoutRepository.GetEnrolledUserIdsAsync(courseId);
                if (enrolledUserIds.Any())
                {
                    foreach (var studentId in enrolledUserIds)
                    {
                        await _notificationService.SendNotificationAsync(
                            studentId,
                            "Course Content Updated",
                            $"The course '{course.Title}' you enrolled in has been updated with new content. Check it out now!",
                            $"/Course/Learn?id={courseId}"
                        );
                    }
                }
            }

            // Invalidate Cache
            await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));

            return true;
        }

        public async Task<bool> RejectCourseAsync(int courseId, string reason)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return false;

            course.CourseStatus = CourseStatus.Rejected.ToValue();
            course.ModerationFeedback = reason;
            course.UpdatedAt = DateTime.Now;

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();

            if (course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Course Rejected",
                    $"Your course '{course.Title}' was not approved. Reason: {reason}",
                    $"/InstructorCourse/Editor?id={courseId}"
                );
            }

            // Invalidate Cache
            await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));

            return true;
        }

        public async Task<bool> FlagCourseAsync(int courseId, string reason)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return false;

            var currentFlags = (course.CourseFlagCount ?? 0) + 1;
            course.CourseFlagCount = currentFlags;
            course.CourseStatus = CourseStatus.Flagged.ToValue();

            course.ModerationFeedback = $"[VIOLATION FLAG #{currentFlags}] {reason}";
            course.UpdatedAt = DateTime.Now;

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();

            if (course.InstructorId.HasValue)
            {
                string subject = "Course Violation Warning";
                string message = "";

                if (currentFlags == 1)
                {
                    subject = "Course Violation Reminder (1st Time)";
                    message = $"Your course '{course.Title}' has been flagged for a violation and temporarily hidden. Reason: {reason}. Please review the content and ensure compliance.";
                }
                else if (currentFlags == 2)
                {
                    subject = "Severe Violation Warning (2nd Time)";
                    message = $"Your course '{course.Title}' continues to violate policies (2nd time). This is a strong warning. If the violation continues, the course will be permanently deleted.";
                }
                else
                {
                    subject = "Permanent Course Discontinuation Notice (3rd Time)";
                    message = $"Your course '{course.Title}' has violated policies for the 3rd time. The platform has decided to permanently discontinue this course. You will not be able to edit the content or accept new students, but existing students can still access their purchased content.";
                    
                    course.CourseStatus = "archived";
                    _courseRepository.Update(course);
                    await _courseRepository.SaveChangesAsync();
                }

                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    subject,
                    message,
                    $"/InstructorCourse/Editor?id={courseId}"
                );
            }

            // Invalidate Cache
            await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));

            return true;
        }

        public async Task<bool> RejectCourseDetailedAsync(RejectCourseDetailedRequest request)
        {
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null) return false;

            var courseFeedbackParts = new List<string>();
            var rejectedLessonIds = new HashSet<int>();

            // FIRST: Clear ALL old feedback to ensure only new feedback persists
            course.ModerationFeedback = null;
            var allMaterials = await _materialRepository.GetByCourseIdAsync(request.CourseId);
            if (allMaterials != null)
            {
                foreach (var m in allMaterials)
                {
                    m.ModerationFeedback = null;
                    m.LearningStatus = "active"; // Reset to active before applying new rejections
                    _materialRepository.Update(m);
                }
            }

            // Also reset all lessons of this course to active before applying rejections
            var lessons = await _lessonRepository.GetByCourseIdAsync(request.CourseId);
            if (lessons != null)
            {
                foreach (var l in lessons)
                {
                    l.LessonStatus = "active";
                    _lessonRepository.Update(l);
                }
            }

            foreach (var item in request.Items)
            {
                if (item.Target == "file" && item.MaterialId.HasValue)
                {
                    // Update material learning_status = "rejected" and moderation_feedback
                    var material = await _materialRepository.GetByIdAsync(item.MaterialId.Value);
                    if (material != null)
                    {
                        material.LearningStatus = "rejected";
                        material.ModerationFeedback = item.Reason;
                        material.UpdatedAt = DateTime.Now;
                        _materialRepository.Update(material);

                        // Mark the parent lesson as rejected
                        if (material.LessonId.HasValue)
                        {
                            rejectedLessonIds.Add(material.LessonId.Value);
                        }
                    }
                }
                else if (item.Target == "lesson.title" && item.LessonId.HasValue)
                {
                    // Mark lesson as rejected
                    rejectedLessonIds.Add(item.LessonId.Value);
                    courseFeedbackParts.Add($"[Lesson: {item.LessonTitle ?? $"#{item.LessonId}"}] {item.Reason}");
                }
                else if (item.Target.StartsWith("course."))
                {
                    // Aggregate course-level feedback
                    var label = item.Target switch
                    {
                        "course.title" => "Title",
                        "course.description" => "Description",
                        "course.thumbnail" => "Thumbnail",
                        "course.what_you_will_learn" => "What You Will Learn",
                        "course.requirements" => "Requirements",
                        _ => item.Target
                    };
                    courseFeedbackParts.Add($"[@{label}] {item.Reason}");
                }
            }

            // Update rejected lessons
            foreach (var lessonId in rejectedLessonIds)
            {
                var lesson = await _lessonRepository.GetByIdAsync(lessonId);
                if (lesson != null)
                {
                    lesson.LessonStatus = "rejected";
                    lesson.UpdatedAt = DateTime.Now;
                    _lessonRepository.Update(lesson);
                }
            }

            // Update course status
            course.CourseStatus = CourseStatus.Rejected.ToValue();
            course.ModerationFeedback = courseFeedbackParts.Count > 0
                ? string.Join("\n", courseFeedbackParts)
                : "Course rejected. Please check flagged files.";
            course.UpdatedAt = DateTime.Now;

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();

            if (course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Course Rejected",
                    $"Your course '{course.Title}' was not approved. Please check details in the course editor.",
                    $"/InstructorCourse/Editor?id={request.CourseId}"
                );
            }

            // Invalidate Cache
            await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(request.CourseId));

            return true;
        }

        public async Task<List<UserReportModerationDto>> GetAllReportsAsync()
        {
            var reports = await _chatRepository.GetAllReportsAsync();
            return reports.Select(r => new UserReportModerationDto
            {
                ReportId = r.ReportId,
                ReporterName = r.Reporter?.Email ?? "Unknown",
                Reason = r.Reason,
                Description = r.Description,
                ChatId = r.ChatId,
                Status = r.UserReportsStatus,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<bool> ResolveReportAsync(ResolveReportDto dto)
        {
            var report = await _chatRepository.GetReportByIdAsync(dto.ReportId);
            if (report == null) return false;

            report.UserReportsStatus = dto.Status;
            report.ResolutionNote = dto.ResolutionNote;
            report.ResolvedAt = DateTime.Now;


            await _chatRepository.SaveChangesAsync();
            return true;
        }

        // ===== NEW AI MODERATION IMPLEMENTATION =====

        public async Task<ExactDuplicationResult> GetExactDuplicationResult(ExactDuplicationCommand command)
        {
            var res = new ExactDuplicationResult { CourseId = command.CourseExt.CourseId, IsDup = false };
            foreach (var ext in command.ExistingCourseExts)
            {
                if (ext.CourseId == command.CourseExt.CourseId) continue;
                if (ext.title_hash == command.CourseExt.title_hash) res.DupFields.Add("title");
                if (ext.description_hash == command.CourseExt.description_hash) res.DupFields.Add("description");
                if (ext.what_you_will_learn_hash == command.CourseExt.what_you_will_learn_hash) res.DupFields.Add("what_you_will_learn");
                if (ext.requirements_hash == command.CourseExt.requirements_hash) res.DupFields.Add("requirements");
                if (ext.thumbnail_hash == command.CourseExt.thumbnail_hash && !string.IsNullOrEmpty(ext.thumbnail_hash)) res.DupFields.Add("thumbnail");
            }
            if (res.DupFields.Any()) { res.IsDup = true; res.DupFields = res.DupFields.Distinct().ToList(); }
            return res;
        }

        public async Task<ExactDuplicationResult> CheckExactDuplication(int courseId)
        {
            var current = await _contentHashService.GetCourseHashesAsync(courseId);
            var others = await _contentHashService.GetAllCourseHashesAsync();
            return await GetExactDuplicationResult(new ExactDuplicationCommand { CourseExt = current, ExistingCourseExts = others });
        }

        public async Task NotifyAdminAsync(string title, string content, string? linkAction)
        {
            var adminId = await _userRepository.GetAdminIdAsync();
            if (adminId.HasValue) await _notificationService.SendNotificationAsync(adminId.Value, title, content, linkAction);
        }

        public async Task PrepareMaterialEmbeddingsAsync()
        {
            var isInitialized = await _redisService.GetCacheAsync<bool>(CacheKeys.MaterialEmbeddingInitialized.GetKey());
            if (isInitialized) return;

            var allEmbeddings = await _lessonService.GetAllMaterialEmbeddingsAsync();
            foreach (var e in allEmbeddings)
            {
                string cacheKey = CacheKeys.MaterialEmbedding.GetKey(e.EmbeddingId);
                var cached = await _redisService.GetCacheAsync<MaterialEmbedding>(cacheKey);
                if (cached == null)
                {
                    await _redisService.SetCacheAsync(cacheKey, e, CacheTtl.Medium.GetTtl());
                }
            }

            await _redisService.SetCacheAsync(CacheKeys.MaterialEmbeddingInitialized.GetKey(), true, CacheTtl.Medium.GetTtl());
        }

        public async Task<AssignAIModeratorsToCourseResult> AssignAIModeratorsToCourseAsync(int courseId)
        {
            var thresholds = await _aiModerationService.GetScoreThresholdConfigAsync(SystemConfigKeys.ModerationThreshold);
            var classifiers = await _aiModerationService.GetModelsByTypeAsync(AiModelConst.Classifier);
            var generators = await _aiModerationService.GetModelsByTypeAsync(AiModelConst.Generator);
            var modelIds = classifiers.Concat(generators).Select(m => m.model_id).ToList();



            foreach (var mid in modelIds)
            {
                var existing = await _courseService.GetByModelAndCourseAsync(mid, courseId);
                if (existing == null)
                {
                    await _courseService.IntegrateAItoCourseAsync(new CourseAIIntegrationCommand
                    {
                        CourseId = courseId,
                        ModelId = mid,
                        Role = AiModelConst.Moderator,
                        IsEnabled = true,
                        ConfigJson = thresholds
                    });
                }
            }
            return new AssignAIModeratorsToCourseResult
            {
                CourseId = courseId,
                ModelIds = modelIds,
                Thresholds = thresholds
            };
        }

        public async Task<PrepareForCourseAIModerationResult> PrepareForCourseAIModeration(int courseId)
        {
            try
            {
                var assignmentResult = await AssignAIModeratorsToCourseAsync(courseId);
                var course = await _courseService.GetCourseWithDetailsAsync(courseId, 0); // Cache course
                var materialIds = course?.Lessons?
                    .SelectMany(lesson => lesson.LearningMaterials?.Select(material => material.MaterialId) ?? [])
                    .ToList() ?? [];


                await PrepareMaterialEmbeddingsAsync();
                return new PrepareForCourseAIModerationResult
                {
                    CourseId = courseId,
                    MaterialIds = materialIds,
                    ModelIds = assignmentResult.ModelIds,
                    Thresholds = assignmentResult.Thresholds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to prepare for AI moderation");
                throw;
            }
        }

        public async Task ResolveCourseAIModerationResult(CourseModerationResult result)
        {
            var courseId = result.CourseId;
            var course = await _courseService.GetCourseWithDetailsAsync(courseId, 0);
            if (course == null) return;

            // Save material embeddings from Redis cache to database
            var materialIds = course.Lessons?
                .SelectMany(l => l.LearningMaterials?.Select(m => m.MaterialId) ?? [])
                .ToList() ?? [];

            foreach (var matId in materialIds)
            {
                string cacheKey = CacheKeys.MaterialEmbedding.GetKey(matId);
                var cachedEmbedding = await _redisService.GetCacheAsync<List<float>>(cacheKey);
                if (cachedEmbedding != null && cachedEmbedding.Any())
                {
                    await _lessonService.SaveMaterialEmbeddingsAsync(matId, cachedEmbedding);
                }
            }

            if (result.ModerationStatus == ModerationStatus.Approved.ToValue())
            {
                await ApproveCourseAsync(courseId, "AI moderation passed.");
            }
            else if (result.ModerationStatus == ModerationStatus.Rejected.ToValue())
            {
                var items = new List<RejectCourseItemDto>();
                foreach (var log in result.stageLogs)
                {
                    if (log.result == StageLogResult.Flagged.ToValue() || log.result == StageLogResult.MatchFound.ToValue())
                    {
                        foreach (var field in log.flaggedFields)
                        {
                            items.Add(new RejectCourseItemDto
                            {
                                Target = field,
                                Reason = log.reason ?? "AI moderation flag"
                            });
                        }
                    }
                }

                if (!items.Any())
                {
                    items.Add(new RejectCourseItemDto
                    {
                        Target = "course.description",
                        Reason = "Course content violates the moderation policy."
                    });
                }

                await RejectCourseDetailedAsync(new RejectCourseDetailedRequest
                {
                    CourseId = courseId,
                    Items = items
                });
            }
            else if (result.ModerationStatus == ModerationStatus.Flagged.ToValue())
            {
                var items = new List<RejectCourseItemDto>();
                foreach (var log in result.stageLogs)
                {
                    if (log.result == StageLogResult.Flagged.ToValue() || log.result == StageLogResult.MatchFound.ToValue())
                    {
                        foreach (var field in log.flaggedFields)
                        {
                            items.Add(new RejectCourseItemDto
                            {
                                Target = field,
                                Reason = log.reason ?? "AI moderation flag"
                            });
                        }
                    }
                }

                if (!items.Any())
                {
                    items.Add(new RejectCourseItemDto
                    {
                        Target = "course.description",
                        Reason = "Course content violates the moderation policy."
                    });
                }

                await FlagCourseDetailedAsync(new RejectCourseDetailedRequest
                {
                    CourseId = courseId,
                    Items = items
                });
            }
            else if (result.ModerationStatus == ModerationStatus.ManualAudit.ToValue())
            {
                await _courseService.UpdateCourseStatusAndFeedbackAsync(courseId, CourseStatus.Pending.ToValue(), "AI suggested manual audit.");
                await NotifyAdminAsync("Manual Audit Required", $"Course {courseId} requires manual review by AI.", $"/Admin/Moderation/Courses?id={courseId}");
            }
        }

        public async Task<bool> FlagCourseDetailedAsync(RejectCourseDetailedRequest request)
        {
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null) return false;

            var courseFeedbackParts = new List<string>();
            var flaggedLessonIds = new HashSet<int>();

            // FIRST: Clear ALL old feedback to ensure only new feedback persists
            course.ModerationFeedback = null;
            var allMaterials = await _materialRepository.GetByCourseIdAsync(request.CourseId);
            if (allMaterials != null)
            {
                foreach (var m in allMaterials)
                {
                    m.ModerationFeedback = null;
                    m.LearningStatus = "active"; // Reset to active before applying new flags
                    _materialRepository.Update(m);
                }
            }

            // Also reset all lessons of this course to active before applying rejections
            var lessons = await _lessonRepository.GetByCourseIdAsync(request.CourseId);
            if (lessons != null)
            {
                foreach (var l in lessons)
                {
                    l.LessonStatus = "active";
                    _lessonRepository.Update(l);
                }
            }

            foreach (var item in request.Items)
            {
                if (item.Target == "file" && item.MaterialId.HasValue)
                {
                    var material = await _materialRepository.GetByIdAsync(item.MaterialId.Value);
                    if (material != null)
                    {
                        material.LearningStatus = "flagged";
                        material.ModerationFeedback = item.Reason;
                        material.UpdatedAt = DateTime.Now;
                        _materialRepository.Update(material);

                        if (material.LessonId.HasValue)
                        {
                            flaggedLessonIds.Add(material.LessonId.Value);
                        }
                    }
                }
                else if (item.Target == "lesson.title" && item.LessonId.HasValue)
                {
                    flaggedLessonIds.Add(item.LessonId.Value);
                    courseFeedbackParts.Add($"[Lesson: {item.LessonTitle ?? $"#{item.LessonId}"}] {item.Reason}");
                }
                else if (item.Target.StartsWith("course."))
                {
                    var label = item.Target switch
                    {
                        "course.title" => "Title",
                        "course.description" => "Description",
                        "course.thumbnail" => "Thumbnail",
                        "course.what_you_will_learn" => "What you will learn",
                        "course.requirements" => "Requirements",
                        _ => item.Target
                    };
                    courseFeedbackParts.Add($"[@{label}] {item.Reason}");
                }
            }

            foreach (var lessonId in flaggedLessonIds)
            {
                var lesson = await _lessonRepository.GetByIdAsync(lessonId);
                if (lesson != null)
                {
                    lesson.LessonStatus = "rejected";
                    lesson.UpdatedAt = DateTime.Now;
                    _lessonRepository.Update(lesson);
                }
            }

            var currentFlags = (course.CourseFlagCount ?? 0) + 1;
            course.CourseFlagCount = currentFlags;
            course.CourseStatus = CourseStatus.Flagged.ToValue();

            var detailedReason = courseFeedbackParts.Count > 0
                ? string.Join("\n", courseFeedbackParts)
                : "Content flagged for policy violations.";

            course.ModerationFeedback = $"[VIOLATION FLAG #{currentFlags}] {detailedReason}";
            course.UpdatedAt = DateTime.Now;

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();

            {
                string subject = "Course Violation Warning";
                string message = "";
 
                if (currentFlags == 1)
                {
                    subject = "Course Violation Reminder (1st Time)";
                    message = $"Your course '{course.Title}' has been flagged for violation and temporarily hidden. Details:\n{detailedReason}\nPlease review the content and comply with regulations.";
                }
                else if (currentFlags == 2)
                {
                    subject = "Severe Violation Warning (2nd Time)";
                    message = $"Your course '{course.Title}' continues to violate policies (2nd time). Details:\n{detailedReason}\nThis is a strong warning. If the violation continues, the course will be permanently deleted.";
                }
                else
                {
                    subject = "Permanent Course Discontinuation Notice (3rd Time)";
                    message = $"Your course '{course.Title}' has violated our policy for the 3rd time. Details:\n{detailedReason}\nThe system has decided to permanently discontinue this course. You will not be able to edit content or enroll new students, but existing students can still access their purchased content.";


                    course.CourseStatus = CourseStatus.Archived.ToValue();
                    _courseRepository.Update(course);
                    await _courseRepository.SaveChangesAsync();
                }

                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    subject,
                    message,
                    $"/InstructorCourse/Editor?id={request.CourseId}"
                );
            }

            await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(request.CourseId));
            return true;
        }

        public async Task LogCourseAiModeration(LogCourseAiModerationCommand command)
        {
            _logger.LogInformation("Logging course AI moderation for course {CourseId}", command.CourseModerationResult.CourseId);
            var result = command.CourseModerationResult;
            foreach (var stage in result.stageLogs)
            {
                _logger.LogInformation("Logging course AI moderation for course {CourseId} and stage {Stage}", command.CourseModerationResult.CourseId, stage.stage);
                var integration = await _aiIntegrationRepository.GetByModelAndCourseAsync(stage.model_id, result.CourseId);
                if (integration == null)
                {
                    _logger.LogWarning("Integration not found for course {CourseId} and model {ModelId}", result.CourseId, stage.model_id);
                    continue;
                }

                var semRq = command.SemanticDuplicationRequest;
                var harmRq = command.CourseHarmfulRequest;

                _logger.LogInformation("Saving Log for course AI moderation for course {CourseId} and integration {IntegrationId}", command.CourseModerationResult.CourseId, integration.Id);
                var inputJson = JsonSerializer.Serialize(new CourseAiUsageLogInput
                {
                    CourseId = result.CourseId,
                    MaterialIds = semRq.MaterialIds,
                    SimilarityScoreThreshold = semRq.similarityScoreThreshold,
                    SpamScoreThreshold = harmRq.spamScoreThreshold,
                    ToxicScoreThreshold = harmRq.toxicScoreThreshold
                });
                _logger.LogInformation("Input JSON for course AI moderation for course {CourseId} and integration {IntegrationId}: {InputJson}", command.CourseModerationResult.CourseId, integration.Id, inputJson);
                var outputJson = JsonSerializer.Serialize(new CourseAiUsageLogOutput
                {
                    Stage = stage.stage,
                    Step = stage.step,
                    Timestamp = stage.timestamp,
                    Result = stage.result,
                    Reason = stage.reason,
                    FlaggedFields = stage.flaggedFields,
                    Details = stage.details,
                    ConfidenceScore = stage.confidence_score

                });
                _logger.LogInformation("Output JSON for course AI moderation for course {CourseId} and integration {IntegrationId}: {OutputJson}", command.CourseModerationResult.CourseId, integration.Id, outputJson);
                await _aiModerationService.SaveCourseAiUsageLog(new SaveCourseAiUsageLogCommand
                {
                    integration_id = integration.Id,
                    interaction_type = command.InteractionType,
                    input_json = inputJson,
                    output_json = outputJson,
                    latency_ms = stage.latency_ms,
                    token_usage = 0,
                    error_message = command.ErrorMessage
                });
                _logger.LogInformation("Saved Log for course AI moderation for course {CourseId} and integration {IntegrationId}", command.CourseModerationResult.CourseId, integration.Id);
            }
        }

        public async Task<CourseModerationResult> HandleCourseModerationWithAIAsync(CouresModerationRequest request)
        {
            try
            {
                var isHealthy = await _aiModerationService.HealthCheckAsync();
                if (!isHealthy)
                {
                    await NotifyAdminAsync("AI Service Unhealthy", $"Course {request.CourseId} requires manual review due to AI service being unhealthy.", "/AdminModeration/Courses");
                    return new CourseModerationResult { CourseId = request.CourseId, ModerationStatus = ModerationStatus.ManualAudit.ToValue() };
                }

                var prep = await PrepareForCourseAIModeration(request.CourseId);

                var thresholds = prep.Thresholds;
                var materialIds = prep.MaterialIds;


                var semanticReq = new SemanticDuplicationRequest
                {
                    CourseId = request.CourseId,
                    MaterialIds = materialIds,
                    similarityScoreThreshold = thresholds.GetValueOrDefault("similarity", 0.8f)
                };

                var harmfulReq = new CourseHarmfulRequest
                {
                    CourseId = request.CourseId,
                    spamScoreThreshold = thresholds.GetValueOrDefault("spam", 0.7f),
                    toxicScoreThreshold = thresholds.GetValueOrDefault("toxic", 0.7f)
                };

                var result = await _aiModerationService.ModerateCourseFullPipelineAsync(semanticReq, harmfulReq);


                if (result.ModerationStatus == ModerationStatus.ManualAudit.ToValue())
                {
                    await NotifyAdminAsync("Manual Audit Required", $"Course {request.CourseId} flagged for manual review by AI.", "/AdminModeration/Courses");
                }
                else
                {
                    await ResolveCourseAIModerationResult(result);
                }

                await LogCourseAiModeration(new LogCourseAiModerationCommand
                {
                    SemanticDuplicationRequest = semanticReq,
                    CourseHarmfulRequest = harmfulReq,
                    CourseModerationResult = result,
                    InteractionType = "moderation",
                    ErrorMessage = null
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AI moderation for course {CourseId}", request.CourseId);
                await NotifyAdminAsync("Moderation Process Exception", $"Exception during AI moderation for course {request.CourseId}: {ex.Message}", "/AdminModeration/Courses");
                throw;
            }
        }

        public async Task<CourseModerationResult> HandleCourseModerationAsync(CouresModerationRequest request)
        {
            try
            {
                await _courseService.UpdateCourseStatusAsync(request.CourseId, CourseStatus.Pending.ToValue(), request.InstructorId);


                var dupResult = await CheckExactDuplication(request.CourseId);
                if (dupResult.IsDup)
                {
                    var items = dupResult.DupFields.Select(f => new RejectCourseItemDto
                    {
                        Target = $"course.{f}",
                        Reason = "Exact duplication with an existing course found."
                    }).ToList();

                    await RejectCourseDetailedAsync(new RejectCourseDetailedRequest
                    {
                        CourseId = request.CourseId,
                        Items = items
                    });


                    return new CourseModerationResult
                    {
                        CourseId = request.CourseId,
                        ModerationStatus = ModerationStatus.Rejected.ToValue(),
                        flaggedFields = dupResult.DupFields,
                        overall_confidence_score = 1.0f,
                        total_latency_ms = 0,
                        stageLogs = []
                    };
                }

                return await HandleCourseModerationWithAIAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleCourseModerationAsync for course {CourseId}", request.CourseId);
                throw;
            }
        }
    }
}
