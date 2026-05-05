using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

/// <summary>
/// Service xử lý Giỏ hàng và Mã giảm giá.
/// Tuân thủ DIP (SOLID): chỉ phụ thuộc ICartRepository.
/// AppDbContext bị đóng gói hoàn toàn ở Infrastructure layer.
/// - userId luôn được lấy từ JWT Claim bởi Controller, tuyệt đối KHÔNG nhận từ body/query.
/// </summary>
public class CartService : ICartService
{
    private readonly ICartRepository _repo;

    public CartService(ICartRepository repo)
    {
        _repo = repo;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 1. THÊM VÀO GIỎ HÀNG (UC-21)
    // ═══════════════════════════════════════════════════════════════════════
    public async Task AddToCartAsync(int userId, int courseId)
    {
        // Kiểm tra khóa học có tồn tại và đang được published không
        var course = await _repo.GetPublishedCourseAsync(courseId);
        if (course == null)
            throw new InvalidOperationException("Khóa học không tồn tại hoặc chưa được phát hành.");

        // Kiểm tra user đã enrolled chưa (không cần mua lại)
        if (await _repo.IsEnrolledAsync(userId, courseId))
            throw new InvalidOperationException("Bạn đã mua khóa học này rồi.");

        // Kiểm tra khóa học đã có trong giỏ chưa
        if (await _repo.IsCourseInCartAsync(userId, courseId))
            throw new InvalidOperationException("Khóa học đã có trong giỏ hàng.");

        // Thêm vào giỏ với giá hiện tại của khóa học (snapshot price)
        var cartItem = new CartItem
        {
            UserId    = userId,
            CourseId  = courseId,
            Price     = course.Price,       // Lưu snapshot giá tại thời điểm add
            AddedDate = DateTime.Now
        };

        await _repo.AddCartItemAsync(cartItem);
        await _repo.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 2. XÓA KHỎI GIỎ HÀNG (UC-22)
    // ═══════════════════════════════════════════════════════════════════════
    public async Task RemoveFromCartAsync(int userId, int courseId)
    {
        var item = await _repo.GetCartItemAsync(userId, courseId);
        if (item == null)
            throw new InvalidOperationException("Không tìm thấy khóa học trong giỏ hàng.");

        _repo.RemoveCartItem(item);
        await _repo.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 3. LẤY TÓM TẮT GIỎ HÀNG + TÍNH TIỀN COUPON (UC-20, UC-23)
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<CartSummaryResponse> GetCartSummaryAsync(int userId, string? couponCode)
    {
        // ── 3.1 Lấy cart items kèm navigation ──────────────────────────────
        var cartItems = await _repo.GetCartItemsWithDetailsAsync(userId);

        // ── 3.2 Map sang DTO ────────────────────────────────────────────────
        var items = cartItems.Select(c => new CartItemDto
        {
            CourseId       = c.CourseId ?? 0,
            Title          = c.Course?.Title ?? "(Khóa học không tìm thấy)",
            ThumbnailUrl   = c.Course?.CourseThumbnailUrl,
            InstructorName = c.Course?.Instructor?.InstructorNavigation?.FullName,
            // Ưu tiên snapshot giá lúc add; nếu null thì lấy giá hiện tại
            Price          = c.Price ?? c.Course?.Price ?? 0m
        }).ToList();

        // ── 3.3 Tính SubTotal ───────────────────────────────────────────────
        decimal subTotal = items.Sum(i => i.Price);

        // ── 3.4 Response mặc định (không coupon) ────────────────────────────
        var response = new CartSummaryResponse
        {
            Items             = items,
            SubTotal          = subTotal,
            DiscountAmount    = 0m,
            Total             = subTotal,
            AppliedCouponCode = null,
            CouponMessage     = null
        };

        // ── 3.5 Kiểm tra & tính Coupon nếu có ───────────────────────────────
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var now    = DateTime.Now;
            var coupon = await _repo.GetCouponByCodeAsync(couponCode);

            if (coupon == null)
            {
                response.CouponMessage = "Mã giảm giá không tồn tại.";
            }
            else if (coupon.IsActive != true)
            {
                response.CouponMessage = "Mã giảm giá đã bị vô hiệu hóa.";
            }
            else if (coupon.StartDate.HasValue && now < coupon.StartDate.Value)
            {
                response.CouponMessage = $"Mã giảm giá chưa có hiệu lực đến ngày {coupon.StartDate.Value:dd/MM/yyyy}.";
            }
            else if (coupon.EndDate.HasValue && now > coupon.EndDate.Value)
            {
                response.CouponMessage = $"Mã giảm giá đã hết hạn từ ngày {coupon.EndDate.Value:dd/MM/yyyy}.";
            }
            else if (coupon.UsageLimit.HasValue && (coupon.UsedCount ?? 0) >= coupon.UsageLimit.Value)
            {
                response.CouponMessage = "Mã giảm giá đã đạt giới hạn sử dụng.";
            }
            else if (subTotal == 0)
            {
                response.CouponMessage = "Giỏ hàng của bạn đang trống.";
            }
            else if (subTotal < coupon.MinOrderValue)
            {
                var missing = coupon.MinOrderValue - subTotal;
                response.CouponMessage = $"Cần mua thêm {missing:N0} VND để sử dụng mã này (tối thiểu {coupon.MinOrderValue:N0} VND).";
            }
            else
            {
                // ✅ Coupon hợp lệ — tính DiscountAmount theo loại
                decimal discountAmount = coupon.CouponType == "percentage"
                    ? subTotal * (coupon.DiscountValue / 100m)
                    : coupon.DiscountValue;

                // Đảm bảo Total không bao giờ âm
                discountAmount = Math.Round(Math.Min(discountAmount, subTotal), 2);

                response.DiscountAmount    = discountAmount;
                response.Total             = Math.Round(subTotal - discountAmount, 2);
                response.AppliedCouponCode = coupon.CouponCode;
                response.CouponMessage     = coupon.CouponType == "percentage"
                    ? $"Áp dụng thành công! Giảm {coupon.DiscountValue:0.##}% tổng hóa đơn."
                    : $"Áp dụng thành công! Giảm {coupon.DiscountValue:N0} VND.";
            }
        }

        // ── 3.6 Query danh sách voucher khả dụng (Voucher Wallet) ─────────────
        var now2           = DateTime.Now;
        var activeCoupons  = await _repo.GetActiveAvailableCouponsAsync(now2);

        response.AvailableCoupons = activeCoupons.Select(cp =>
        {
            bool isEligible = subTotal >= cp.MinOrderValue;

            string conditionMessage = isEligible
                ? (cp.CouponType == "percentage"
                    ? $"Giảm {cp.DiscountValue:0.##}%"
                    : $"Giảm {cp.DiscountValue:N0}đ")
                : $"Mua thêm {cp.MinOrderValue - subTotal:N0}đ để dùng mã này";

            return new AvailableCouponDto
            {
                CouponCode       = cp.CouponCode,
                CouponType       = cp.CouponType ?? "fixed",
                DiscountValue    = cp.DiscountValue,
                MinOrderValue    = cp.MinOrderValue,
                EndDate          = cp.EndDate,
                IsEligible       = isEligible,
                ConditionMessage = conditionMessage
            };
        }).ToList();

        return response;
    }
}
