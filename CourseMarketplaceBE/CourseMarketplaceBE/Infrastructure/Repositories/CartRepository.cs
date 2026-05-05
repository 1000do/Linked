using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

/// <summary>
/// Cài đặt ICartRepository ở Infrastructure layer.
/// AppDbContext được đóng gói hoàn toàn tại đây, Application không bao giờ thấy nó.
/// </summary>
public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Cart Items ────────────────────────────────────────────────────────────

    public async Task<List<CartItem>> GetCartItemsWithDetailsAsync(int userId)
        => await _context.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Course)
                .ThenInclude(course => course!.Instructor)
                    .ThenInclude(i => i!.InstructorNavigation)
            .OrderBy(c => c.AddedDate)
            .ToListAsync();

    public async Task<bool> IsCourseInCartAsync(int userId, int courseId)
        => await _context.CartItems.AnyAsync(c => c.UserId == userId && c.CourseId == courseId);

    public async Task AddCartItemAsync(CartItem item)
        => await _context.CartItems.AddAsync(item);

    public async Task<CartItem?> GetCartItemAsync(int userId, int courseId)
        => await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId);

    public void RemoveCartItem(CartItem item)
        => _context.CartItems.Remove(item);

    // ── Course / Enrollment ────────────────────────────────────────────────────

    public async Task<Course?> GetPublishedCourseAsync(int courseId)
        => await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId && c.CourseStatus == "published");

    public async Task<bool> IsEnrolledAsync(int userId, int courseId)
        => await _context.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

    // ── Coupon ────────────────────────────────────────────────────────────────

    public async Task<Coupon?> GetCouponByCodeAsync(string couponCode)
        => await _context.Coupons
            .FirstOrDefaultAsync(cp => cp.CouponCode.ToLower() == couponCode.Trim().ToLower());

    public async Task<List<Coupon>> GetActiveAvailableCouponsAsync(DateTime now)
        => await _context.Coupons
            .Where(cp =>
                cp.IsActive == true &&
                (cp.StartDate == null || cp.StartDate <= now) &&
                (cp.EndDate == null || cp.EndDate >= now) &&
                (cp.UsageLimit == null || (cp.UsedCount ?? 0) < cp.UsageLimit))
            .OrderBy(cp => cp.MinOrderValue)
            .ToListAsync();

    // ── Persistence ────────────────────────────────────────────────────────────

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
