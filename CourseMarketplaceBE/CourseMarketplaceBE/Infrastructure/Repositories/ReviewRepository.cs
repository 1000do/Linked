using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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

    public async Task<(List<CourseReview> Items, int TotalCount)> GetCourseReviewsWithDetailsAsync(int courseId, int page, int pageSize)
    {
        var query = _context.CourseReviews
            .Include(r => r.Enrollment)
                .ThenInclude(e => e!.User)
                    .ThenInclude(u => u!.UserNavigation)
            .Where(r => r.Enrollment != null && r.Enrollment.CourseId == courseId && r.IsRemoved != true);
            
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (items, totalCount);
    }

    public async Task<(List<LessonReview> Items, int TotalCount)> GetLessonReviewsWithDetailsAsync(int lessonId, int page, int pageSize)
    {
        var query = _context.LessonReviews
            .Include(r => r.Enrollment)
                .ThenInclude(e => e!.User)
                    .ThenInclude(u => u!.UserNavigation)
            .Include(r => r.Lesson)
            .Where(r => r.LessonId == lessonId && r.IsRemoved != true);
            
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (items, totalCount);
    }

    public async Task<List<float>> GetCourseReviewRatingsAsync(int courseId)
    {
        return await _context.CourseReviews
            .Include(r => r.Enrollment)
            .Where(r => r.Enrollment != null && r.Enrollment.CourseId == courseId && r.IsRemoved != true && r.Rating != null)
            .Select(r => r.Rating!.Value)
            .ToListAsync();
    }

    public async Task<CourseReview?> GetCourseReviewByEnrollmentAsync(int enrollmentId)
    {
        return await _context.CourseReviews
            .FirstOrDefaultAsync(r => r.EnrollmentId == enrollmentId && r.IsRemoved != true);
    }

    public async Task<LessonReview?> GetLessonReviewByEnrollmentAsync(int enrollmentId, int lessonId)
    {
        return await _context.LessonReviews
            .FirstOrDefaultAsync(r => r.EnrollmentId == enrollmentId && r.LessonId == lessonId && r.IsRemoved != true);
    }

    public async Task AddCourseReviewAsync(CourseReview review)
    {
        await _context.CourseReviews.AddAsync(review);
    }

    public async Task AddLessonReviewAsync(LessonReview review)
    {
        await _context.LessonReviews.AddAsync(review);
    }

    public void UpdateCourseReview(CourseReview review)
    {
        _context.CourseReviews.Update(review);
    }

    public void UpdateLessonReview(LessonReview review)
    {
        _context.LessonReviews.Update(review);
    }

    public async Task<CourseReview?> GetCourseReviewByIdAsync(int reviewId)
    {
        return await _context.CourseReviews
            .Include(r => r.Enrollment)
            .FirstOrDefaultAsync(r => r.CourseReviewId == reviewId);
    }

    public async Task<LessonReview?> GetLessonReviewByIdAsync(int reviewId)
    {
        return await _context.LessonReviews
            .Include(r => r.Enrollment)
            .FirstOrDefaultAsync(r => r.LessonReviewId == reviewId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
