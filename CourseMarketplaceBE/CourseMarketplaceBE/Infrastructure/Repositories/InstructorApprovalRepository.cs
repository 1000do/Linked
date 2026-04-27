using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    /// <summary>
    /// Repository thống nhất cho Instructor. AppDbContext chỉ xuất hiện tại Infrastructure.
    /// </summary>
    public class InstructorRepository : IInstructorRepository
    {
        private readonly AppDbContext _context;

        public InstructorRepository(AppDbContext context) => _context = context;

        // ── Approval ──────────────────────────────────────────────────────────

        public async Task<IEnumerable<Instructor>> GetPendingInstructorsAsync()
            => await _context.Instructors
                .Include(i => i.InstructorNavigation)
                    .ThenInclude(u => u!.UserNavigation)
                .Where(i => i.ApprovalStatus == "Pending")
                .ToListAsync();

        // ── Lookup ────────────────────────────────────────────────────────────

        public async Task<Instructor?> GetByIdAsync(int id)
            => await _context.Instructors
                .Include(i => i.InstructorNavigation)
                    .ThenInclude(u => u!.UserNavigation)
                .FirstOrDefaultAsync(i => i.InstructorId == id);

        public async Task<Instructor?> GetByIdWithNavigationAsync(int instructorId)
            => await _context.Instructors
                .Include(i => i.InstructorNavigation)
                    .ThenInclude(u => u!.UserNavigation)
                .FirstOrDefaultAsync(i => i.InstructorId == instructorId);

        // ── Account ───────────────────────────────────────────────────────────

        public async Task<Account?> GetAccountByIdAsync(int userId)
            => await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == userId);

        // ── Write ─────────────────────────────────────────────────────────────

        public async Task AddAsync(Instructor instructor)
            => await _context.Instructors.AddAsync(instructor);

        public void Update(Instructor instructor)
            => _context.Instructors.Update(instructor);

        // ── Dashboard Queries ─────────────────────────────────────────────────

        public async Task<List<InstructorDashboardDto>> GetAllApplicationsDtoAsync()
            => await _context.Instructors
                .Include(i => i.InstructorNavigation)
                    .ThenInclude(u => u!.UserNavigation)
                .Select(i => new InstructorDashboardDto
                {
                    InstructorId           = i.InstructorId,
                    ProfessionalTitle      = i.ProfessionalTitle,
                    ExpertiseCategories    = i.ExpertiseCategories,
                    ApprovalStatus         = i.ApprovalStatus,
                    StripeAccountId        = i.StripeAccountId,
                    StripeOnboardingStatus = i.StripeOnboardingStatus,
                    PayoutsEnabled         = i.PayoutsEnabled ?? false,
                    ChargesEnabled         = i.ChargesEnabled ?? false,
                    FullName               = i.InstructorNavigation!.FullName,
                    Email                  = i.InstructorNavigation.UserNavigation!.Email
                })
                .ToListAsync();

        public async Task<InstructorDashboardDto?> GetDashboardDtoAsync(int userId)
            => await _context.Instructors
                .Include(i => i.InstructorNavigation)
                    .ThenInclude(u => u!.UserNavigation)
                .Where(i => i.InstructorId == userId)
                .Select(i => new InstructorDashboardDto
                {
                    InstructorId           = i.InstructorId,
                    ProfessionalTitle      = i.ProfessionalTitle,
                    ExpertiseCategories    = i.ExpertiseCategories,
                    ApprovalStatus         = i.ApprovalStatus,
                    StripeAccountId        = i.StripeAccountId,
                    StripeOnboardingStatus = i.StripeOnboardingStatus,
                    PayoutsEnabled         = i.PayoutsEnabled ?? false,
                    ChargesEnabled         = i.ChargesEnabled ?? false,
                    FullName               = i.InstructorNavigation!.FullName,
                    Email                  = i.InstructorNavigation.UserNavigation!.Email
                })
                .FirstOrDefaultAsync();

        public async Task<InstructorDashboardDto?> GetRejectedApplicationDtoAsync(int userId)
            => await _context.Instructors
                .Include(i => i.InstructorNavigation)
                    .ThenInclude(u => u!.UserNavigation)
                .Where(i => i.InstructorId == userId && i.ApprovalStatus == "Rejected")
                .Select(i => new InstructorDashboardDto
                {
                    InstructorId        = i.InstructorId,
                    ProfessionalTitle   = i.ProfessionalTitle,
                    ExpertiseCategories = i.ExpertiseCategories,
                    LinkedinUrl         = i.LinkedinUrl,
                    DocumentUrl         = i.DocumentUrl,
                    ApprovalStatus      = i.ApprovalStatus,
                    FullName            = i.InstructorNavigation!.FullName,
                    Email               = i.InstructorNavigation.UserNavigation!.Email
                })
                .FirstOrDefaultAsync();

        public async Task<InstructorStats?> GetStatsAsync(int userId)
            => await _context.InstructorStats.FirstOrDefaultAsync(s => s.InstructorId == userId);

        public async Task<int> CountActiveCoursesAsync(int instructorId)
            => await _context.Courses.CountAsync(c => c.InstructorId == instructorId && c.CourseStatus == "published");

        // ── Persistence ───────────────────────────────────────────────────────

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
