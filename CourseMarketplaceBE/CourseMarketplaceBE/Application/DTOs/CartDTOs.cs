namespace CourseMarketplaceBE.Application.DTOs;

/// <summary>
/// Thông tin 1 khóa học trong giỏ hàng trả về cho client.
/// </summary>
public class CartItemDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? InstructorName { get; set; }   // Tên giảng viên (full_name từ users)
    public decimal Price { get; set; }             // Giá tại thời điểm thêm vào giỏ
}

/// <summary>
/// Kết quả tổng hợp giỏ hàng, bao gồm tính toán coupon và danh sách voucher khả dụng.
/// </summary>
public class CartSummaryResponse
{
    /// <summary>Danh sách khóa học trong giỏ.</summary>
    public List<CartItemDto> Items { get; set; } = new();

    /// <summary>Tổng tiền trước khi giảm giá (SubTotal = sum of Items.Price).</summary>
    public decimal SubTotal { get; set; }

    /// <summary>Số tiền được giảm (= 0 nếu không áp dụng coupon hợp lệ).</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Tổng tiền thanh toán = SubTotal - DiscountAmount (không bao giờ âm).</summary>
    public decimal Total { get; set; }

    /// <summary>Mã coupon đã áp dụng thành công (null nếu không dùng).</summary>
    public string? AppliedCouponCode { get; set; }

    /// <summary>Thông báo về coupon (thành công / lỗi / hết hạn...).</summary>
    public string? CouponMessage { get; set; }

    /// <summary>
    /// Danh sách các voucher đang hoạt động, kèm trạng thái IsEligible dựa trên SubTotal hiện tại.
    /// Dùng để render Voucher Wallet trên UI (kiểu Shopee).
    /// </summary>
    public List<AvailableCouponDto> AvailableCoupons { get; set; } = new();
}

