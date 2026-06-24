using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IEmbeddingService
{
    Task<List<MaterialEmbeddingResponse>> GetAllMaterialEmbeddingsAsync();
    Task SaveMaterialEmbeddingsAsync(int materialId, List<float> embedding, string embeddingType);
    Task SaveValidDuplicateEmbeddingsAsync(int courseId, HashSet<int> excludedMaterialIds);
    Task PrepareMaterialEmbeddingsAsync();
    Task PersistMaterialEmbeddingsAsync(int courseId);
}
