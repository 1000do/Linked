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
                .Where(i => i.ApprovalStatus != null && i.ApprovalStatus.ToLower() == "pending")
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
                    StripeCountry          = i.StripeCountry,
                    FullName               = i.InstructorNavigation != null ? i.InstructorNavigation.FullName : "N/A",
                    Email                  = i.InstructorNavigation != null && i.InstructorNavigation.UserNavigation != null 
                                             ? i.InstructorNavigation.UserNavigation.Email : "N/A"
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
                    StripeCountry          = i.StripeCountry,
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
        {
            var stats = await _context.InstructorStats.FirstOrDefaultAsync(s => s.InstructorId == userId);
            if (stats != null)
            {
                // Check if instructor is enrolled in any of their own courses
                var instructorOwnCourseIds = await _context.Courses
                    .Where(c => c.InstructorId == userId)
                    .Select(c => c.CourseId)
                    .ToListAsync();
                
                if (instructorOwnCourseIds.Any())
                {
                    var isEnrolledInAny = await _context.Enrollments
                        .AnyAsync(e => e.UserId == userId && e.CourseId.HasValue && instructorOwnCourseIds.Contains(e.CourseId.Value));
                    
                    if (isEnrolledInAny)
                    {
                        stats.TotalStudentsCount = Math.Max(0, stats.TotalStudentsCount - 1);
                    }
                }
            }
            return stats;
        }

        public async Task<int> CountActiveCoursesAsync(int instructorId)
            => await _context.Courses.CountAsync(c => c.InstructorId == instructorId && c.CourseStatus == "published");

        public async Task<List<InstructorPayoutDto>> GetPayoutsAsync(int instructorId)
        {
            return await _context.InstructorPayouts
                .Where(p => p.InstructorId == instructorId)
                .Include(p => p.Transaction)
                    .ThenInclude(t => t!.OrderItem)
                        .ThenInclude(oi => oi!.Course)
                .OrderByDescending(p => p.PayoutDate)
                .Select(p => new InstructorPayoutDto
                {
                    PayoutId = p.PayoutId,
                    Amount = p.PayoutAmount,
                    PayoutDate = p.PayoutDate,
                    IsPaid = p.IsPaid,
                    CourseTitle = p.Transaction!.OrderItem!.Course!.Title ?? "N/A",
                    TotalAmount = p.Transaction.Amount
                })
                .ToListAsync();
        }

        public async Task<int> GetTotalApprovedInstructorsCountAsync()
            => await _context.Instructors.CountAsync(i => i.ApprovalStatus == "Approved");

        // ── Persistence ───────────────────────────────────────────────────────

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
