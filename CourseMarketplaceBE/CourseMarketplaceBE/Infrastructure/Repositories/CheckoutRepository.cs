using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

/// <summary>
/// SOLID — DIP: Implement ICheckoutRepository ở Infrastructure layer.
/// Application layer không bao giờ thấy AppDbContext — chỉ biết interface.
/// </summary>
public class CheckoutRepository : ICheckoutRepository
{
    private readonly AppDbContext _context;

    public CheckoutRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Đọc dữ liệu ─────────────────────────────────────────────────────────

    public async Task<List<CartItem>> GetCartItemsWithCourseAndInstructorAsync(int userId)
        => await _context.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Course)
                .ThenInclude(course => course!.Instructor)
            .OrderBy(c => c.AddedDate)
            .ToListAsync();

    public async Task<Coupon?> GetValidCouponAsync(string couponCode, DateTime now)
        => await _context.Coupons.FirstOrDefaultAsync(cp =>
            cp.CouponCode.ToLower() == couponCode.Trim().ToLower() &&
            cp.IsActive == true &&
            (cp.StartDate == null || cp.StartDate <= now) &&
            (cp.EndDate == null || cp.EndDate >= now) &&
            (cp.UsageLimit == null || (cp.UsedCount ?? 0) < cp.UsageLimit));

    public async Task<bool> IsEnrolledAsync(int userId, int courseId)
        => await _context.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

    public async Task<string?> GetUserEmailAsync(int userId)
        => await _context.Accounts
            .Where(a => a.AccountId == userId)
            .Select(a => a.Email)
            .FirstOrDefaultAsync();

    // ── Tìm transaction theo Stripe Session ID ───────────────────────────────

    public async Task<List<Transaction>> GetTransactionsBySessionIdAsync(string stripeSessionId)
        => await _context.Transactions
            .Where(t => t.StripeSessionId == stripeSessionId)
            .Include(t => t.OrderItem)
                .ThenInclude(oi => oi!.Course)
                    .ThenInclude(c => c!.Instructor)
            .Include(t => t.OrderItem)
                .ThenInclude(oi => oi!.Order)
            .Include(t => t.InstructorPayouts)
            .ToListAsync();

    public async Task<OrderInfo?> GetOrderWithDetailsAsync(int orderId)
        => await _context.OrderInfos
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Course)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

    public async Task<string?> GetInstructorStripeAccountIdAsync(int instructorId)
        => await _context.Instructors
            .Where(i => i.InstructorId == instructorId)
            .Select(i => i.StripeAccountId)
            .FirstOrDefaultAsync();

    // ── Ghi dữ liệu ─────────────────────────────────────────────────────────

    public async Task AddOrderAsync(OrderInfo order)
        => await _context.OrderInfos.AddAsync(order);

    public async Task AddOrderItemAsync(OrderItem item)
        => await _context.OrderItems.AddAsync(item);

    public async Task AddTransactionAsync(Transaction transaction)
        => await _context.Transactions.AddAsync(transaction);

    public async Task AddEnrollmentAsync(Enrollment enrollment)
        => await _context.Enrollments.AddAsync(enrollment);

    public async Task AddEnrollmentProgressAsync(EnrollmentProgress progress)
        => await _context.EnrollmentProgresses.AddAsync(progress);

    public async Task AddInstructorPayoutAsync(InstructorPayout payout)
        => await _context.InstructorPayouts.AddAsync(payout);

    public async Task ClearCartAsync(int userId)
    {
        var items = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();
        _context.CartItems.RemoveRange(items);
    }

    public async Task IncrementCouponUsageAsync(int couponId)
    {
        var coupon = await _context.Coupons.FindAsync(couponId);
        if (coupon != null)
        {
            coupon.UsedCount = (coupon.UsedCount ?? 0) + 1;
        }
    }

    // ── Unit of Work ─────────────────────────────────────────────────────────

    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await _context.Database.BeginTransactionAsync();

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
