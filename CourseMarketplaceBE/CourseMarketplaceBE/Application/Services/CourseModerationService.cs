using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Application.Services
{
    public class CourseModerationService : ICourseModerationService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICheckoutRepository _checkoutRepository;
        private readonly IRedisService _redisService;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;

        public CourseModerationService(
            ICourseRepository courseRepository,
            IMaterialRepository materialRepository,
            ILessonRepository lessonRepository,
            ICheckoutRepository checkoutRepository,
            IRedisService redisService,
            INotificationService notificationService,
            IUserRepository userRepository)
        {
            _courseRepository = courseRepository;
            _materialRepository = materialRepository;
            _lessonRepository = lessonRepository;
            _checkoutRepository = checkoutRepository;
            _redisService = redisService;
            _notificationService = notificationService;
            _userRepository = userRepository;
        }

        public async Task<CourseModerationStatsDto> GetCourseModerationStatsAsync()
        {
            return await _courseRepository.GetCourseModerationStatsAsync();
        }

        public async Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseModerationDto>> GetPendingCoursesAsync(ModerationFilterDto filter)
        {
            return await _courseRepository.GetPendingCoursesModerationAsync(filter);
        }

        public async Task<bool> ApproveCourseAsync(int courseId, string? feedback)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return false;

            course.CourseStatus = "published";
            course.ModerationFeedback = null;
            course.UpdatedAt = DateTime.Now;
            course.LastApprovedAt = DateTime.Now;

            var materials = await _materialRepository.GetByCourseIdAsync(courseId);
            if (materials != null && materials.Any())
            {
                foreach (var material in materials)
                {
                    material.ModerationFeedback = null;
                    material.LearningStatus = "active";
                    _materialRepository.Update(material);
                }
            }

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

            await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
            return true;
        }

        public async Task<bool> RejectCourseDetailedAsync(RejectCourseDetailedRequest request)
        {
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null) return false;

            var courseFeedbackParts = new List<string>();
            var rejectedLessonIds = new HashSet<int>();

            course.ModerationFeedback = null;
            var allMaterials = await _materialRepository.GetByCourseIdAsync(request.CourseId);
            if (allMaterials != null)
            {
                foreach (var m in allMaterials)
                {
                    m.ModerationFeedback = null;
                    m.LearningStatus = "active";
                    _materialRepository.Update(m);
                }
            }

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
                        material.LearningStatus = "rejected";
                        material.ModerationFeedback = item.Reason;
                        material.UpdatedAt = DateTime.Now;
                        _materialRepository.Update(material);

                        if (material.LessonId.HasValue)
                        {
                            rejectedLessonIds.Add(material.LessonId.Value);
                        }
                    }
                }
                else if (item.Target == "lesson.title" && item.LessonId.HasValue)
                {
                    rejectedLessonIds.Add(item.LessonId.Value);
                    courseFeedbackParts.Add($"[Lesson: {item.LessonTitle ?? $"#{item.LessonId}"}] {item.Reason}");
                }
                else if (item.Target.StartsWith("course."))
                {
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

            await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(request.CourseId));
            return true;
        }

        public async Task<bool> FlagCourseDetailedAsync(RejectCourseDetailedRequest request)
        {
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null) return false;

            var courseFeedbackParts = new List<string>();
            var flaggedLessonIds = new HashSet<int>();

            course.ModerationFeedback = null;
            var allMaterials = await _materialRepository.GetByCourseIdAsync(request.CourseId);
            if (allMaterials != null)
            {
                foreach (var m in allMaterials)
                {
                    m.ModerationFeedback = null;
                    m.LearningStatus = "active";
                    _materialRepository.Update(m);
                }
            }

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

            if (course.InstructorId.HasValue)
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

        public async Task NotifyAdminAsync(string title, string content, string? linkAction)
        {
            var adminId = await _userRepository.GetAdminIdAsync();
            if (adminId.HasValue) await _notificationService.SendNotificationAsync(adminId.Value, title, content, linkAction);
        }
    }
}
