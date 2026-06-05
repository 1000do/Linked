using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Lấy danh sách reviews ─────────────────────────────────────────────

    public async Task<(List<CourseReview> Items, int TotalCount)> GetCourseReviewsWithDetailsAsync(
        int courseId, int page, int pageSize, int? starFilter = null)
    {
        var query = _context.CourseReviews
            .Include(r => r.Enrollment)
                .ThenInclude(e => e!.User)
                    .ThenInclude(u => u!.UserNavigation)
            .Where(r => r.Enrollment != null
                     && r.Enrollment.CourseId == courseId
                     // Exclude reviews the user themselves deleted (status="removed")
                     // but keep admin-removed ones (status="violating") to show placeholder
                     && !(r.IsRemoved == true && r.CourseReviewStatus == "removed"));

        // Server-side star filter (làm tròn rating đến số nguyên gần nhất)
        if (starFilter.HasValue)
        {
            query = query.Where(r => r.Rating != null
                && (int)Math.Round((double)r.Rating.Value) == starFilter.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<LessonReview> Items, int TotalCount)> GetLessonReviewsWithDetailsAsync(
        int lessonId, int page, int pageSize)
    {
        var query = _context.LessonReviews
            .Include(r => r.Enrollment)
                .ThenInclude(e => e!.User)
                    .ThenInclude(u => u!.UserNavigation)
            .Include(r => r.Lesson)
                .ThenInclude(l => l!.Course)
            .Where(r => r.LessonId == lessonId
                     // Exclude reviews the user themselves deleted (status="removed")
                     // but keep admin-removed ones (status="violating") to show placeholder
                     && !(r.IsRemoved == true && r.LessonReviewStatus == "removed"));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // ── Check pending moderation reports ────────────────────────────────

    /// <summary>Returns true if a course review has any report that is pending or under_review.</summary>
    public async Task<bool> HasPendingCourseReviewReportsAsync(int reviewId)
    {
        return await _context.CourseReviewReports
            .AnyAsync(rp => rp.CourseReviewId == reviewId
                         && (rp.UserReportsStatus == "pending" || rp.UserReportsStatus == "under_review"));
    }

    /// <summary>Returns true if a lesson review has any report that is pending or under_review.</summary>
    public async Task<bool> HasPendingLessonReviewReportsAsync(int reviewId)
    {
        return await _context.LessonReviewReports
            .AnyAsync(rp => rp.LessonReviewId == reviewId
                         && (rp.UserReportsStatus == "pending" || rp.UserReportsStatus == "under_review"));
    }

    // ── Thống kê sao ─────────────────────────────────────────────────────

    public async Task<List<float>> GetCourseReviewRatingsAsync(int courseId)
    {
        return await _context.CourseReviews
            .Include(r => r.Enrollment)
            .Where(r => r.Enrollment != null
                     && r.Enrollment.CourseId == courseId
                     && r.IsRemoved != true
                     && r.Rating != null)
            .Select(r => r.Rating!.Value)
            .ToListAsync();
    }

    public async Task<List<float>> GetLessonReviewRatingsAsync(int lessonId)
    {
        return await _context.LessonReviews
            .Where(r => r.LessonId == lessonId
                     && r.IsRemoved != true
                     && r.Rating != null)
            .Select(r => r.Rating!.Value)
            .ToListAsync();
    }

    public async Task<List<(int LessonId, double AvgRating, int Count)>> GetLessonRatingsForCourseAsync(int courseId)
    {
        return await _context.LessonReviews
            .Include(r => r.Lesson)
            .Where(r => r.Lesson != null
                     && r.Lesson.CourseId == courseId
                     && r.IsRemoved != true
                     && r.Rating != null)
            .GroupBy(r => r.LessonId!.Value)
            .Select(g => new
            {
                LessonId = g.Key,
                AvgRating = g.Average(r => (double)r.Rating!.Value),
                Count = g.Count()
            })
            .ToListAsync()
            .ContinueWith(t => t.Result
                .Select(x => (x.LessonId, x.AvgRating, x.Count))
                .ToList());
    }

    // ── Lookup theo enrollment ────────────────────────────────────────────

    public async Task<CourseReview?> GetCourseReviewByEnrollmentAsync(int enrollmentId)
    {
        return await _context.CourseReviews
            .FirstOrDefaultAsync(r => r.EnrollmentId == enrollmentId && r.IsRemoved != true);
    }

    public async Task<LessonReview?> GetLessonReviewByEnrollmentAsync(int enrollmentId, int lessonId)
    {
        return await _context.LessonReviews
            .FirstOrDefaultAsync(r => r.EnrollmentId == enrollmentId
                                   && r.LessonId == lessonId
                                   && r.IsRemoved != true);
    }

    // ── Lookup theo reviewId ──────────────────────────────────────────────

    public async Task<CourseReview?> GetCourseReviewByIdAsync(int reviewId)
    {
        return await _context.CourseReviews
            .Include(r => r.Enrollment)
                .ThenInclude(e => e!.User)
            .FirstOrDefaultAsync(r => r.CourseReviewId == reviewId);
    }

    public async Task<LessonReview?> GetLessonReviewByIdAsync(int reviewId)
    {
        return await _context.LessonReviews
            .Include(r => r.Enrollment)
                .ThenInclude(e => e!.User)
            .FirstOrDefaultAsync(r => r.LessonReviewId == reviewId);
    }

    // ── Thêm ─────────────────────────────────────────────────────────────

    public async Task AddCourseReviewAsync(CourseReview review)
    {
        await _context.CourseReviews.AddAsync(review);
    }

    public async Task AddLessonReviewAsync(LessonReview review)
    {
        await _context.LessonReviews.AddAsync(review);
    }

    // ── Sửa ──────────────────────────────────────────────────────────────

    public void UpdateCourseReview(CourseReview review)
    {
        _context.CourseReviews.Update(review);
    }

    public void UpdateLessonReview(LessonReview review)
    {
        _context.LessonReviews.Update(review);
    }

    // ── Xóa mềm ──────────────────────────────────────────────────────────

    public void SoftDeleteCourseReview(CourseReview review)
    {
        review.IsRemoved = true;
        review.UpdatedAt = DateTime.Now;
        _context.CourseReviews.Update(review);
    }

    public void SoftDeleteLessonReview(LessonReview review)
    {
        review.IsRemoved = true;
        review.UpdatedAt = DateTime.Now;
        _context.LessonReviews.Update(review);
    }

    // ── Save ─────────────────────────────────────────────────────────────

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

