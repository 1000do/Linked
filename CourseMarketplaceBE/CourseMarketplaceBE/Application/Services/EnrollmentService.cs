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
            throw new InvalidOperationException("Khóa học không tồn tại.");

        if (course.Price > 0)
            throw new InvalidOperationException("Khóa học này không miễn phí. Vui lòng thanh toán qua giỏ hàng.");

        // 1b. Check if instructor is trying to enroll in their own course
        if (course.InstructorId == userId)
            throw new InvalidOperationException("Bạn không thể ghi danh vào khóa học của chính mình.");

        // 2. Check if already enrolled
        if (await _repo.IsEnrolledAsync(userId, courseId))
            throw new InvalidOperationException("Bạn đã ghi danh vào khóa học này rồi.");

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
        if (enrollment == null) return null;

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
        if (enrollment == null) throw new InvalidOperationException("Enrollment not found.");

        // 1. Mark this specific material as completed if not already
        var alreadyCompleted = await _repo.IsMaterialCompletedAsync(enrollment.EnrollmentId, materialId);
        if (!alreadyCompleted)
        {
            var completion = new MaterialCompletion
            {
                EnrollmentId = enrollment.EnrollmentId,
                MaterialId = materialId,
                CompletedAt = DateTime.Now
            };
            await _repo.AddMaterialCompletionAsync(completion);

            // 2. Sync the old LearnedMaterialCount for backward compatibility
            if (enrollment.Progress == null)
            {
                enrollment.Progress = new EnrollmentProgress
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    LearnedMaterialCount = 1,
                    LastModifiedAt = DateTime.Now
                };
                await _repo.AddEnrollmentProgressAsync(enrollment.Progress);
            }
            else
            {
                var actualCount = await _repo.GetCompletedMaterialCountAsync(enrollment.EnrollmentId);
                enrollment.Progress.LearnedMaterialCount = actualCount + 1;
                enrollment.Progress.LastModifiedAt = DateTime.Now;
            }

            // 3. Mark enrollment as completed if reached total
            var stats = await _courseRepo.GetCourseStatsAsync(courseId);
            var totalMaterials = stats?.TotalMaterials ?? 0;
            var currentLearned = await _repo.GetCompletedMaterialCountAsync(enrollment.EnrollmentId) + 1;

            if (currentLearned >= totalMaterials && totalMaterials > 0)
            {
                enrollment.IsCompleted = true;
                enrollment.CompletedDate = DateOnly.FromDateTime(DateTime.Now);
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
