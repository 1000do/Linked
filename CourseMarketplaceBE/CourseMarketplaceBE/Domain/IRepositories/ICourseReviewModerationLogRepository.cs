using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface ICourseReviewModerationLogRepository
    {
        Task AddAsync(CourseReviewModerationLog log);
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Gets paginated course review moderation logs for admin view.
        /// </summary>
        Task<(List<CourseReviewModerationLog> Items, int TotalCount)> GetPagedAdminAsync(int page, int pageSize);

        /// <summary>
        /// Gets a detailed course review moderation log for admin view.
        /// </summary>
        Task<CourseReviewModerationLog?> GetAdminDetailByIdAsync(int logId);
    }
}
