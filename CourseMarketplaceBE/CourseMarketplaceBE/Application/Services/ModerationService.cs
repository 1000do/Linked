using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public ModerationService(
            ICourseRepository courseRepository, 
            IChatRepository chatRepository, 
            INotificationService notificationService,
            IMaterialRepository materialRepository,
            ILessonRepository lessonRepository,
            ICheckoutRepository checkoutRepository,
            IRedisService redisService)
        {
            _courseRepository = courseRepository;
            _chatRepository = chatRepository;
            _notificationService = notificationService;
            _materialRepository = materialRepository;
            _lessonRepository = lessonRepository;
            _checkoutRepository = checkoutRepository;
            _redisService = redisService;
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
                    "Course Rejected",
                    $"Your course '{course.Title}' was not approved. Reason: {reason}",
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
            course.CourseStatus = "rejected";
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
    }
}
