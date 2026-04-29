namespace CourseMarketplaceBE.Application.DTOs;

/// <summary>
/// Request để khởi tạo checkout — couponCode tùy chọn.
/// </summary>
public class CheckoutRequest
{
    /// <summary>Mã giảm giá (null = không áp dụng).</summary>
    public string? CouponCode { get; set; }

    /// <summary>URL redirect khi thanh toán thành công.</summary>
    public string SuccessUrl { get; set; } = string.Empty;

    /// <summary>URL redirect khi hủy thanh toán.</summary>
    public string CancelUrl { get; set; } = string.Empty;
}

/// <summary>
/// Response trả về Stripe Session URL để FE redirect.
/// </summary>
public class CheckoutResponse
{
    /// <summary>URL Stripe Checkout Page — FE sẽ redirect tới đây.</summary>
    public string SessionUrl { get; set; } = string.Empty;

    /// <summary>Stripe Session ID — dùng để verify khi success callback.</summary>
    public string SessionId { get; set; } = string.Empty;
}

/// <summary>
/// DTO nội bộ để IPaymentGatewayService trả về kết quả tạo session.
/// </summary>
public class PaymentSessionResult
{
    public string SessionUrl { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

/// <summary>
/// DTO mô tả 1 item trong đơn hàng, dùng để gửi cho Payment Gateway.
/// Tách biệt khỏi Entity để tuân thủ DIP — Payment Gateway không cần biết EF Core entities.
/// </summary>
public class PaymentLineItem
{
    public string CourseName { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public decimal UnitPrice { get; set; }
}
