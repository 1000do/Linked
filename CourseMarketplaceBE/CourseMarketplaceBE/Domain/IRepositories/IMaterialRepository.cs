using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IMaterialRepository
{
    Task<LearningMaterial?> GetByIdAsync(int materialId);
    Task<List<LearningMaterial>> GetMaterialsByLessonIdAsync(int lessonId);
    Task AddAsync(LearningMaterial material);
    void Update(LearningMaterial material);
    void Delete(LearningMaterial material);
    Task SaveChangesAsync();
}
