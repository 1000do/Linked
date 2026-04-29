namespace CourseMarketplaceBE.Application.DTOs;

// ═══════════════════════════════════════════════════════════════════════════
// Transaction DTOs — UC-114, UC-115
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// UC-115: 1 dòng trong danh sách giao dịch.
/// Projection tối giản — chỉ cột cần thiết cho bảng (không join thừa).
/// </summary>
public class TransactionListDto
{
    public int TransactionId { get; set; }

    /// <summary>Mã tham chiếu Stripe — chỉ hiển thị, không fetch Stripe API</summary>
    public string? StripeSessionId { get; set; }

    public DateTime? Date { get; set; }

    /// <summary>Tổng tiền khách trả (AUD)</summary>
    public decimal Amount { get; set; }

    /// <summary>succeeded | failed | pending</summary>
    public string? Status { get; set; }

    /// <summary>Tên người mua</summary>
    public string BuyerName { get; set; } = string.Empty;

    /// <summary>Tên khóa học</summary>
    public string CourseTitle { get; set; } = string.Empty;

    /// <summary>Tên giảng viên nhận tiền</summary>
    public string InstructorName { get; set; } = string.Empty;
}

/// <summary>
/// UC-114: Chi tiết 1 giao dịch — biên lai đầy đủ.
/// JOIN: transactions → order_items → courses → instructors → users
///       transactions → account_from → users (buyer)
///       transactions → instructor_payouts
/// </summary>
public class TransactionDetailDto
{
    // ── Thông tin giao dịch ───────────────────────────────────────────────
    public int TransactionId { get; set; }
    public string? StripeSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public DateTime? Date { get; set; }
    public string? Status { get; set; }
    public string? Currency { get; set; }

    // ── Thông tin người mua ───────────────────────────────────────────────
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;

    // ── Thông tin khóa học ────────────────────────────────────────────────
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseThumbnail { get; set; }

    // ── Thông tin giảng viên ──────────────────────────────────────────────
    public string InstructorName { get; set; } = string.Empty;
    public string InstructorEmail { get; set; } = string.Empty;

    // ── Phân bổ tài chính ★ ───────────────────────────────────────────────
    /// <summary>Tổng khách trả</summary>
    public decimal GrossAmount { get; set; }

    /// <summary>Tỷ lệ giảng viên nhận (%) lúc giao dịch</summary>
    public decimal TransferRate { get; set; }

    /// <summary>Giảng viên nhận = GrossAmount × TransferRate / 100</summary>
    public decimal InstructorPayout { get; set; }

    /// <summary>Sàn giữ = GrossAmount - InstructorPayout</summary>
    public decimal PlatformProfit { get; set; }

    /// <summary>Đã chuyển tiền cho giảng viên chưa</summary>
    public bool IsPaid { get; set; }
}

/// <summary>
/// Response wrapper phân trang cho danh sách giao dịch (UC-115).
/// </summary>
public class TransactionPagedResult
{
    public List<TransactionListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
