using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices;

public interface IEnrollmentService
{
    Task EnrollFreeAsync(int userId, int courseId);
    Task<DTOs.ProgressResponse> GetProgressAsync(int userId, int courseId);
    Task UpdateProgressAsync(int userId, int courseId, int materialId);
}
