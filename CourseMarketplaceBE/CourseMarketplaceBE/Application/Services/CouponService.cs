using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

/// <summary>
/// SOLID — SRP: Chỉ điều phối nghiệp vụ coupon.
///   - Không biết EF Core (phụ thuộc ICouponRepository — DIP).
///   - Không xử lý HTTP (Controller lo — SRP).
///
/// ★ Business Rules quan trọng:
///   1. Create: coupon_code UNIQUE, used_count = 0, is_active = true mặc định.
///   2. Update: CHỈ cho sửa end_date, usage_limit, is_active.
///              coupon_code, coupon_type, discount_value BỊ KHÓA.
///   3. Delete: LUÔN soft-delete (is_active = false).
///              KHÔNG hard delete nếu used_count > 0.
///   4. Instructor: Browse coupons active + gắn/gỡ coupon vào course.
///                  JWT ownership: instructorUserId PHẢI là chủ khóa học.
/// </summary>
public class CouponService : ICouponService
{
    private readonly ICouponRepository _repo;
    private readonly IRedisService _redisService;

    public CouponService(ICouponRepository repo, IRedisService redisService)
    {
        _repo = repo;
        _redisService = redisService;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static string NormalizeType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("CouponType is required (fixed | percentage).");

        type = type.Trim().ToLower();

        return type switch
        {
            "fixed" or "amount" => "fixed",
            "percent" or "percentage" => "percentage",
            _ => throw new ArgumentException("Invalid CouponType. Only 'fixed' or 'percentage' are allowed.")
        };
    }

    private static void ValidateCreate(string type, decimal value, DateTime? start, DateTime? end, int? usageLimit)
    {
        if (value <= 0)
            throw new ArgumentException("DiscountValue must be greater than 0.");

        if (type == "percentage" && value > 100)
            throw new ArgumentException("Discount percentage cannot exceed 100%.");

        if (start.HasValue && end.HasValue && start > end)
            throw new ArgumentException("StartDate must be before EndDate.");

        if (usageLimit.HasValue && usageLimit < 1)
            throw new ArgumentException("UsageLimit must be >= 1.");
    }

    private static CouponResponse MapToResponse(Coupon x) => new()
    {
        CouponId      = x.CouponId,
        CouponCode    = x.CouponCode,
        CouponType    = x.CouponType ?? "fixed",
        DiscountValue = x.DiscountValue,
        MinOrderValue = x.MinOrderValue,
        StartDate     = x.StartDate,
        EndDate       = x.EndDate,
        UsageLimit    = x.UsageLimit,
        UsedCount     = x.UsedCount,
        IsActive      = x.IsActive
    };

    // ═══════════════════════════════════════════════════════════════════════
    // UC-63: DANH SÁCH COUPON (ADMIN)
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<List<CouponResponse>> GetAll(int managerId, bool? isActive, string? type, string? search, bool isAdmin = false)
    {
        // Nếu là Admin thì lấy tất cả (managerId = null)
        int? filterId = isAdmin ? null : managerId;
        var data = await _repo.GetAllAsync(filterId, isActive, type, search);
        return data.Select(MapToResponse).ToList();
    }

