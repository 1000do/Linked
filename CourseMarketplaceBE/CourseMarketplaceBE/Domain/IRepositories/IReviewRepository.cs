using CourseMarketplaceBE.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IReviewRepository
{
    Task<(List<CourseReview> Items, int TotalCount)> GetCourseReviewsWithDetailsAsync(int courseId, int page, int pageSize);
    Task<(List<LessonReview> Items, int TotalCount)> GetLessonReviewsWithDetailsAsync(int lessonId, int page, int pageSize);
    Task<List<float>> GetCourseReviewRatingsAsync(int courseId);
    Task<CourseReview?> GetCourseReviewByEnrollmentAsync(int enrollmentId);
    Task<LessonReview?> GetLessonReviewByEnrollmentAsync(int enrollmentId, int lessonId);
    Task AddCourseReviewAsync(CourseReview review);
    Task AddLessonReviewAsync(LessonReview review);
    void UpdateCourseReview(CourseReview review);
    void UpdateLessonReview(LessonReview review);
    Task<CourseReview?> GetCourseReviewByIdAsync(int reviewId);
    Task<LessonReview?> GetLessonReviewByIdAsync(int reviewId);
    Task SaveChangesAsync();
}
