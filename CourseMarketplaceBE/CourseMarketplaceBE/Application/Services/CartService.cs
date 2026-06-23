using CourseMarketplaceBE.Domain.Constants;
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
    private readonly ICourseRepository _courseRepo;
    private readonly ICouponRepository _couponRepo;

    public CartService(
        ICartRepository repo,
        ICourseRepository courseRepo,
        ICouponRepository couponRepo)
    {
        _repo = repo;
        _courseRepo = courseRepo;
        _couponRepo = couponRepo;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 1. THÊM VÀO GIỎ HÀNG (UC-21)
    // ═══════════════════════════════════════════════════════════════════════
    public async Task AddToCartAsync(int userId, int courseId)
    {
        // Kiểm tra khóa học có tồn tại và đang được published không
        var course = await _courseRepo.GetByIdAsync(courseId);
        if (course == null || course.CourseStatus != CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue())
            throw new InvalidOperationException("Course does not exist or is not published.");

        // Kiểm tra không cho phép giảng viên tự mua khóa học của mình
        if (course.InstructorId == userId)
            throw new InvalidOperationException("You cannot add your own course to the cart.");

        // Kiểm tra user đã enrolled chưa (không cần mua lại)
        if (await _courseRepo.IsEnrolledAsync(userId, courseId))
            throw new InvalidOperationException("You have already purchased this course.");

        // Kiểm tra khóa học đã có trong giỏ chưa
        if (await _repo.IsCourseInCartAsync(userId, courseId))
            throw new InvalidOperationException("Course is already in the cart.");

        // Thêm vào giỏ với giá hiện tại của khóa học (snapshot price)
        var cartItem = new CartItem
        {
            UserId    = userId,
            CourseId  = courseId,
            Price     = course.Price,       // Lưu snapshot giá tại thời điểm add
            AddedDate = DateTime.Now
        };

        await _repo.AddCartItemAsync(cartItem);
        int numberOfRowsAffected = await _repo.SaveChangesAsync();
        if (numberOfRowsAffected <= 0)
            throw new InvalidOperationException("Failed to save changes");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 2. XÓA KHỎI GIỎ HÀNG (UC-22)
    // ═══════════════════════════════════════════════════════════════════════
    public async Task RemoveFromCartAsync(int userId, int courseId)
    {
        var item = await _repo.GetCartItemAsync(userId, courseId);
        if (item == null)
            throw new InvalidOperationException("Course not found in the cart.");

        _repo.RemoveCartItem(item);
        int numberOfRowsAffected = await _repo.SaveChangesAsync();
        if (numberOfRowsAffected <= 0)
            throw new InvalidOperationException("Failed to save changes");
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
            Title          = c.Course?.Title ?? "(Course not found)",
            ThumbnailUrl   = c.Course?.CourseThumbnailUrl,
            InstructorName = c.Course?.Instructor?.InstructorNavigation?.FullName,
            // Ưu tiên snapshot giá lúc add; nếu null thì lấy giá hiện tại
            Price          = c.Price ?? c.Course?.Price ?? 0m,
            OriginalPrice  = c.Price ?? c.Course?.Price ?? 0m,
            DiscountedPrice = c.Price ?? c.Course?.Price ?? 0m,
            AppliedCouponCode = null,
            DiscountAmount = 0m
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
            var now = DateTime.Now;
            var codes = couponCode.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim().ToUpper())
                .Distinct()
                .ToList();

            var appliedCoupons = new List<string>();
            var couponMessages = new List<string>();

            foreach (var code in codes)
            {
                var coupon = await _couponRepo.GetByCodeAsync(code);
                if (coupon == null)
                {
                    throw new InvalidOperationException("This coupon does not exist.");
                }

                if (coupon.IsActive != true ||
                    (coupon.StartDate.HasValue && now < coupon.StartDate.Value) ||
                    (coupon.EndDate.HasValue && now > coupon.EndDate.Value) ||
                    (coupon.UsageLimit.HasValue && (coupon.UsedCount ?? 0) >= coupon.UsageLimit.Value))
                {
                    throw new InvalidOperationException("This coupon has expired or run out of usage.");
                }

                // Logic mới: Chỉ giảm cho khóa học khớp mã
                decimal eligibleSubTotal = cartItems
                    .Where(c => c.Course != null && c.Course.CouponId == coupon.CouponId)
                    .Sum(c => c.Price ?? c.Course?.Price ?? 0m);

                if (eligibleSubTotal >= coupon.MinOrderValue && eligibleSubTotal > 0)
                {
                    // ✅ Coupon hợp lệ — tính DiscountAmount theo loại dựa trên eligibleSubTotal
                    decimal discountAmount = coupon.CouponType == "percentage"
                        ? eligibleSubTotal * (coupon.DiscountValue / 100m)
                        : coupon.DiscountValue;

                    // Đảm bảo không vượt quá giá trị khóa học
                    discountAmount = Math.Round(Math.Min(discountAmount, eligibleSubTotal), 2);

                    response.DiscountAmount += discountAmount;
                    appliedCoupons.Add(coupon.CouponCode);

                    var msg = coupon.CouponType == "percentage"
                        ? $"off {coupon.DiscountValue:0.##}%"
                        : $"off ${coupon.DiscountValue:N2}";
                    couponMessages.Add($"{coupon.CouponCode} ({msg})");

                    // Cập nhật giá trị giảm giá cho từng item cụ thể!
                    foreach (var item in items)
                    {
                        var matchingCartItem = cartItems.FirstOrDefault(c => c.CourseId == item.CourseId);
                        if (matchingCartItem != null && matchingCartItem.Course != null && matchingCartItem.Course.CouponId == coupon.CouponId)
                        {
                            decimal itemDiscount = coupon.CouponType == "percentage"
                                ? item.OriginalPrice * (coupon.DiscountValue / 100m)
                                : coupon.DiscountValue;

                            itemDiscount = Math.Round(Math.Min(itemDiscount, item.OriginalPrice), 2);

                            item.DiscountAmount = itemDiscount;
                            item.DiscountedPrice = item.OriginalPrice - itemDiscount;
                            item.AppliedCouponCode = coupon.CouponCode;
                        }
                    }
                }
            }

            if (appliedCoupons.Any())
            {
                response.Total = Math.Round(Math.Round(subTotal - response.DiscountAmount, 2), 2); // Tránh nested math
                response.AppliedCouponCode = string.Join(",", appliedCoupons);
                response.CouponMessage = "Applied: " + string.Join(", ", couponMessages);
            }
            else
            {
                response.CouponMessage = "No valid coupon applied.";
            }
        }

        // ── 3.6 Query danh sách voucher khả dụng (Voucher Wallet) ─────────────
        var now2           = DateTime.Now;
        var activeCoupons  = await _couponRepo.GetActiveAvailableCouponsAsync(now2);

        response.AvailableCoupons = activeCoupons.Select(cp =>
        {
            // Tính eligibleSubTotal cho cp này
            decimal eligibleSubTotal = cartItems
                .Where(c => c.Course != null && c.Course.CouponId == cp.CouponId)
                .Sum(c => c.Price ?? c.Course?.Price ?? 0m);

            bool isEligible = eligibleSubTotal >= cp.MinOrderValue && eligibleSubTotal > 0;

            string conditionMessage;
            if (eligibleSubTotal == 0)
            {
                conditionMessage = "Not applicable to any course in the cart";
            }
            else if (!isEligible)
            {
                conditionMessage = $"Spend another ${(cp.MinOrderValue - eligibleSubTotal):N2} to use this coupon";
            }
            else
            {
                conditionMessage = cp.CouponType == "percentage"
                    ? $"Off {cp.DiscountValue:0.##}%"
                    : $"Off ${cp.DiscountValue:N2}";
            }

            return new AvailableCouponDto
            {
                CouponCode       = cp.CouponCode,
                CouponType       = cp.CouponType ?? "fixed",
                DiscountValue    = cp.DiscountValue,
                MinOrderValue    = cp.MinOrderValue,
                EndDate          = cp.EndDate,
                IsEligible       = isEligible,
                ConditionMessage = conditionMessage,
                CourseId         = cp.Courses.FirstOrDefault()?.CourseId
            };
        }).ToList();

        return response;
    }
}

