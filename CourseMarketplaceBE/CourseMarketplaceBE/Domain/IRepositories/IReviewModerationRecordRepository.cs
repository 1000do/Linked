using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IReviewModerationRecordRepository
{
    Task AddCourseReviewModerationRecordAsync(CourseReviewModerationRecord record);
    Task AddLessonReviewModerationRecordAsync(LessonReviewModerationRecord record);

    Task<(System.Collections.Generic.List<CourseReviewModerationRecord> Items, int TotalCount)> GetCourseReviewModerationRecordsAsync(PagedReviewModerationRequest request);
    Task<(System.Collections.Generic.List<LessonReviewModerationRecord> Items, int TotalCount)> GetLessonReviewModerationRecordsAsync(PagedReviewModerationRequest request);

    Task<CourseReviewModerationRecord?> GetCourseReviewModerationRecordByIdAsync(int recordId);
    Task<LessonReviewModerationRecord?> GetLessonReviewModerationRecordByIdAsync(int recordId);

    Task UpdateCourseReviewModerationRecordAsync(CourseReviewModerationRecord record);
    Task UpdateLessonReviewModerationRecordAsync(LessonReviewModerationRecord record);

    Task<int> CountByModerationStatusAsync(string moderationStatus);
    Task<int> SaveChangesAsync();
}
