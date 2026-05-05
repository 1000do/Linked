namespace CourseMarketplaceFE.Models;

/// <summary>
/// ViewModel cho trang giỏ hàng (Cart/Index.cshtml).
/// Dữ liệu được map từ CartSummaryResponse trả về bởi BE API.
/// </summary>
public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string? AppliedCouponCode { get; set; }
    public string? CouponMessage { get; set; }

    /// <summary>True nếu coupon được áp dụng thành công (DiscountAmount > 0).</summary>
    public bool HasDiscount => DiscountAmount > 0;

    /// <summary>Danh sách voucher khả dụng từ hệ thống (Voucher Wallet UI).</summary>
    public List<AvailableCouponViewModel> AvailableCoupons { get; set; } = new();
}

public class CartItemViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? InstructorName { get; set; }
    public decimal Price { get; set; }
}

public class AvailableCouponViewModel
{
    public string CouponCode { get; set; } = string.Empty;
    public string CouponType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsEligible { get; set; }
    public string ConditionMessage { get; set; } = string.Empty;
}

