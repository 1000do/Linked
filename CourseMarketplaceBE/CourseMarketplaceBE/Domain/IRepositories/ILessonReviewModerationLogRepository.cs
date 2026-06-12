using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface ILessonReviewModerationLogRepository
    {
        Task AddAsync(LessonReviewModerationLog log);
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Gets paginated lesson review moderation logs for admin view.
        /// </summary>
        Task<(List<LessonReviewModerationLog> Items, int TotalCount)> GetPagedAdminAsync(int page, int pageSize);

        /// <summary>
        /// Gets a detailed lesson review moderation log for admin view.
        /// </summary>
        Task<LessonReviewModerationLog?> GetAdminDetailByIdAsync(int logId);
    }
}
