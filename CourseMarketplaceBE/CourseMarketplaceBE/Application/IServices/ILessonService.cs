using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Application.IServices;

public interface ILessonService
{
    Task<LessonResponse> CreateLessonAsync(LessonCreateRequest request, int instructorId);
    Task<MaterialResponse> AddMaterialToLessonAsync(int lessonId, MaterialCreateRequest request, int instructorId);
    Task RemoveMaterialAsync(int materialId, int instructorId);
    Task DeleteLessonAsync(int lessonId, int instructorId);
    Task<IEnumerable<MaterialTrashResponse>> GetTrashMaterialsAsync(int instructorId);
    Task PermanentDeleteMaterialAsync(int materialId, int instructorId);
    Task RestoreMaterialAsync(int materialId, int instructorId);
    Task<List<MaterialEmbeddingResponse>> GetAllMaterialEmbeddingsAsync();
    Task SaveMaterialEmbeddingsAsync(int materialId, List<float> embedding);
}
