using CourseMarketplaceBE.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    /// <summary>
    /// Repository interface for CourseAiUsageLog entity.
    /// </summary>
    public interface ICourseAiUsageLogRepository
    {
        /// <summary>
        /// Add a new usage log.
        /// </summary>
        Task AddAsync(CourseAiUsageLog log);

        /// <summary>
        /// Save changes to the database.
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Get log by ID.
        /// </summary>
        Task<CourseAiUsageLog?> GetByIdAsync(int logId);

        /// <summary>
        /// Get all logs for an integration.
        /// </summary>
        Task<List<CourseAiUsageLog>> GetByIntegrationIdAsync(int integrationId);

        /// <summary>
        /// Get all usage logs.
        /// </summary>
        Task<List<CourseAiUsageLog>> GetAllAsync();
    }
}
