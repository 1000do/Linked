using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                    "Khóa học đã được phê duyệt",
                    $"Khóa học '{course.Title}' của bạn đã được phê duyệt và hiện đã hiển thị trên cửa hàng. {feedback}",
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
                            "Khóa học đã có nội dung mới",
                            $"Khóa học '{course.Title}' bạn đang tham gia vừa được giảng viên cập nhật nội dung mới. Hãy vào xem ngay!",
                            $"/Course/Learn?id={courseId}"
                        );
                    }
                }
            }

            // Invalidate Cache
            await _redisService.RemoveCacheAsync($"course:detail:{courseId}");

            return true;
        }

        public async Task<bool> RejectCourseAsync(int courseId, string reason)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return false;

            course.CourseStatus = "rejected";
            course.ModerationFeedback = reason;
            course.UpdatedAt = DateTime.Now;

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();

            if (course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Khóa học bị từ chối",
                    $"Khóa học '{course.Title}' của bạn không được phê duyệt. Lý do: {reason}",
                    $"/InstructorCourse/Editor?id={courseId}"
                );
            }

            // Invalidate Cache
            await _redisService.RemoveCacheAsync($"course:detail:{courseId}");

            return true;
        }

        public async Task<bool> FlagCourseAsync(int courseId, string reason)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return false;

            var currentFlags = (course.CourseFlagCount ?? 0) + 1;
            course.CourseFlagCount = currentFlags;
            course.CourseStatus = "flagged"; 
            course.ModerationFeedback = $"[VIOLATION FLAG #{currentFlags}] {reason}";
            course.UpdatedAt = DateTime.Now;

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();

            if (course.InstructorId.HasValue)
            {
                string subject = "Cảnh báo vi phạm khóa học";
                string message = "";

                if (currentFlags == 1)
                {
                    subject = "Nhắc nhở vi phạm khóa học (Lần 1)";
                    message = $"Khóa học '{course.Title}' của bạn đã bị gắn cờ vi phạm lần đầu và tạm ẩn. Lý do: {reason}. Vui lòng kiểm tra lại nội dung và tuân thủ quy định.";
                }
                else if (currentFlags == 2)
                {
                    subject = "Cảnh báo vi phạm nghiêm trọng (Lần 2)";
                    message = $"Khóa học '{course.Title}' của bạn tiếp tục vi phạm lần thứ 2. Đây là cảnh báo mạnh mẽ. Nếu tiếp tục vi phạm, khóa học sẽ bị xóa vĩnh viễn.";
                }
                else
                {
                    subject = "Thông báo ngừng kinh doanh khóa học vĩnh viễn (Lần 3)";
                    message = $"Khóa học '{course.Title}' của bạn đã vi phạm chính sách lần thứ 3. Hệ thống quyết định ngừng kinh doanh khóa học này vĩnh viễn. Bạn sẽ không thể chỉnh sửa nội dung hoặc nhận học viên mới, nhưng học viên cũ vẫn có thể truy cập nội dung đã mua.";
                    
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
            await _redisService.RemoveCacheAsync($"course:detail:{courseId}");

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
                        "course.title" => "Tiêu đề",
                        "course.description" => "Mô tả",
                        "course.thumbnail" => "Thumbnail",
                        "course.what_you_will_learn" => "Nội dung học được",
                        "course.requirements" => "Yêu cầu",
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
            course.CourseStatus = "rejected";
            course.ModerationFeedback = courseFeedbackParts.Count > 0
                ? string.Join("\n", courseFeedbackParts)
                : "Khóa học bị từ chối. Vui lòng kiểm tra các file bị đánh dấu.";
            course.UpdatedAt = DateTime.Now;

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();

            if (course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Khóa học bị từ chối",
                    $"Khóa học '{course.Title}' của bạn không được phê duyệt. Vui lòng kiểm tra chi tiết trong trang chỉnh sửa.",
                    $"/InstructorCourse/Editor?id={request.CourseId}"
                );
            }

            // Invalidate Cache
            await _redisService.RemoveCacheAsync($"course:detail:{request.CourseId}");

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
            var allEmbeddings = await _lessonService.GetAllMaterialEmbeddingsAsync();
            foreach (var e in allEmbeddings)
            {
                await _redisService.SetCacheAsync($"material_embedding:{e.EmbeddingId}", e, TimeSpan.FromHours(3));
            }
        }

        public async Task AssignAIModeratorsToCourseAsync(int courseId)
        {
            var thresholds = await _aiModerationService.GetScoreThresholdConfigAsync("moderation_threshold");
            var modelIds = await _aiModerationService.GetModelIdsByType("LLM");
            foreach (var mid in modelIds)
            {
                var existing = await _aiIntegrationRepository.GetByModelAndCourseAsync(mid, courseId);
                if (existing == null)
                {
                    await _courseService.IntegrateAItoCourseAsync(new CourseAIIntegrationCommand
                    {
                        CourseId = courseId,
                        ModelId = mid,
                        Role = "moderator",
                        IsEnabled = true,
                        ConfigJson = thresholds
                    });
                }
            }
        }

        public async Task<bool> PrepareForCourseAIModeration(int courseId)
        {
            try
            {
                await AssignAIModeratorsToCourseAsync(courseId);
                await _courseService.GetCourseWithDetailsAsync(courseId, null); // Cache course
                await PrepareMaterialEmbeddingsAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to prepare for AI moderation");
                throw;
            }
        }

        public async Task ResolveCourseAIModerationResult(CourseAIModerationResult result)
        {
            var courseId = result.CourseId;
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return;

            if (result.ModerationStatus == "APPROVED")
            {
                await ApproveCourseAsync(courseId, "AI moderation passed.");
            }
            else if (result.ModerationStatus == "REJECTED" || result.ModerationStatus == "FLAGGED")
            {
                var reason = string.Join("\n", result.stageLogs.Select(l => l.reason).Where(r => !string.IsNullOrEmpty(r)));
                await RejectCourseAsync(courseId, reason);
            }
            else if (result.ModerationStatus == "MANUAL_AUDIT")
            {
                course.CourseStatus = "pending";
                course.ModerationFeedback = "AI suggested manual audit.";
                _courseRepository.Update(course);
                await _courseRepository.SaveChangesAsync();
                await NotifyAdminAsync("Manual Audit Required", $"Course {courseId} requires manual audit.", $"/Admin/Moderation/Courses?id={courseId}");
            }
            
            // Notify instructor if status changed from Pending
            if (course.CourseStatus != "pending" && course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Cập nhật kết quả kiểm duyệt AI",
                    $"Khóa học '{course.Title}' của bạn đã có kết quả kiểm duyệt: {course.CourseStatus}.",
                    $"/InstructorCourse/Editor?id={courseId}"
                );
            }
        }

        public async Task LogCourseAiModerationResult(CourseAIModerationResult result)
        {
            var integrations = await _aiIntegrationRepository.GetByCourseIdAsync(result.CourseId);
            var integration = integrations.FirstOrDefault(i => i.IsEnabled);
            if (integration == null) return;

            foreach (var stage in result.stageLogs)
            {
                await _aiModerationService.SaveCourseAiUsageLog(new SaveCourseAiUsageLogCommand
                {
                    integration_id = integration.Id,
                    interaction_type = $"stage_{stage.stage}_step_{stage.step}",
                    input_json = new Dictionary<string, object> { { "course_id", result.CourseId } },
                    output_json = new Dictionary<string, object> { { "result", stage.result ?? "N/A" }, { "reason", stage.reason ?? "N/A" }, { "details", stage.details ?? new() } },
                    latency_ms = stage.latency_ms,
                    token_usage = 0,
                    error_message = null
                });
            }
        }

        public async Task<CourseAIModerationResult> HandleCourseModerationWithAIAsync(CouresModerationRequest request)
        {
            try
            {
                var isHealthy = await _aiModerationService.HealthCheckAsync();
                if (!isHealthy)
                {
                    await NotifyAdminAsync("AI Service Unhealthy", $"Course {request.CourseId} requires manual review due to AI service being unhealthy.", "/Admin/Moderation/Courses");
                    return new CourseAIModerationResult { CourseId = request.CourseId, ModerationStatus = "MANUAL_AUDIT" };
                }

                await PrepareForCourseAIModeration(request.CourseId);

                var thresholds = await _aiModerationService.GetScoreThresholdConfigAsync("moderation_threshold");
                var materials = await _materialRepository.GetByCourseIdAsync(request.CourseId);
                
                var semanticReq = new SemanticDuplicationRequest
                {
                    CourseId = request.CourseId,
                    MaterialIds = materials.Select(m => m.MaterialId).ToList(),
                    similarityScoreThreshold = thresholds.GetValueOrDefault("similarity", 0.8f)
                };

                var harmfulReq = new CourseHarmfulRequest
                {
                    CourseId = request.CourseId,
                    spamScoreThreshold = thresholds.GetValueOrDefault("spam", 0.7f),
                    toxicScoreThreshold = thresholds.GetValueOrDefault("toxic", 0.7f)
                };

                var result = await _aiModerationService.ModerateCourseFullPipelineAsync(semanticReq, harmfulReq);
                
                if (result.ModerationStatus == "MANUAL_AUDIT")
                {
                    await NotifyAdminAsync("Manual Audit Required", $"Course {request.CourseId} flagged for manual review by AI.", "/Admin/Moderation/Courses");
                }
                else
                {
                    await ResolveCourseAIModerationResult(result);
                }

                await LogCourseAiModerationResult(result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AI moderation for course {CourseId}", request.CourseId);
                // Handle specific HTTP exceptions if needed, but generic catch for now as per to_do
                await NotifyAdminAsync("Moderation Process Exception", $"Exception during AI moderation for course {request.CourseId}: {ex.Message}", null);
                throw;
            }
        }

        public async Task HandleCourseModerationAsync(CouresModerationRequest request)
        {
            try
            {
                await _courseRepository.UpdateCourseStatusAsync(request.CourseId, "pending", request.InstructorId);
                
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
                    return;
                }

                await HandleCourseModerationWithAIAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HandleCourseModerationAsync for course {CourseId}", request.CourseId);
                throw;
            }
        }
    }
}
