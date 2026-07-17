using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Application.Services
{
    public class CourseModerationService : ICourseModerationService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IRedisService _redisService;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly ILessonService _lessonService;
        private readonly IEmbeddingService _embeddingService;

        public CourseModerationService(
            ICourseRepository courseRepository,
            IMaterialRepository materialRepository,
            ILessonRepository lessonRepository,
            IEnrollmentRepository enrollmentRepository,
            IRedisService redisService,
            INotificationService notificationService,
            IUserRepository userRepository,
            ILessonService lessonService,
            IEmbeddingService embeddingService)
        {
            _courseRepository = courseRepository;
            _materialRepository = materialRepository;
            _lessonRepository = lessonRepository;
            _enrollmentRepository = enrollmentRepository;
            _redisService = redisService;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _lessonService = lessonService;
            _embeddingService = embeddingService;
        }

        public async Task<CourseModerationStatsDto> GetCourseModerationStatsAsync()
        {
            return await _courseRepository.GetCourseModerationStatsAsync();
            // return new CourseModerationStatsDto();
        }

        public async Task<PagedResult<CourseModerationDto>> GetPendingCoursesAsync(ModerationFilterDto filter)
        {
            return await _courseRepository.GetPendingCoursesModerationAsync(filter);
            // return new PagedResult<CourseModerationDto>();
        }

        public async Task<bool> ApproveCourseAsync(int courseId, string? feedback)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return false;

            course.CourseStatus = CourseStatus.Published.ToValue();
            course.ModerationFeedback = null;
            course.FieldModerationFeedbacks.Clear();
            course.UpdatedAt = DateTime.Now;
            course.LastApprovedAt = DateTime.Now;

            var materials = await _materialRepository.GetByCourseIdAsync(courseId);
            if (materials != null && materials.Any())
            {
                foreach (var material in materials)
                {
                    if (material.LearningStatus != LearningStatus.Removed.ToValue())
                    {
                        material.ModerationFeedback = null;
                        if (material.LearningStatus != LearningStatus.Active.ToValue())
                        {
                            material.LearningStatus = LearningStatus.Active.ToValue();
                        }
                        _materialRepository.Update(material);
                    }
                }

                await _embeddingService.PersistPendingMaterialEmbeddingsAsync(courseId, new HashSet<int>());
            }

            var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
            if (lessons != null && lessons.Any())
            {
                foreach (var lesson in lessons)
                {
                    if (lesson.LessonStatus != LessonStatus.Active.ToValue())
                    {
                        lesson.LessonStatus = LessonStatus.Active.ToValue();
                        _lessonRepository.Update(lesson);
                    }
                }
            }

            _courseRepository.Update(course);
            int rowsApprove = await _courseRepository.SaveChangesAsync();
            /* zero rows exception removed */

            if (course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Course Approved",
                    $"Your course '{course.Title}' has been approved and is now published in the store. {feedback}",
                    $"/Course/Details/{courseId}"
                );

                var enrolledUserIds = await _enrollmentRepository.GetEnrolledUserIdsAsync(courseId);
                // var enrolledUserIds = new List<int>();
                if (enrolledUserIds.Any())
                {
                    var studentNotifications = enrolledUserIds.Select(studentId => new NotificationBulkDto
                    {
                        ReceiverId = studentId,
                        Title = "Course Content Updated",
                        Content = $"The course '{course.Title}' you enrolled in has been updated with new content. Check it out now!",
                        LinkAction = $"/Course/Learn?id={courseId}"
                    }).ToList();

                    await _notificationService.SendBulkNotificationsAsync(studentNotifications);
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
            int rowsReject = await _courseRepository.SaveChangesAsync();
            /* zero rows exception removed */

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

            if ((course.CourseFlagCount ?? 0) >= 3) return false;

            var currentFlags = (course.CourseFlagCount ?? 0) + 1;
            course.CourseFlagCount = currentFlags;

            course.ModerationFeedback = $"[VIOLATION FLAG #{currentFlags}] {reason}";
            course.UpdatedAt = DateTime.Now;

            _courseRepository.Update(course);
            int rowsFlag = await _courseRepository.SaveChangesAsync();
            /* zero rows exception removed */

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

                    course.CourseStatus = CourseStatus.Archived.ToValue();
                    _courseRepository.Update(course);
                    int rowsArchive1 = await _courseRepository.SaveChangesAsync();
                    /* zero rows exception removed */
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

        public async Task<bool> UnflagCourseAsync(int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null) return false;

            int currentFlags = course.CourseFlagCount ?? 0;
            if (currentFlags <= 0) return false;

            if (currentFlags > 0)
            {
                int newFlags = currentFlags - 1;
                course.CourseFlagCount = newFlags;

                if (currentFlags == 3 && newFlags == 2 && string.Equals(course.CourseStatus, CourseStatus.Archived.ToValue(), StringComparison.OrdinalIgnoreCase))
                {
                    course.CourseStatus = CourseStatus.Rejected.ToValue();
                }

                course.UpdatedAt = DateTime.Now;
                _courseRepository.Update(course);
                int rows = await _courseRepository.SaveChangesAsync();
                /* zero rows exception removed */

                if (course.InstructorId.HasValue)
                {
                    await _notificationService.SendNotificationAsync(
                        course.InstructorId.Value,
                        "Course Unflagged",
                        $"Your course '{course.Title}' has had a violation flag removed. Current flags: {newFlags}.",
                        $"/InstructorCourse/Editor/{courseId}"
                    );
                }

                await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
            }
            return true;
        }

        public async Task<bool> RejectCourseDetailedAsync(RejectCourseDetailedRequest request)
        {
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null) return false;

            var rejectedLessonIds = new HashSet<int>();
            var rejectedMaterialIds = new HashSet<int>();

            foreach (var item in request.Items)
            {
                if (item.Target == "file" && item.MaterialId.HasValue)
                {
                    rejectedMaterialIds.Add(item.MaterialId.Value);
                    var material = await _materialRepository.GetByIdAsync(item.MaterialId.Value);
                    if (material != null)
                    {
                        material.LearningStatus = LearningStatus.Rejected.ToValue();
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
                }
                else if (item.Target.StartsWith("course."))
                {
                    var existingFieldFeedback = course.FieldModerationFeedbacks.FirstOrDefault(f => f.FieldName == item.Target);
                    if (existingFieldFeedback != null)
                    {
                        existingFieldFeedback.FeedbackText = item.Reason;
                        existingFieldFeedback.DateAdded = DateTime.Now;
                    }
                    else
                    {
                        course.FieldModerationFeedbacks.Add(new CourseFieldModerationFeedback
                        {
                            CourseId = course.CourseId,
                            FieldName = item.Target,
                            FeedbackText = item.Reason,
                            DateAdded = DateTime.Now
                        });
                    }
                }
            }

            foreach (var lessonId in rejectedLessonIds)
            {
                var lesson = await _lessonRepository.GetByIdAsync(lessonId);
                if (lesson != null)
                {
                    lesson.LessonStatus = LessonStatus.Rejected.ToValue();
                    lesson.UpdatedAt = DateTime.Now;
                    _lessonRepository.Update(lesson);
                }
            }

            // Aggregate all feedbacks (both old and new)
            var courseFeedbackParts = new System.Text.StringBuilder();

            foreach (var fieldFeedback in course.FieldModerationFeedbacks)
            {
                var label = fieldFeedback.FieldName switch
                {
                    "course.title" => "Course Title",
                    "course.description" => "Description",
                    "course.thumbnail" => "Thumbnail",
                    "course.what_you_will_learn" => "What You Will Learn",
                    "course.requirements" => "Requirements",
                    _ => fieldFeedback.FieldName
                };
                courseFeedbackParts.AppendLine($"- {label}: {fieldFeedback.FeedbackText}");
            }

            var allMaterials = await _materialRepository.GetByCourseIdAsync(request.CourseId);
            if (allMaterials != null)
            {
                var rejectedMaterials = allMaterials.Where(m => !string.IsNullOrEmpty(m.ModerationFeedback)).ToList();
                foreach (var m in rejectedMaterials)
                {
                    var lessonTitle = m.Lesson?.Title ?? "Unknown Lesson";
                    courseFeedbackParts.AppendLine($"- Lesson '{lessonTitle}' - File '{m.Title}': {m.ModerationFeedback}");
                }
            }

            course.CourseStatus = CourseStatus.Rejected.ToValue();
            course.ModerationFeedback = courseFeedbackParts.Length > 0
                ? courseFeedbackParts.ToString().TrimEnd()
                : "Course rejected. Please check flagged files.";
            course.UpdatedAt = DateTime.Now;

            // Conditionally persist pending embeddings for materials that were NOT rejected
            await _embeddingService.PersistPendingMaterialEmbeddingsAsync(request.CourseId, excludedMaterialIds: rejectedMaterialIds);

            _courseRepository.Update(course);
            
            try
            {
                int rowsRejectDetailed = await _courseRepository.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                throw new BadRequestException("Database operation failed due to a constraint violation or data issue while saving the course rejection.");
            }

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

            if ((course.CourseFlagCount ?? 0) >= 3) return false;

            var courseFeedbackParts = new List<string>();
            var flaggedLessonIds = new HashSet<int>();

            course.ModerationFeedback = null;
            course.FieldModerationFeedbacks.Clear();
            var allMaterials = await _materialRepository.GetByCourseIdAsync(request.CourseId);
            if (allMaterials != null)
            {
                foreach (var m in allMaterials)
                {
                    if (m.LearningStatus != LearningStatus.Removed.ToValue())
                    {
                        m.ModerationFeedback = null;
                        if (m.LearningStatus != LearningStatus.Active.ToValue())
                        {
                            m.LearningStatus = LearningStatus.Active.ToValue();
                        }
                        _materialRepository.Update(m);
                    }
                }
            }

            var lessons = await _lessonRepository.GetByCourseIdAsync(request.CourseId);
            if (lessons != null)
            {
                foreach (var l in lessons)
                {
                    if (l.LessonStatus != LessonStatus.Active.ToValue())
                    {
                        l.LessonStatus = LessonStatus.Active.ToValue();
                        _lessonRepository.Update(l);
                    }
                }
            }

            foreach (var item in request.Items)
            {
                if (item.Target == "file" && item.MaterialId.HasValue)
                {
                    var material = await _materialRepository.GetByIdAsync(item.MaterialId.Value);
                    if (material != null)
                    {
                        material.LearningStatus = LearningStatus.Flagged.ToValue();
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
                    course.FieldModerationFeedbacks.Add(new CourseFieldModerationFeedback
                    {
                        CourseId = course.CourseId,
                        FieldName = item.Target,
                        FeedbackText = item.Reason,
                        DateAdded = DateTime.Now
                    });
                }
            }

            foreach (var lessonId in flaggedLessonIds)
            {
                var lesson = await _lessonRepository.GetByIdAsync(lessonId);
                if (lesson != null)
                {
                    lesson.LessonStatus = LessonStatus.Rejected.ToValue();
                    lesson.UpdatedAt = DateTime.Now;
                    _lessonRepository.Update(lesson);
                }
            }

            var currentFlags = (course.CourseFlagCount ?? 0) + 1;
            course.CourseFlagCount = currentFlags;

            var detailedReason = courseFeedbackParts.Count > 0
                ? string.Join("\n", courseFeedbackParts)
                : "Content flagged for policy violations.";

            course.ModerationFeedback = $"[VIOLATION FLAG #{currentFlags}] {detailedReason}";
            course.UpdatedAt = DateTime.Now;

            _courseRepository.Update(course);
            int rowsFlagDetailed = await _courseRepository.SaveChangesAsync();
            if (rowsFlagDetailed <= 0)
                throw new InvalidOperationException("Failed to save changes when flagging course (detailed).");

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
                    int rowsArchive2 = await _courseRepository.SaveChangesAsync();
                    /* zero rows exception removed */
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
