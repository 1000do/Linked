using System;

namespace CourseMarketplaceBE.Domain.Entities;

/// <summary>
/// Lưu lịch sử rút tiền lợi nhuận của Sàn (Admin) từ Stripe về ngân hàng.
/// </summary>
public partial class PlatformWithdrawal
{
    public int WithdrawalId { get; set; }

    /// <summary>ID của Admin thực hiện lệnh rút</summary>
    public int? ManagerId { get; set; }

    /// <summary>Số tiền rút (USD)</summary>
    public decimal Amount { get; set; }

    /// <summary>Loại tiền tệ (mặc định: usd)</summary>
    public string Currency { get; set; } = "usd";

    /// <summary>Mã Payout trên Stripe (po_xxx)</summary>
    public string? StripePayoutId { get; set; }

    /// <summary>
    /// Trạng thái rút tiền:
    /// pending | in_transit | paid | failed | canceled
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>Ghi chú từ Admin</summary>
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>Thời điểm tiền đã về ngân hàng</summary>
    public DateTime? ArrivedAt { get; set; }

    public virtual Manager? Manager { get; set; }
}
