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
        var course = await ValidateAndGetCourseForAdditionAsync(userId, courseId);

        var cartItem = new CartItem
        {
            UserId    = userId,
            CourseId  = courseId,
            Price     = course.Price,
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
            throw new InvalidOperationException("Course not found in the cart.");

        _repo.RemoveCartItem(item);
        await _repo.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 3. LẤY TÓM TẮT GIỎ HÀNG + TÍNH TIỀN COUPON (UC-20, UC-23)
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<CartSummaryResponse> GetCartSummaryAsync(int userId, string? couponCode)
    {
        var cartItems = await _repo.GetCartItemsWithDetailsAsync(userId);
        var cartItemsList = cartItems.ToList();

        var items = MapToCartItemDtos(cartItemsList);
        decimal subTotal = items.Sum(i => i.Price);

        var response = new CartSummaryResponse
        {
            Items             = items,
            SubTotal          = subTotal,
            DiscountAmount    = 0m,
            Total             = subTotal,
            AppliedCouponCode = null,
            CouponMessage     = null
        };

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            await ApplyCouponsAsync(response, cartItemsList, items, couponCode);
        }

        response.AvailableCoupons = await GetAvailableCouponsAsync(cartItemsList);

        return response;
    }

    // ─── PRIVATE HELPERS ──────────────────────────────────────────────────────

    private async Task<Course> ValidateAndGetCourseForAdditionAsync(int userId, int courseId)
    {
        var course = await _courseRepo.GetByIdAsync(courseId);
        if (course == null || course.CourseStatus != CourseStatus.Published.ToValue())
            throw new InvalidOperationException("Course does not exist or is not published.");

        if (course.InstructorId == userId)
            throw new InvalidOperationException("You cannot add your own course to the cart.");

        if (await _courseRepo.IsEnrolledAsync(userId, courseId))
            throw new InvalidOperationException("You have already purchased this course.");

        if (await _repo.IsCourseInCartAsync(userId, courseId))
            throw new InvalidOperationException("Course is already in the cart.");

        return course;
    }

    private List<CartItemDto> MapToCartItemDtos(List<CartItem> cartItems)
    {
        return cartItems.Select(c => new CartItemDto
        {
            CourseId       = c.CourseId ?? 0,
            Title          = c.Course?.Title ?? "(Course not found)",
            ThumbnailUrl   = c.Course?.CourseThumbnailUrl,
            InstructorName = c.Course?.Instructor?.InstructorNavigation?.FullName,
            Price          = c.Price ?? c.Course?.Price ?? 0m,
            OriginalPrice  = c.Price ?? c.Course?.Price ?? 0m,
            DiscountedPrice = c.Price ?? c.Course?.Price ?? 0m,
            AppliedCouponCode = null,
            DiscountAmount = 0m
        }).ToList();
    }

    private async Task ApplyCouponsAsync(CartSummaryResponse response, List<CartItem> cartItems, List<CartItemDto> items, string couponCode)
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
            ValidateCoupon(coupon, now);

            decimal eligibleSubTotal = CalculateEligibleSubTotal(cartItems, coupon.CouponId);

            if (eligibleSubTotal >= coupon.MinOrderValue && eligibleSubTotal > 0)
            {
                ApplyDiscount(response, items, cartItems, coupon, eligibleSubTotal, appliedCoupons, couponMessages);
            }
        }

        if (appliedCoupons.Any())
        {
            response.Total = Math.Round(Math.Round(response.SubTotal - response.DiscountAmount, 2), 2);
            response.AppliedCouponCode = string.Join(",", appliedCoupons);
            response.CouponMessage = "Applied: " + string.Join(", ", couponMessages);
        }
        else
        {
            response.CouponMessage = "No valid coupon applied.";
        }
    }

    private void ValidateCoupon(Coupon? coupon, DateTime now)
    {
        if (coupon == null)
            throw new InvalidOperationException("This coupon does not exist.");

        if (coupon.IsActive != true ||
            (coupon.StartDate.HasValue && now < coupon.StartDate.Value) ||
            (coupon.EndDate.HasValue && now > coupon.EndDate.Value) ||
            (coupon.UsageLimit.HasValue && (coupon.UsedCount ?? 0) >= coupon.UsageLimit.Value))
        {
            throw new InvalidOperationException("This coupon has expired or run out of usage.");
        }
    }

    private decimal CalculateEligibleSubTotal(IEnumerable<CartItem> cartItems, int couponId)
    {
        return cartItems
            .Where(c => c.Course != null && c.Course.CouponId == couponId)
            .Sum(c => c.Price ?? c.Course?.Price ?? 0m);
    }

    private void ApplyDiscount(CartSummaryResponse response, List<CartItemDto> items, List<CartItem> cartItems, Coupon coupon, decimal eligibleSubTotal, List<string> appliedCoupons, List<string> couponMessages)
    {
        decimal discountAmount = coupon.CouponType == "percentage"
            ? eligibleSubTotal * (coupon.DiscountValue / 100m)
            : coupon.DiscountValue;

        discountAmount = Math.Round(Math.Min(discountAmount, eligibleSubTotal), 2);

        response.DiscountAmount += discountAmount;
        appliedCoupons.Add(coupon.CouponCode);

        var msg = coupon.CouponType == "percentage"
            ? $"off {coupon.DiscountValue:0.##}%"
            : $"off ${coupon.DiscountValue:N2}";
        couponMessages.Add($"{coupon.CouponCode} ({msg})");

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

    private async Task<List<AvailableCouponDto>> GetAvailableCouponsAsync(List<CartItem> cartItems)
    {
        var now = DateTime.Now;
        var activeCoupons = await _couponRepo.GetActiveAvailableCouponsAsync(now);

        return activeCoupons.Select(cp =>
        {
            decimal eligibleSubTotal = CalculateEligibleSubTotal(cartItems, cp.CouponId);
            bool isEligible = eligibleSubTotal >= cp.MinOrderValue && eligibleSubTotal > 0;
            string conditionMessage = GenerateCouponConditionMessage(cp, eligibleSubTotal, isEligible);

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
    }

    private string GenerateCouponConditionMessage(Coupon cp, decimal eligibleSubTotal, bool isEligible)
    {
        if (eligibleSubTotal == 0)
        {
            return "Not applicable to any course in the cart";
        }
        else if (!isEligible)
        {
            return $"Spend another ${(cp.MinOrderValue - eligibleSubTotal):N2} to use this coupon";
        }
        else
        {
            return cp.CouponType == "percentage"
                ? $"Off {cp.DiscountValue:0.##}%"
                : $"Off ${cp.DiscountValue:N2}";
        }
    }
}
