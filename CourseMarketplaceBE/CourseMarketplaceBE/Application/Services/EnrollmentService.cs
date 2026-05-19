using System;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Application.IServices;
using System.Collections.Generic;
using System.Linq;

namespace CourseMarketplaceBE.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly ICheckoutRepository _repo;
    private readonly ICourseRepository _courseRepo;

    public EnrollmentService(ICheckoutRepository repo, ICourseRepository courseRepo)
    {
        _repo = repo;
        _courseRepo = courseRepo;
    }

    public async Task EnrollFreeAsync(int userId, int courseId)
    {
        // 1. Check if course exists and is free
        var course = await _courseRepo.GetByIdAsync(courseId);
        if (course == null)
            throw new InvalidOperationException("Course not found.");

        if (course.Price > 0)
            throw new InvalidOperationException("This course is not free. Please complete the purchase via your cart.");

        // 1b. Check if instructor is trying to enroll in their own course
        if (course.InstructorId == userId)
            throw new InvalidOperationException("You cannot enroll in your own course.");

        // 2. Check if already enrolled
        if (await _repo.IsEnrolledAsync(userId, courseId))
            throw new InvalidOperationException("You have already enrolled in this course.");

        // 3. Perform enrollment
        await using var transaction = await _repo.BeginTransactionAsync();
        try
        {
            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                Title = course.Title,
                EnrollDate = DateOnly.FromDateTime(DateTime.Now),
                IsCompleted = false,
                EnrollmentStatus = "active",
                LastAccessedAt = DateTime.Now
            };
            await _repo.AddEnrollmentAsync(enrollment);
            await _repo.SaveChangesAsync();

            var progress = new EnrollmentProgress
            {
                EnrollmentId = enrollment.EnrollmentId,
                LearnedMaterialCount = 0,
                LastModifiedAt = DateTime.Now
            };
            await _repo.AddEnrollmentProgressAsync(progress);
            await _repo.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<DTOs.ProgressResponse> GetProgressAsync(int userId, int courseId)
    {
        var enrollment = await _repo.GetEnrollmentAsync(userId, courseId);
        
        if (enrollment == null) 
        {
            // Bypass cho giảng viên: Trả về tiến độ ảo nếu là chủ sở hữu
            var course = await _courseRepo.GetByIdAsync(courseId);
            if (course != null && course.InstructorId == userId)
            {
                var courseStats = await _courseRepo.GetCourseStatsAsync(courseId);
                return new DTOs.ProgressResponse
                {
                    EnrollmentId = 0, 
                    LearnedMaterialCount = courseStats?.TotalMaterials ?? 0, // Chủ sở hữu coi như học hết
                    TotalMaterialCount = courseStats?.TotalMaterials ?? 0,
                    CompletedMaterialIds = new List<int>()
                };
            }
            return null;
        }

        var stats = await _courseRepo.GetCourseStatsAsync(courseId);
        var totalMaterials = stats?.TotalMaterials ?? 0;
        var learnedCount = await _repo.GetCompletedMaterialCountAsync(enrollment.EnrollmentId);
        var completedIds = await _repo.GetCompletedMaterialIdsAsync(enrollment.EnrollmentId);

        return new DTOs.ProgressResponse
        {
            EnrollmentId = enrollment.EnrollmentId,
            LearnedMaterialCount = learnedCount,
            TotalMaterialCount = totalMaterials,
            CompletedMaterialIds = completedIds
        };
    }

    public async Task UpdateProgressAsync(int userId, int courseId, int materialId)
    {
        var enrollment = await _repo.GetEnrollmentWithProgressAsync(userId, courseId);
        
        // Nếu không tìm thấy enrollment, kiểm tra xem có phải owner không
        if (enrollment == null) 
        {
            var isOwner = await _courseRepo.IsOwnerAsync(userId, courseId);
            if (isOwner) 
            {
                // Giảng viên xem bài học của chính mình -> Không cần lưu tiến độ vào DB
                return;
            }
            throw new InvalidOperationException("You are not enrolled in this course.");
        }

        // 1. Đánh dấu tài liệu này đã hoàn thành
        var alreadyCompleted = await _repo.IsMaterialCompletedAsync(enrollment.EnrollmentId, materialId);
        if (!alreadyCompleted)
        {
            var completion = new MaterialCompletion
            {
                EnrollmentId = enrollment.EnrollmentId,
                MaterialId = materialId,
                CompletedAt = DateTime.UtcNow
            };
            await _repo.AddMaterialCompletionAsync(completion);

            // Lưu thay đổi ngay để các hàm đếm phía sau chính xác
            await _repo.SaveChangesAsync();

            // 2. Cập nhật số lượng bài đã học trong bảng Progress
            if (enrollment.Progress == null)
            {
                enrollment.Progress = new EnrollmentProgress
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    LearnedMaterialCount = 1,
                    LastModifiedAt = DateTime.UtcNow
                };
                await _repo.AddEnrollmentProgressAsync(enrollment.Progress);
            }
            else
            {
                // Đếm chính xác số lượng từ DB sau khi đã SaveChanges bản ghi mới
                var actualCount = await _repo.GetCompletedMaterialCountAsync(enrollment.EnrollmentId);
                enrollment.Progress.LearnedMaterialCount = actualCount;
                enrollment.Progress.LastModifiedAt = DateTime.UtcNow;
            }

            // 3. Kiểm tra hoàn thành khóa học
            var stats = await _courseRepo.GetCourseStatsAsync(courseId);
            var totalMaterials = stats?.TotalMaterials ?? 0;
            var currentLearned = enrollment.Progress.LearnedMaterialCount;

            if (currentLearned >= totalMaterials && totalMaterials > 0)
            {
                enrollment.IsCompleted = true;
                enrollment.CompletedDate = DateOnly.FromDateTime(DateTime.UtcNow);
            }

            await _repo.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<DTOs.EnrolledCourseResponse>> GetMyEnrolledCoursesAsync(int userId)
    {
        var enrollments = await _repo.GetMyEnrolledCoursesAsync(userId);

        var courseIds = enrollments.Select(e => e.CourseId ?? 0).Where(id => id > 0).ToList();
        var stats = await _courseRepo.GetCourseStatsAsync(courseIds);

        return await Task.WhenAll(enrollments.Select(async e =>
        {
            var s = stats.FirstOrDefault(st => st.CourseId == e.CourseId);
            var totalMaterials = s?.TotalMaterials ?? 0;
            var learnedCount = await _repo.GetCompletedMaterialCountAsync(e.EnrollmentId);
            var pct = totalMaterials > 0 ? (double)learnedCount / totalMaterials * 100 : 0;

            return new DTOs.EnrolledCourseResponse
            {
                CourseId = e.CourseId ?? 0,
                Title = e.Course?.Title ?? "Unknown Course",
                CourseThumbnailUrl = e.Course?.CourseThumbnailUrl,
                InstructorName = e.Course?.Instructor?.InstructorNavigation?.FullName ?? "Unknown Instructor",
                ProgressPercentage = Math.Round(pct, 1),
                IsCompleted = e.IsCompleted == true,
                LastAccessedAt = e.LastAccessedAt
            };
        }));
    }
}
