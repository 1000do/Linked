using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    /// <summary>
    /// Repository thống nhất cho Instructor (merge InstructorApproval + InstructorService).
    /// Đây là contract duy nhất — Application không biết gì về EF Core (DIP).
    /// </summary>
    public interface IInstructorRepository
    {
        // ── Approval (dùng cho InstructorApprovalService) ──────────────────────
        Task<IEnumerable<Instructor>> GetPendingInstructorsAsync();

        // ── Lookup cơ bản ─────────────────────────────────────────────────────
        Task<Instructor?> GetByIdAsync(int id);
        Task<Instructor?> GetByIdWithNavigationAsync(int instructorId);

        // ── Account ────────────────────────────────────────────────────────────
        Task<Account?> GetAccountByIdAsync(int userId);

        // ── Write ──────────────────────────────────────────────────────────────
        Task AddAsync(Instructor instructor);
        void Update(Instructor instructor);

        // ── Dashboard Queries ──────────────────────────────────────────────────
        Task<List<InstructorDashboardDto>> GetAllApplicationsDtoAsync();
        Task<InstructorDashboardDto?> GetDashboardDtoAsync(int userId);
        Task<InstructorDashboardDto?> GetRejectedApplicationDtoAsync(int userId);
        Task<InstructorStats?> GetStatsAsync(int userId);
        Task<int> CountActiveCoursesAsync(int instructorId);

        // ── Persistence ────────────────────────────────────────────────────────
        Task SaveChangesAsync();
    }
}
