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

        /// <summary>
        /// Get all active models that match any of the given paths.
        /// </summary>
        Task<List<AiModel>> GetActiveModelsByPathsAsync(List<string> paths);

        /// <summary>
        /// Get models by type.
        /// </summary>
        Task<List<AiModel>> GetModelsByTypeAsync(string modelType);

        /// <summary>
        /// Check if an AI model with the given name exists.
        /// </summary>
        Task<bool> ExistsByModelNameAsync(string modelName);

        /// <summary>
        /// Check if an AI model with the given path exists.
        /// </summary>
        Task<bool> ExistsByModelPathAsync(string modelPath);

        /// <summary>
        /// Retrieve AI model metadata mapped as DTO using model path.
        /// </summary>
        Task<CourseMarketplaceBE.Application.DTOs.AiModelDto?> GetByModelPathAsync(string modelPath);

        /// <summary>
        /// Adds a new AI model to the database tracking.
        /// </summary>
        AiModel Add(AiModel model);

        /// <summary>
        /// Updates an existing AI model tracking.
        /// </summary>
        AiModel Update(AiModel model);

        /// <summary>
        /// Removes an existing AI model tracking.
        /// </summary>
        void Remove(AiModel model);

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        Task<int> SaveChangesAsync();

        Task<List<AiModel>> GetAllAdminAsync();

        /// <summary>
        /// Gets all AI models with pagination.
        /// </summary>
        Task<(List<AiModel> Items, int TotalCount)> GetPagedAdminAsync(int page, int pageSize);
    }
}
