using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;

namespace CourseMarketplaceBE.Application.IServices;

public interface IReviewService
{
    Task<PagedResult<ReviewResponse>> GetCourseReviewsAsync(int courseId, int page = 1, int pageSize = 10);
    Task<PagedResult<ReviewResponse>> GetLessonReviewsAsync(int lessonId, int page = 1, int pageSize = 10);
    Task<ReviewStatsResponse> GetReviewStatsAsync(int courseId);
    Task SubmitReviewAsync(int userId, ReviewRequest request, bool requireCompletion);
    Task<EnrollmentStatusResponse> GetEnrollmentStatusAsync(int userId, int courseId);
    Task ReportReviewAsync(int userId, int reviewId, string type, string reason);
}
