using CourseMarketplaceBE.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface IMediaEmbeddingRepository
    {
        Task AddAsync(MediaEmbedding embedding);
        void Update(MediaEmbedding embedding);
        void Delete(MediaEmbedding embedding);
        Task<int> SaveChangesAsync();
        Task<MediaEmbedding?> GetByIdAsync(int embeddingId);
        Task<List<MediaEmbedding>> GetAllAsync();
        Task<List<MediaEmbedding>> GetByMaterialIdAsync(int materialId);
    }
}
