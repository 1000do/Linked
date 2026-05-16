using CourseMarketplaceBE.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    /// <summary>
    /// Repository interface for CourseAiIntegration entity (formerly AiModelsCourse).
    /// </summary>
    public interface ICourseAiIntegrationRepository
    {
        /// <summary>
        /// Add a new course AI integration.
        /// </summary>
        Task AddAsync(CourseAiIntegration integration);

        /// <summary>
        /// Add multiple course AI integrations.
        /// </summary>
        Task AddRangeAsync(List<CourseAiIntegration> integrations);

        /// <summary>
        /// Update an existing integration.
        /// </summary>
        void Update(CourseAiIntegration integration);

        /// <summary>
        /// Delete an integration.
        /// </summary>
        void Delete(CourseAiIntegration integration);

        /// <summary>
        /// Save changes to the database.
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Get integration by ID.
        /// </summary>
        Task<CourseAiIntegration?> GetByIdAsync(int integrationId);

        /// <summary>
        /// Get all integrations for a course.
        /// </summary>
        Task<List<CourseAiIntegration>> GetByCourseIdAsync(int courseId);

        /// <summary>
        /// Get all integrations for a model.
        /// </summary>
        Task<List<CourseAiIntegration>> GetByModelIdAsync(int modelId);

        /// <summary>
        /// Get all integrations.
        /// </summary>
        Task<List<CourseAiIntegration>> GetAllAsync();
    }
}
