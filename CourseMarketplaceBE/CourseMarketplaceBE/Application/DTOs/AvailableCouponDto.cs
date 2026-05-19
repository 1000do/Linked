namespace CourseMarketplaceBE.Application.DTOs;

/// <summary>
/// DTO for a coupon displayed in the Learner's "Voucher Wallet".
/// IsEligible = true if CartSubTotal meets the MinOrderValue requirement.
/// ConditionMessage = friendly text displayed on the UI (Shopee style).
/// </summary>
public class AvailableCouponDto
{
    public string CouponCode { get; set; } = null!;
    public string CouponType { get; set; } = null!;   // "percentage" | "fixed"
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>True when CartSubTotal >= MinOrderValue.</summary>
    public bool IsEligible { get; set; }

    /// <summary>Text displaying conditions.
    /// Eligible:   "Off 20%" | "Off $50.00"
    /// Ineligible: "Spend another $30.00 to use this coupon"
    /// </summary>
    public string ConditionMessage { get; set; } = string.Empty;

    /// <summary>ID of the course this coupon is linked to.</summary>
    public int? CourseId { get; set; }
}
