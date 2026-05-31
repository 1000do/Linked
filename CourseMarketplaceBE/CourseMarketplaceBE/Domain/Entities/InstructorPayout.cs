using System;
using CourseMarketplaceBE.Domain.Constants;

namespace CourseMarketplaceBE.Domain.Entities;

/// <summary>
/// Lưu dữ liệu giao dịch chuyển tiền từ hệ thống sang tài khoản ngân hàng của instructor.
/// </summary>
public partial class InstructorPayout
{
    public int PayoutId { get; set; }

    public int? TransactionId { get; set; }

    public int? InstructorId { get; set; }

    /// <summary>Số tiền sẽ chuyển cho instructor (đã trừ phần sàn ăn)</summary>
    public decimal PayoutAmount { get; set; }

    /// <summary>Ngày hệ thống sẽ chuyển tiền (theo lịch)</summary>
    public DateTime PayoutDate { get; set; }

    public bool IsPaid { get; set; }

    /// <summary>
    /// Trạng thái thanh toán end-to-end:
    /// pending | transferred | in_transit | paid | failed
    /// </summary>
    public string PayoutStatus { get; set; } = Constants.PayoutStatus.Pending.ToValue();

    /// <summary>ID lệnh Transfer (tx_xxx): Sàn → Connected Account</summary>
    public string? StripeTransferId { get; set; }

    /// <summary>ID lệnh Payout (po_xxx): Connected Account → Bank</summary>
    public string? StripePayoutId { get; set; }

    /// <summary>Thời điểm Stripe confirm tiền đã về ngân hàng (payout.paid webhook)</summary>
    public DateTime? PaidToBankAt { get; set; }

    public virtual Transaction? Transaction { get; set; }

    public virtual Instructor? Instructor { get; set; }
}
