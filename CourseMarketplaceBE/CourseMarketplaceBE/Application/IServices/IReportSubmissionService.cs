using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IReportSubmissionService
{
    Task<bool> CreateCourseReportAsync(int reporterId, CreateCourseReportRequest request);
    Task<bool> CreateCourseReviewReportAsync(int reporterId, CreateCourseReviewReportRequest request);
    Task<bool> CreateLessonReviewReportAsync(int reporterId, CreateLessonReviewReportRequest request);

    Task<IEnumerable<MyCourseReportResponse>> GetMyCourseReportsAsync(int reporterId);
    Task<IEnumerable<MyReviewReportResponse>> GetMyReviewReportsAsync(int reporterId);

    Task<IEnumerable<CourseReportDetailResponse>> GetReportsOnMyCourseAsync(int instructorId, int courseId);
}
