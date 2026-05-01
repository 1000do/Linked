using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.Services
{
    public class ModerationService : IModerationService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public ModerationService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<CourseModerationDto>> GetPendingCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Instructor).ThenInclude(i => i.InstructorNavigation)
                .Include(c => c.Category)
                .Where(c => c.CourseStatus == "pending")
                .Select(c => new CourseModerationDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    InstructorName = c.Instructor.InstructorNavigation.FullName,
                    CategoryName = c.Category.CategoriesName,
                    Price = c.Price,
                    CreatedAt = c.CreatedAt,
                    CourseStatus = c.CourseStatus,
                    CourseThumbnailUrl = c.CourseThumbnailUrl
                })
                .ToListAsync();
        }

        public async Task<bool> ApproveCourseAsync(int courseId, string? feedback)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return false;

            course.CourseStatus = "published";
            course.ModerationFeedback = feedback;
            course.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            if (course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Khóa học đã được phê duyệt",
                    $"Khóa học '{course.Title}' của bạn đã được phê duyệt và hiện đã hiển thị trên cửa hàng. {feedback}",
                    $"/Course/Details/{courseId}"
                );
            }

            return true;
        }

        public async Task<bool> RejectCourseAsync(int courseId, string reason)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return false;

            course.CourseStatus = "rejected";
            course.ModerationFeedback = reason;
            course.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            if (course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Khóa học bị từ chối",
                    $"Khóa học '{course.Title}' của bạn không được phê duyệt. Lý do: {reason}",
                    $"/InstructorCourse/Editor?courseId={courseId}"
                );
            }

            return true;
        }

        public async Task<bool> FlagCourseAsync(int courseId, string reason)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return false;

            // Flagged course is usually moved to rejected or a blocked state
            course.CourseStatus = "rejected"; 
            course.ModerationFeedback = $"[VIOLATION FLAG] {reason}";
            course.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            if (course.InstructorId.HasValue)
            {
                await _notificationService.SendNotificationAsync(
                    course.InstructorId.Value,
                    "Cảnh báo vi phạm khóa học",
                    $"Khóa học '{course.Title}' của bạn đã bị gắn cờ vi phạm và tạm ẩn. Lý do: {reason}. Vui lòng kiểm tra lại nội dung.",
                    $"/InstructorCourse/Editor?courseId={courseId}"
                );
            }

            return true;
        }

        public async Task<List<UserReportModerationDto>> GetAllReportsAsync()
        {
            return await _context.UserReports
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new UserReportModerationDto
                {
                    ReportId = r.ReportId,
                    ReporterName = r.Reporter.Email,
                    Reason = r.Reason,
                    Description = r.Description,
                    ChatId = r.ChatId,
                    Status = r.UserReportsStatus,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> ResolveReportAsync(ResolveReportDto dto)
        {
            var report = await _context.UserReports.FindAsync(dto.ReportId);
            if (report == null) return false;

            report.UserReportsStatus = dto.Status;
            report.ResolutionNote = dto.ResolutionNote;
            report.ResolvedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
