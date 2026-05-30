using CourseMarketplaceBE.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface ITextEmbeddingRepository
    {
        Task AddAsync(TextEmbedding embedding);
        void Update(TextEmbedding embedding);
        void Delete(TextEmbedding embedding);
        Task<int> SaveChangesAsync();
        Task<TextEmbedding?> GetByIdAsync(int embeddingId);
        Task<List<TextEmbedding>> GetAllAsync();
        Task<List<TextEmbedding>> GetByMaterialIdAsync(int materialId);
    }
}
