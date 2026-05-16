using CourseMarketplaceBE.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    /// <summary>
    /// Repository interface for AiModel entity.
    /// </summary>
    public interface IAiModelRepository
    {
        /// <summary>
        /// Get model IDs by type and status.
        /// </summary>
        Task<List<int>> GetModelIdsByTypeAsync(string modelType);

        /// <summary>
        /// Get AI model by ID.
        /// </summary>
        Task<AiModel?> GetByIdAsync(int modelId);

        /// <summary>
        /// Get all active models of a specific type.
        /// </summary>
        Task<List<AiModel>> GetByTypeAsync(string modelType);
    }
}
