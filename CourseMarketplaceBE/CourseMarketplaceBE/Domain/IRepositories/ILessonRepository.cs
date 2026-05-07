using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdAsync(int lessonId);
    Task AddAsync(Lesson lesson);
    void Update(Lesson lesson);
    void Delete(Lesson lesson);
    Task<List<Lesson>> GetByCourseIdAsync(int courseId);
    Task SaveChangesAsync();
}
