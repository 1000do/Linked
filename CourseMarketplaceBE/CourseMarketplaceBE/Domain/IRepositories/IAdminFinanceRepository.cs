namespace CourseMarketplaceBE.Domain.IRepositories;

/// <summary>
/// SOLID — ISP: Interface chỉ chứa những method mà AdminFinanceService cần.
/// SOLID — DIP: Service layer phụ thuộc vào abstraction, không phụ thuộc DbContext.
/// </summary>
public interface IAdminFinanceRepository
{
    // ── System Config ────────────────────────────────────────────────────

    /// <summary>Lấy giá trị config theo key từ bảng system_configs.</summary>
    Task<string?> GetConfigValueAsync(string configKey);

    /// <summary>
    /// Upsert (Insert hoặc Update) config value.
    /// Nếu key đã tồn tại → update. Chưa có → insert.
    /// </summary>
    Task UpsertConfigAsync(string configKey, string configValue, string? description = null);

    // ── Financial Aggregations (tính trực tiếp trong DB bằng SUM) ─────

    /// <summary>
    /// Tổng doanh thu = SUM(amount) WHERE status = 'succeeded'
    /// Thực thi trên PostgreSQL, KHÔNG kéo data về RAM.
    /// </summary>
    Task<decimal> GetGrossRevenueAsync();

    /// <summary>Tổng số giao dịch thành công.</summary>
    Task<int> GetSucceededTransactionCountAsync();

    /// <summary>Tổng tiền đã trả giảng viên (is_paid = true).</summary>
    Task<decimal> GetTotalPaidOutAsync();

    /// <summary>Tổng tiền chờ trả giảng viên (is_paid = false).</summary>
    Task<decimal> GetPendingEscrowAsync();

    // ── Payout History ───────────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách chi tiết chia tiền, JOIN qua:
    ///   instructor_payouts → transactions → order_items → courses
    ///   instructor_payouts → instructors → users → accounts
    /// </summary>
    Task<List<PayoutDetailProjection>> GetPayoutDetailsAsync();

    /// <summary>Commit changes.</summary>
    Task SaveChangesAsync();
}

/// <summary>
/// Projection DTO dùng nội bộ trong Repository → Service.
/// Tránh expose EF entity ra ngoài (DIP).
/// </summary>
public class PayoutDetailProjection
{
    public int PayoutId { get; set; }
    public int TransactionId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public string InstructorEmail { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal InstructorReceived { get; set; }
    public decimal TransferRate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? TransactionDate { get; set; }
    public DateTime PayoutDate { get; set; }
}
