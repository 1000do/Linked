using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

/// <summary>
/// Repository Interface cho nghiệp vụ Giỏ hàng (SOLID - DIP).
/// CartService chỉ phụ thuộc interface này, không biết gì về EF Core.
/// </summary>
public interface ICartRepository
{
    // ── Cart Items ────────────────────────────────────────────────────────────
    /// <summary>Lấy tất cả cart items của user, kèm Course + Instructor navigation.</summary>
    Task<List<CartItem>> GetCartItemsWithDetailsAsync(int userId);
    Task<bool> IsCourseInCartAsync(int userId, int courseId);
    Task AddCartItemAsync(CartItem item);
    Task<CartItem?> GetCartItemAsync(int userId, int courseId);
    void RemoveCartItem(CartItem item);

    // ── Course / Enrollment (kiểm tra điều kiện) ─────────────────────────────
    Task<Course?> GetPublishedCourseAsync(int courseId);
    Task<bool> IsEnrolledAsync(int userId, int courseId);

    // ── Coupon ────────────────────────────────────────────────────────────────
    Task<Coupon?> GetCouponByCodeAsync(string couponCode);
    Task<List<Coupon>> GetActiveAvailableCouponsAsync(DateTime now);

    // ── Persistence ────────────────────────────────────────────────────────────
    Task SaveChangesAsync();
}
