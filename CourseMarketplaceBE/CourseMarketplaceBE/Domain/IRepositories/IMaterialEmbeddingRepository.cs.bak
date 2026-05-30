using CourseMarketplaceBE.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    /// <summary>
    /// Repository interface for MaterialEmbedding entity.
    /// </summary>
    public interface IMaterialEmbeddingRepository
    {
        /// <summary>
        /// Add a new material embedding.
        /// </summary>
        Task AddAsync(MaterialEmbedding embedding);

        /// <summary>
        /// Update an existing material embedding.
        /// </summary>
        void Update(MaterialEmbedding embedding);

        /// <summary>
        /// Delete a material embedding.
        /// </summary>
        void Delete(MaterialEmbedding embedding);

        /// <summary>
        /// Save changes to the database.
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Get material embedding by ID.
        /// </summary>
        Task<MaterialEmbedding?> GetByIdAsync(int embeddingId);

        /// <summary>
        /// Get all material embeddings.
        /// </summary>
        Task<List<MaterialEmbedding>> GetAllAsync();

        /// <summary>
        /// Get embeddings for a specific material.
        /// </summary>
        Task<List<MaterialEmbedding>> GetByMaterialIdAsync(int materialId);
    }
}
