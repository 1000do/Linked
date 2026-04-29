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

    /// <summary>Tổng tiền đang giữ hộ giảng viên (is_paid = false)</summary>
    public decimal PendingEscrow { get; set; }

    /// <summary>Lợi nhuận ròng = GrossRevenue - TotalPaidOut - PendingEscrow</summary>
    public decimal PlatformNetProfit { get; set; }

    /// <summary>Tỷ lệ chia sẻ hiện tại (VD: 70 = Giảng viên nhận 70%)</summary>
    public decimal CurrentTransferRate { get; set; }

    /// <summary>Tổng số giao dịch thành công</summary>
    public int TotalTransactions { get; set; }
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
