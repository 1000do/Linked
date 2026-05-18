using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

/// <summary>
/// SOLID — ISP: Interface nhỏ, chỉ chứa nghiệp vụ coupon.
///
/// ★ 3 nhóm use-case:
///   1. Admin  — CRUD kho voucher (UC-62, 63, 64, 65)
///   2. Instructor — Browse coupons active + gắn/gỡ coupon vào course (UC-54 Extension)
///   3. Checkout — Tính toán giảm giá (gọi bởi CheckoutService, không expose qua Controller)
/// </summary>
public interface ICouponService
{
    // ── Admin CRUD ────────────────────────────────────────────────────────
    /// <summary>UC-63: Danh sách coupon của manager hiện tại.</summary>
    Task<List<CouponResponse>> GetAll(int managerId, bool? isActive, string? type, string? search, bool isAdmin = false);

    /// <summary>UC-63: Chi tiết 1 coupon.</summary>
    Task<CouponResponse?> GetById(int id, int managerId, bool isAdmin = false);

    /// <summary>UC-62: Tạo mã giảm giá mới.</summary>
    Task Create(CreateCouponRequest request, int managerId);

    /// <summary>
    /// UC-64: Sửa mã — CHỈ cho phép sửa end_date, usage_limit, is_active.
    /// coupon_code, coupon_type, discount_value bị khóa.
    /// </summary>
    Task Update(int id, UpdateCouponRequest request, int managerId, bool isAdmin = false);

    /// <summary>
    /// UC-65: Xóa mềm — set is_active = false.
    /// KHÔNG hard delete nếu used_count > 0 để giữ đối soát kế toán.
    /// </summary>
    Task SoftDelete(int id, int managerId, bool isAdmin = false);

    // ── Instructor: Browse & Attach ───────────────────────────────────────
    /// <summary>
    /// Lấy danh sách mã đang is_active = true, chưa hết hạn, còn slot.
    /// Giảng viên dùng để chọn gắn vào khóa học.
    /// </summary>
    Task<List<CouponResponse>> GetActivePlatformCouponsAsync();

    /// <summary>
    /// Giảng viên gắn 1 coupon vào course.
    /// ★ Bắt buộc kiểm tra JWT: instructorUserId phải là chủ của courseId.
    /// </summary>
    Task ApplyCouponToCourseAsync(int courseId, int couponId, int instructorUserId);

    /// <summary>
    /// Giảng viên gỡ coupon khỏi course (set coupon_id = null).
    /// ★ Bắt buộc kiểm tra JWT ownership.
    /// </summary>
    Task RemoveCouponFromCourseAsync(int courseId, int instructorUserId);

    // ── Checkout (internal) ───────────────────────────────────────────────
    /// <summary>Tính giá sau khi áp dụng coupon. Dùng bởi CheckoutService.</summary>
    decimal ApplyCoupon(Domain.Entities.Coupon coupon, decimal originalPrice);
}
