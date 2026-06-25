using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface IEnrollmentRepository
    {
        Task<Enrollment?> GetEnrollmentAsync(int userId, int courseId);
        Task<Enrollment?> GetEnrollmentWithProgressAsync(int userId, int courseId);
        Task<Enrollment?> GetEnrollmentIncludingRefundedAsync(int userId, int courseId);
        Task ClearMaterialCompletionsAsync(int enrollmentId);
        Task<List<Enrollment>> GetMyEnrolledCoursesAsync(int userId);
        Task<bool> IsMaterialCompletedAsync(int enrollmentId, int materialId);
        Task<int> GetCompletedMaterialCountAsync(int enrollmentId);
        Task<List<int>> GetCompletedMaterialIdsAsync(int enrollmentId);
        Task<List<int>> GetEnrolledUserIdsAsync(int courseId);

        Task AddEnrollmentAsync(Enrollment enrollment);
        
        Task AddMaterialCompletionAsync(MaterialCompletion completion);

        Task<bool> IsUserEnrolledInAnyCoursWithQuizAsync(int userId, int quizId);

        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();
        Task<int> SaveChangesAsync();
    }
}