    public async Task<CouponResponse?> GetById(int id, int managerId, bool isAdmin = false)
    {
        int? filterId = isAdmin ? null : managerId;
        var x = await _repo.GetByIdAsync(id, filterId);
        return x == null ? null : MapToResponse(x);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // UC-62: TẠO MÃ GIẢM GIÁ (ADMIN)
    // ═══════════════════════════════════════════════════════════════════════

    public async Task Create(CreateCouponRequest req, int managerId)
    {
        var type = NormalizeType(req.CouponType);
        ValidateCreate(type, req.DiscountValue, req.StartDate, req.EndDate, req.UsageLimit);

        // ★ Kiểm tra coupon_code UNIQUE
        var existing = await _repo.GetByCodeAsync(req.CouponCode.Trim());
        if (existing != null)
            throw new InvalidOperationException($"Coupon code '{req.CouponCode.Trim()}' already exists.");

        var coupon = new Coupon
        {
            CouponCode    = req.CouponCode.Trim().ToUpper(),
            CouponType    = type,
            DiscountValue = req.DiscountValue,
            MinOrderValue = req.MinOrderValue < 0 ? 0 : req.MinOrderValue,
            StartDate     = req.StartDate,
            EndDate       = req.EndDate,
            UsageLimit    = req.UsageLimit,
            UsedCount     = 0,       // ★ Luôn khởi tạo = 0
            IsActive      = true,    // ★ Luôn active khi tạo mới
            ManagerId     = managerId
        };

        await _repo.AddAsync(coupon);
        await _repo.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // UC-64: SỬA MÃ GIẢM GIÁ (ADMIN)
    //
    // ★ CHỈ cho phép sửa: end_date, usage_limit, is_active
    // ★ KHÓA: coupon_code, coupon_type, discount_value, min_order_value
    // ═══════════════════════════════════════════════════════════════════════

    public async Task Update(int id, UpdateCouponRequest req, int managerId, bool isAdmin = false)
    {
        int? filterId = isAdmin ? null : managerId;
        var coupon = await _repo.GetByIdAsync(id, filterId);
        if (coupon == null)
            throw new KeyNotFoundException($"Coupon #{id} not found.");

        // Chỉ sửa end_date nếu được truyền
        if (req.EndDate.HasValue)
        {
            if (coupon.StartDate.HasValue && req.EndDate < coupon.StartDate)
                throw new ArgumentException("EndDate must be after StartDate.");
            coupon.EndDate = req.EndDate;
        }

        // Chỉ sửa usage_limit nếu được truyền
        if (req.UsageLimit.HasValue)
        {
            if (req.UsageLimit < (coupon.UsedCount ?? 0))
                throw new ArgumentException(
                    $"UsageLimit ({req.UsageLimit}) cannot be less than the used count ({coupon.UsedCount}).");
            coupon.UsageLimit = req.UsageLimit;
        }

        // Bật/tắt trạng thái
        if (req.IsActive.HasValue)
            coupon.IsActive = req.IsActive;

        _repo.Update(coupon);
        await _repo.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // UC-65: XÓA MỀM (ADMIN)
    //
    // ★ Nếu used_count > 0: CHỈ soft-delete (is_active = false)
    //   Tuyệt đối KHÔNG hard delete để giữ đối soát kế toán.
    // ★ Nếu used_count == 0: Vẫn chỉ soft-delete cho nhất quán.
    // ═══════════════════════════════════════════════════════════════════════

    public async Task SoftDelete(int id, int managerId, bool isAdmin = false)
    {
        int? filterId = isAdmin ? null : managerId;
        var coupon = await _repo.GetByIdAsync(id, filterId);
        if (coupon == null)
            throw new KeyNotFoundException($"Coupon #{id} not found.");

        // ★ Luôn soft-delete: set is_active = false + end_date = hôm qua
        coupon.IsActive = false;
        coupon.EndDate = DateTime.Now.AddDays(-1); // Hết hạn ngay lập tức

        _repo.Update(coupon);
        await _repo.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // INSTRUCTOR: Browse coupons active trên nền tảng
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<List<CouponResponse>> GetActivePlatformCouponsAsync()
    {
        var data = await _repo.GetActivePlatformCouponsAsync();
        return data.Select(MapToResponse).ToList();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // INSTRUCTOR: Gắn coupon vào khóa học
    //
    // ★ Kiểm tra JWT ownership: instructorUserId PHẢI là chủ khóa học
    // ★ Kiểm tra coupon vẫn còn hợp lệ (active, chưa hết hạn, còn slot)
    // ═══════════════════════════════════════════════════════════════════════

    public async Task ApplyCouponToCourseAsync(int courseId, int couponId, int instructorUserId)
    {
        // 1. Kiểm tra khóa học tồn tại + JWT ownership
        var course = await _repo.GetCourseWithInstructorAsync(courseId);
        if (course == null)
            throw new KeyNotFoundException($"Course #{courseId} not found.");

        if (course.Instructor == null || course.Instructor.InstructorId != instructorUserId)
            throw new UnauthorizedAccessException("You are not the owner of this course.");

        if (course.Price == 0)
            throw new InvalidOperationException("Coupons cannot be applied to free courses.");

        // 2. Kiểm tra coupon tồn tại
        var coupon = await _repo.GetByIdGlobalAsync(couponId);
        if (coupon == null)
            throw new KeyNotFoundException($"Coupon #{couponId} not found.");

        // 3. Kiểm tra coupon còn hợp lệ
        var now = DateTime.UtcNow;
        if (coupon.IsActive != true)
            throw new InvalidOperationException("This coupon has been disabled.");

        if (coupon.EndDate.HasValue && coupon.EndDate < now)
            throw new InvalidOperationException("This coupon has expired.");

        if (coupon.StartDate.HasValue && coupon.StartDate > now)
            throw new InvalidOperationException("This coupon has not started yet.");

        if (coupon.UsageLimit.HasValue && (coupon.UsedCount ?? 0) >= coupon.UsageLimit)
            throw new InvalidOperationException("This coupon usage limit has been reached.");

        // 4. Gắn coupon vào course
        course.CouponId = couponId;
        _repo.UpdateCourse(course);
        await _repo.SaveChangesAsync();

        // 5. Invalidate Cache
        await _redisService.RemoveCacheAsync($"course:detail:{courseId}");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // INSTRUCTOR: Gỡ coupon khỏi khóa học
    // ═══════════════════════════════════════════════════════════════════════

    public async Task RemoveCouponFromCourseAsync(int courseId, int instructorUserId)
    {
        var course = await _repo.GetCourseWithInstructorAsync(courseId);
        if (course == null)
            throw new KeyNotFoundException($"Course #{courseId} not found.");

        if (course.Instructor == null || course.Instructor.InstructorId != instructorUserId)
            throw new UnauthorizedAccessException("You are not the owner of this course.");

        course.CouponId = null;
        _repo.UpdateCourse(course);
        await _repo.SaveChangesAsync();

        // Invalidate Cache
        await _redisService.RemoveCacheAsync($"course:detail:{courseId}");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // CHECKOUT: Tính giá sau coupon (gọi bởi CheckoutService)
    // ═══════════════════════════════════════════════════════════════════════

    public decimal ApplyCoupon(Coupon coupon, decimal originalPrice)
    {
        if (coupon.IsActive != true)
            throw new InvalidOperationException("Coupon is not active.");

        if (coupon.StartDate.HasValue && DateTime.UtcNow < coupon.StartDate)
            throw new InvalidOperationException("Coupon has not started yet.");

        if (coupon.EndDate.HasValue && DateTime.UtcNow > coupon.EndDate)
            throw new InvalidOperationException("Coupon has expired.");

        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit)
            throw new InvalidOperationException("Coupon usage limit has been reached.");

        if (coupon.MinOrderValue > 0 && originalPrice < coupon.MinOrderValue)
            throw new InvalidOperationException(
                $"Minimum order value required to use this coupon is {coupon.MinOrderValue:N0} VND.");

        var finalPrice = coupon.CouponType == "percentage"
            ? originalPrice - (originalPrice * coupon.DiscountValue / 100)
            : originalPrice - coupon.DiscountValue;

        return Math.Max(finalPrice, 0);
    }
}
