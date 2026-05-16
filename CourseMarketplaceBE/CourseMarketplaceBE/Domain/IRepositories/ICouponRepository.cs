using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

/// <summary>
/// SOLID — ISP: Interface riêng cho Coupon data access.
/// SOLID — DIP: Service layer chỉ biết interface này, không biết EF Core.
///
/// ★ Tách biệt:
///   - Admin operations: CRUD trên bảng coupons (scoped by managerId)
///   - Instructor operations: Đọc coupon active + gắn coupon vào course
///   - Global read: Đọc coupon không phân biệt manager (cho instructor browse)
/// </summary>
public interface ICouponRepository
{
    // ── Admin CRUD ────────────────────────────────────────────────────────
    Task<List<Coupon>> GetAllAsync(int? managerId, bool? isActive, string? type, string? search);
    Task<Coupon?> GetByIdAsync(int id, int? managerId);
    Task<Coupon?> GetByCodeAsync(string couponCode);
    Task AddAsync(Coupon coupon);
    void Update(Coupon coupon);
    Task SaveChangesAsync();

    // ── Instructor: Browse & Attach ───────────────────────────────────────
    /// <summary>
    /// Lấy danh sách coupon đang hoạt động trên toàn nền tảng:
    /// is_active = true, chưa hết hạn, used_count < usage_limit.
    /// Instructor dùng để chọn gắn vào khóa học.
    /// </summary>
    Task<List<Coupon>> GetActivePlatformCouponsAsync();

    /// <summary>Lấy coupon by ID (không cần managerId — cho instructor dùng).</summary>
    Task<Coupon?> GetByIdGlobalAsync(int couponId);

    /// <summary>Lấy course kèm Instructor navigation.</summary>
    Task<Course?> GetCourseWithInstructorAsync(int courseId);

    /// <summary>Cập nhật course (gắn/gỡ coupon_id).</summary>
    void UpdateCourse(Course course);
}
