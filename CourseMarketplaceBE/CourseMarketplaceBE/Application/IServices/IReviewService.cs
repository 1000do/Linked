using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IReviewService
{
    Task<IEnumerable<ReviewResponse>> GetCourseReviewsAsync(int courseId);
    Task<IEnumerable<ReviewResponse>> GetLessonReviewsAsync(int lessonId);
    Task<ReviewStatsResponse> GetReviewStatsAsync(int courseId);
    Task SubmitReviewAsync(int userId, ReviewRequest request, bool requireCompletion);
    Task<EnrollmentStatusResponse> GetEnrollmentStatusAsync(int userId, int courseId);
    Task ReportReviewAsync(int userId, int reviewId, string type, string reason);
}
