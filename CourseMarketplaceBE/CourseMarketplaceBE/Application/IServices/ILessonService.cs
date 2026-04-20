using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface ILessonService
{
    Task<LessonResponse> CreateLessonAsync(LessonCreateRequest request, int instructorId);
    Task<MaterialResponse> AddMaterialToLessonAsync(int lessonId, MaterialCreateRequest request, int instructorId);
    Task DeleteLessonAsync(int lessonId, int instructorId);
}
