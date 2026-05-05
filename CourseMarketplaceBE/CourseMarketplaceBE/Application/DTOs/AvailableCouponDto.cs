namespace CourseMarketplaceBE.Application.DTOs;

/// <summary>
/// DTO cho 1 voucher hiển thị trong "Ví Voucher" của Learner.
/// IsEligible = true nếu CartSubTotal đủ điều kiện MinOrderValue.
/// ConditionMessage = text thân thiện hiển thị trên UI (Shopee style).
/// </summary>
public class AvailableCouponDto
{
    public string CouponCode { get; set; } = null!;
    public string CouponType { get; set; } = null!;   // "percentage" | "fixed"
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>True khi CartSubTotal >= MinOrderValue.</summary>
    public bool IsEligible { get; set; }

    /// <summary>
    /// Text hiển thị điều kiện.
    /// Eligible:   "Giảm 20%" | "Giảm 50,000đ"
    /// Ineligible: "Mua thêm 30,000đ để dùng mã này"
    /// </summary>
    public string ConditionMessage { get; set; } = string.Empty;
}
