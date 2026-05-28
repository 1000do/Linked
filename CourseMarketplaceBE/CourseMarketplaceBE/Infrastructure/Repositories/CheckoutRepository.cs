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

    public async Task<Course?> GetCourseWithInstructorAsync(int courseId)
        => await _context.Courses
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

    public async Task<Coupon?> GetValidCouponAsync(string couponCode, DateTime now)
        => await _context.Coupons.FirstOrDefaultAsync(cp =>
            cp.CouponCode.ToLower() == couponCode.Trim().ToLower() &&
            cp.IsActive == true &&
            (cp.StartDate == null || cp.StartDate <= now) &&
            (cp.EndDate == null || cp.EndDate >= now) &&
            (cp.UsageLimit == null || (cp.UsedCount ?? 0) < cp.UsageLimit));

    public async Task<bool> IsEnrolledAsync(int userId, int courseId)
        => await _context.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

    public async Task<Enrollment?> GetEnrollmentAsync(int userId, int courseId)
        => await _context.Enrollments
            .Include(e => e.Progress)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

    public async Task<Enrollment?> GetEnrollmentWithProgressAsync(int userId, int courseId)
        => await _context.Enrollments
            .Include(e => e.Progress)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

    public async Task<List<Enrollment>> GetMyEnrolledCoursesAsync(int userId)
    {
        return await _context.Enrollments
            .Where(e => e.UserId == userId)
            .Include(e => e.Progress)
            .Include(e => e.Course)
                .ThenInclude(c => c!.Instructor)
                    .ThenInclude(i => i!.InstructorNavigation)
            .OrderByDescending(e => e.LastAccessedAt)
            .ToListAsync();
    }
    public async Task<bool> IsMaterialCompletedAsync(int enrollmentId, int materialId)
        => await _context.MaterialCompletions.AnyAsync(mc => mc.EnrollmentId == enrollmentId && mc.MaterialId == materialId);

    public async Task<int> GetCompletedMaterialCountAsync(int enrollmentId)
        => await _context.MaterialCompletions.CountAsync(mc => mc.EnrollmentId == enrollmentId);

    public async Task<List<int>> GetCompletedMaterialIdsAsync(int enrollmentId)
        => await _context.MaterialCompletions
            .Where(mc => mc.EnrollmentId == enrollmentId)
            .Select(mc => mc.MaterialId)
            .ToListAsync();

    public async Task<List<int>> GetEnrolledUserIdsAsync(int courseId)
    {
        return await _context.Enrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => e.UserId ?? 0)
            .Where(id => id != 0)
            .Distinct()
            .ToListAsync();
    }

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

    public async Task<List<Transaction>> GetTransactionsByOrderIdAsync(int orderId)
        => await _context.Transactions
            .Where(t => t.OrderItem != null && t.OrderItem.OrderId == orderId)
            .Include(t => t.OrderItem)
                .ThenInclude(oi => oi!.Order)
            .ToListAsync();

    public async Task<OrderInfo?> GetOrderWithDetailsAsync(int orderId)
        => await _context.OrderInfos
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Course)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

    public async Task<List<OrderInfo>> GetPendingOrdersByUserAsync(int userId)
        => await _context.OrderInfos
            .Where(o => o.UserId == userId && o.OrderStatus == "pending")
            .Include(o => o.OrderItems)
            .ToListAsync();

    public async Task<string?> GetInstructorStripeAccountIdAsync(int instructorId)
        => await _context.Instructors
            .Where(i => i.InstructorId == instructorId)
            .Select(i => i.StripeAccountId)
            .FirstOrDefaultAsync();

    public async Task<string?> GetInstructorStripeCountryAsync(int instructorId)
        => await _context.Instructors
            .Where(i => i.InstructorId == instructorId)
            .Select(i => i.StripeCountry)
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

    public async Task AddMaterialCompletionAsync(MaterialCompletion completion)
        => await _context.MaterialCompletions.AddAsync(completion);

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

    public Task DeleteOrderAsync(OrderInfo order)
    {
        // Xóa OrderItems trước do foreign key (nếu DB không cấu hình cascade delete)
        if (order.OrderItems != null && order.OrderItems.Any())
        {
            _context.OrderItems.RemoveRange(order.OrderItems);
        }
        _context.OrderInfos.Remove(order);
        return Task.CompletedTask;
    }

    public Task DeleteTransactionsAsync(IEnumerable<Transaction> transactions)
    {
        _context.Transactions.RemoveRange(transactions);
        return Task.CompletedTask;
    }

    // ── Unit of Work ─────────────────────────────────────────────────────────

    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await _context.Database.BeginTransactionAsync();

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
