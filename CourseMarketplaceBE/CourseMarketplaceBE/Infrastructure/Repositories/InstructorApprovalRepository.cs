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

        public async Task<(IEnumerable<Instructor> Items, int TotalCount)> GetPendingInstructorsAsync(int page = 1, int pageSize = 10)
        {
            var query = _context.Instructors
                .Include(i => i.InstructorNavigation)
                    .ThenInclude(u => u!.UserNavigation)
                .Where(i => i.ApprovalStatus != null && i.ApprovalStatus.ToLower() == "pending");
                
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return (items, totalCount);
        }

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
                    LinkedinUrl            = i.LinkedinUrl,
                    YoutubeUrl             = i.YoutubeUrl,
                    FacebookUrl            = i.FacebookUrl,
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
                    LinkedinUrl            = i.LinkedinUrl,
                    YoutubeUrl             = i.YoutubeUrl,
                    FacebookUrl            = i.FacebookUrl,
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
                    YoutubeUrl          = i.YoutubeUrl,
                    FacebookUrl         = i.FacebookUrl,
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

        public async Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseMarketplaceBE.Application.DTOs.InstructorPayoutDto>> GetPayoutsAsync(int instructorId, int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = "date_desc", string? status = null, int? year = null, int? month = null)
        {
            var query = _context.InstructorPayouts
                .Where(p => p.InstructorId == instructorId)
                .Include(p => p.Transaction)
                    .ThenInclude(t => t!.OrderItem)
                        .ThenInclude(oi => oi!.Course)
                .AsQueryable();

            if (year.HasValue && month.HasValue)
            {
                var startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddMonths(1);
                query = query.Where(p => p.Transaction != null && p.Transaction.TransactionCreatedAt >= startDate && p.Transaction.TransactionCreatedAt < endDate);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(p => 
                    (p.Transaction != null && p.Transaction.OrderItem != null && p.Transaction.OrderItem.Course != null && p.Transaction.OrderItem.Course.Title != null && p.Transaction.OrderItem.Course.Title.ToLower().Contains(lowerKeyword)) ||
                    (p.StripePayoutId != null && p.StripePayoutId.ToLower().Contains(lowerKeyword)) ||
                    (p.StripeTransferId != null && p.StripeTransferId.ToLower().Contains(lowerKeyword))
                );
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = status.ToLower() switch
                {
                    "succeeded" => query.Where(p => p.PayoutStatus == "paid" || p.PayoutStatus == "transferred"),
                    "pending" => query.Where(p => p.PayoutStatus == "pending" || p.PayoutStatus == "in_transit"),
                    "failed" => query.Where(p => p.PayoutStatus == "failed" || p.PayoutStatus == "canceled"),
                    _ => query.Where(p => p.PayoutStatus == status)
                };
            }

            query = sortBy switch
            {
                "date_asc" => query.OrderBy(p => p.PayoutDate),
                "amount_desc" => query.OrderByDescending(p => p.PayoutAmount),
                "amount_asc" => query.OrderBy(p => p.PayoutAmount),
                _ => query.OrderByDescending(p => p.PayoutDate)
            };

            int totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new InstructorPayoutDto
                {
                    PayoutId = p.PayoutId,
                    Amount = p.PayoutAmount,
                    PayoutDate = p.PayoutDate,
                    IsPaid = p.IsPaid,
                    CourseTitle = p.Transaction!.OrderItem!.Course!.Title ?? "N/A",
                    TotalAmount = p.Transaction.Amount,
                    PayoutStatus = p.PayoutStatus,
                    PaidToBankAt = p.PaidToBankAt,
                    StripeTransferId = p.StripeTransferId,
                    StripePayoutId = p.StripePayoutId
                })
                .ToListAsync();

            return new CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseMarketplaceBE.Application.DTOs.InstructorPayoutDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<int> GetTotalApprovedInstructorsCountAsync()
            => await _context.Instructors.CountAsync(i => i.ApprovalStatus == "Approved");

        public async Task<List<Instructor>> GetInstructorsWithStripeAsync()
        {
            return await _context.Instructors
                .Where(i => !string.IsNullOrEmpty(i.StripeAccountId))
                .ToListAsync();
        }

        // ── Persistence ───────────────────────────────────────────────────────

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
