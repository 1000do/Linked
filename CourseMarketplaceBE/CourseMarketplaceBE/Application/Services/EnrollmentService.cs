using System;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

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

        return new DTOs.ProgressResponse
        {
            EnrollmentId = enrollment.EnrollmentId,
            LearnedMaterialCount = enrollment.Progress?.LearnedMaterialCount ?? 0,
            TotalMaterialCount = totalMaterials
        };
    }

    public async Task UpdateProgressAsync(int userId, int courseId, int materialId)
    {
        var enrollment = await _repo.GetEnrollmentWithProgressAsync(userId, courseId);
        if (enrollment == null) throw new InvalidOperationException("Enrollment not found.");

        if (enrollment.Progress == null)
        {
            enrollment.Progress = new EnrollmentProgress
            {
                EnrollmentId = enrollment.EnrollmentId,
                LearnedMaterialCount = 0,
                LastModifiedAt = DateTime.Now
            };
            await _repo.AddEnrollmentProgressAsync(enrollment.Progress);
        }

        // Simulating completion: Increment count if not already completed
        // Note: For a real system, we'd check if this specific materialId was already learned.
        // But the current schema only has a count.
        
        var stats = await _courseRepo.GetCourseStatsAsync(courseId);
        if (enrollment.Progress.LearnedMaterialCount < (stats?.TotalMaterials ?? 0))
        {
            enrollment.Progress.LearnedMaterialCount++;
            enrollment.Progress.LastModifiedAt = DateTime.Now;

            // Mark enrollment as completed if reached total
            if (enrollment.Progress.LearnedMaterialCount >= (stats?.TotalMaterials ?? 0))
            {
                enrollment.IsCompleted = true;
                enrollment.CompletedDate = DateOnly.FromDateTime(DateTime.Now);
            }

            await _repo.SaveChangesAsync();
        }
    }
}
