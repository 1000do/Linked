using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface ICourseRepository
{
    Task<IEnumerable<Course>> GetInstructorCoursesAsync(int instructorId);
    Task<Course?> GetCourseWithDetailsAsync(int courseId);
    Task<Course?> GetByIdAsync(int courseId);
    Task<bool> HasEnrollmentsAsync(int courseId);
    Task AddAsync(Course course);
    void Update(Course course);
    void Delete(Course course);
    Task SaveChangesAsync();
}
