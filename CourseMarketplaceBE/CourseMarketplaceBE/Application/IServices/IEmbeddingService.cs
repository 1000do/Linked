using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IEmbeddingService
{
    Task<List<MaterialEmbeddingResponse>> GetAllMaterialEmbeddingsAsync();
    Task<int> SaveMaterialEmbeddingsAsync(int materialId, List<float> embedding, string embeddingType);
    Task<int> PersistPendingMaterialEmbeddingsAsync(int courseId, HashSet<int> excludedMaterialIds);
    Task<bool> PrepareMaterialEmbeddingsAsync();
    // Task PersistMaterialEmbeddingsAsync(int courseId);
}
