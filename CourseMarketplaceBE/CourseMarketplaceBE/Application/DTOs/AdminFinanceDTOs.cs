namespace CourseMarketplaceBE.Application.DTOs;

// ═══════════════════════════════════════════════════════════════════════════
// Admin Finance DTOs — Tách biệt khỏi Entity (DIP)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Tổng quan tài chính hệ thống (UC-112)
/// 
/// Công thức:
///   GrossRevenue      = SUM(transactions.amount)         WHERE status = 'succeeded'
///   TotalPaidOut       = SUM(instructor_payouts.payout_amount)  WHERE is_paid = true
///   PendingEscrow      = SUM(instructor_payouts.payout_amount)  WHERE is_paid = false
///   PlatformNetProfit  = GrossRevenue - TotalPaidOut - PendingEscrow
///   CurrentTransferRate = system_configs.TransferRate
/// </summary>
public class FinancialSummaryResponse
{
    /// <summary>Tổng doanh thu từ mọi giao dịch thành công</summary>
    public decimal GrossRevenue { get; set; }

    /// <summary>Tổng đã trả cho giảng viên (is_paid = true)</summary>
    public decimal TotalPaidOut { get; set; }

    /// <summary>Tổng tiền đang giữ hộ giảng viên (is_paid = false) và còn trong 14 ngày</summary>
    public decimal PendingEscrow { get; set; }

    /// <summary>Tổng tiền đã qua thời gian hoàn trả (> 14 ngày) sẵn sàng thanh toán</summary>
    public decimal MaturedEscrow { get; set; }

    /// <summary>Lợi nhuận ròng = GrossRevenue - TotalPaidOut - PendingEscrow - MaturedEscrow</summary>
    public decimal PlatformNetProfit { get; set; }

    /// <summary>Tỷ lệ chia sẻ hiện tại (VD: 70 = Giảng viên nhận 70%)</summary>
    public decimal CurrentTransferRate { get; set; }

    /// <summary>Tổng số giao dịch thành công</summary>
    public int TotalTransactions { get; set; }

    /// <summary>Tổng số tiền đã hoàn trả cho học viên</summary>
    public decimal TotalRefunded { get; set; }
}

/// <summary>
/// Chi tiết 1 khoản chia tiền giảng viên — JOIN instructor_payouts + transactions + users
/// </summary>
public class PayoutDetailResponse
{
    public int PayoutId { get; set; }
    public int TransactionId { get; set; }

    /// <summary>Tên giảng viên (users.full_name)</summary>
    public string InstructorName { get; set; } = string.Empty;

    /// <summary>Email giảng viên</summary>
    public string InstructorEmail { get; set; } = string.Empty;

    /// <summary>Tên khóa học</summary>
    public string CourseTitle { get; set; } = string.Empty;

    /// <summary>Tổng tiền khách trả (transactions.amount)</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Giảng viên nhận (instructor_payouts.payout_amount)</summary>
    public decimal InstructorReceived { get; set; }

    /// <summary>Sàn nhận = TotalAmount - InstructorReceived</summary>
    public decimal PlatformReceived { get; set; }

    /// <summary>Tỷ lệ chia sẻ lúc mua (transactions.transfer_rate)</summary>
    public decimal TransferRate { get; set; }

    /// <summary>Đã chuyển tiền qua Stripe chưa</summary>
    public bool IsPaid { get; set; }

    /// <summary>
    /// Trạng thái thanh toán end-to-end:
    /// pending | transferred | in_transit | paid | failed
    /// </summary>
    public string PayoutStatus { get; set; } = "pending";

    /// <summary>Mã Transfer Stripe (tx_xxx): Sàn → Connected Account</summary>
    public string? StripeTransferId { get; set; }

    /// <summary>Mã Payout Stripe (po_xxx): Connected Account → Bank</summary>
    public string? StripePayoutId { get; set; }

    /// <summary>Thời điểm tiền đã về ngân hàng giảng viên</summary>
    public DateTime? PaidToBankAt { get; set; }

    /// <summary>Ngày giao dịch</summary>
    public DateTime? TransactionDate { get; set; }

    /// <summary>Ngày dự kiến chuyển tiền</summary>
    public DateTime PayoutDate { get; set; }
}

/// <summary>
/// Request cập nhật tỷ lệ chia sẻ doanh thu (UC-120)
/// </summary>
public class SetTransferRateRequest
{
    /// <summary>Tỷ lệ giảng viên nhận (1-100). VD: 70 = 70% cho giảng viên, 30% cho sàn.</summary>
    public decimal Rate { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════
// Platform Balance & Withdrawal DTOs
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Số dư Stripe Platform (lấy từ Stripe Balance API)
/// </summary>
public class PlatformBalanceResponse
{
    /// <summary>Số tiền có thể rút ngay (Available)</summary>
    public decimal Available { get; set; }

    /// <summary>Số tiền đang chờ vào ví (Incoming/Pending)</summary>
    public decimal Incoming { get; set; }

    /// <summary>Tổng = Available + Incoming</summary>
    public decimal Total { get; set; }

    public string Currency { get; set; } = "usd";

    /// <summary>Lịch rút tiền hiện tại (manual / daily / weekly / monthly)</summary>
    public string PayoutScheduleInterval { get; set; } = "manual";

    /// <summary>Chi tiết lịch (VD: "Monday" nếu weekly)</summary>
    public string? PayoutScheduleAnchor { get; set; }
}

/// <summary>
/// Request rút tiền từ Stripe về ngân hàng
/// </summary>
public class WithdrawRequest
{
    /// <summary>Số tiền muốn rút (USD). Để 0 hoặc null = rút toàn bộ Available.</summary>
    public decimal? Amount { get; set; }

    /// <summary>Ghi chú (tùy chọn)</summary>
    public string? Description { get; set; }
}

/// <summary>
/// Response sau khi tạo lệnh rút thành công
/// </summary>
public class WithdrawResponse
{
    public int WithdrawalId { get; set; }
    public string StripePayoutId { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO hiển thị lịch sử rút tiền
/// </summary>
public class WithdrawalHistoryItem
{
    public int WithdrawalId { get; set; }
    public string? ManagerName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string? StripePayoutId { get; set; }
    public string Status { get; set; } = "pending";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
}

/// <summary>
/// Kết quả đồng bộ trạng thái lệnh rút
/// </summary>
public class BulkPayoutResult
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════════════
// REFUND DTOs
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Request hoàn tiền từ Admin Dashboard.
/// </summary>
public class RefundRequest
{
    /// <summary>Lý do hoàn tiền: requested_by_customer | duplicate | fraudulent</summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Response chi tiết kết quả hoàn tiền.
/// </summary>
public class RefundResultResponse
{
    /// <summary>Mã Stripe Refund (re_xxx)</summary>
    public string StripeRefundId { get; set; } = "";

    /// <summary>Mã Transfer Reversal (trr_xxx) — null nếu chưa transfer cho GV</summary>
    public string? StripeReversalId { get; set; }

    /// <summary>Số tiền hoàn lại cho khách hàng</summary>
    public decimal RefundedAmount { get; set; }

    /// <summary>Enrollment bị thu hồi hay không</summary>
    public bool EnrollmentRevoked { get; set; }

    /// <summary>Trạng thái giao dịch sau khi refund</summary>
    public string TransactionStatus { get; set; } = "refunded";
}

public class InstructorCourseRevenueResponse
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal PreviousMonthRevenue { get; set; }
    public decimal YearlyRevenue { get; set; }
    public decimal LifetimeRevenue { get; set; }
}

