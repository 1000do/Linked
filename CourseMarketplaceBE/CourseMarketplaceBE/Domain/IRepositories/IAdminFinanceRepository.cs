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
    Task<decimal> GetGrossRevenueAsync(int? year = null, int? month = null);

    /// <summary>Tổng số giao dịch thành công.</summary>
    Task<int> GetSucceededTransactionCountAsync(int? year = null, int? month = null);

    /// <summary>Tổng tiền đã trả giảng viên (is_paid = true).</summary>
    Task<decimal> GetTotalPaidOutAsync(int? year = null, int? month = null);

    /// <summary>Tổng tiền chờ trả giảng viên (is_paid = false) và còn trong thời gian hoàn tiền (<= 14 ngày).</summary>
    Task<decimal> GetPendingEscrowAsync(int? year = null, int? month = null);

    /// <summary>Tổng tiền chờ trả giảng viên (is_paid = false) và đã qua thời gian hoàn tiền (> 14 ngày).</summary>
    Task<decimal> GetMaturedEscrowAsync(int? year = null, int? month = null);

    // ── Payout History ───────────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách chi tiết chia tiền, JOIN qua:
    ///   instructor_payouts → transactions → order_items → courses
    ///   instructor_payouts → instructors → users → accounts
    /// </summary>
    Task<List<PayoutDetailProjection>> GetPayoutDetailsAsync(int? year = null, int? month = null);

    /// <summary>Lấy thông tin payout theo Id.</summary>
    Task<Domain.Entities.InstructorPayout?> GetPayoutByIdAsync(int payoutId);

    /// <summary>
    /// Tìm TẤT CẢ payout theo Stripe Payout ID (po_xxx).
    /// Dùng trong Webhook payout.paid / payout.failed để match bản ghi DB.
    /// </summary>
    Task<List<Domain.Entities.InstructorPayout>> GetPayoutsByStripePayoutIdAsync(string stripePayoutId);

    /// <summary>
    /// Tìm TẤT CẢ payout đang ở trạng thái 'transferred' của một Connected Account cụ thể.
    /// Dùng trong Webhook payout.created để lưu stripe_payout_id cho toàn bộ các khoản tiền đang chờ.
    /// </summary>
    Task<List<Domain.Entities.InstructorPayout>> GetTransferredPayoutsByAccountAsync(string stripeAccountId);

    /// <summary>Tìm 1 bản ghi payout theo mã Stripe Transfer ID.</summary>
    Task<Domain.Entities.InstructorPayout?> GetPayoutByTransferIdAsync(string transferId);

    // ── Platform Withdrawals ──────────────────────────────────────────────

    /// <summary>Thêm mới bản ghi rút tiền Sàn.</summary>
    Task AddWithdrawalAsync(Domain.Entities.PlatformWithdrawal withdrawal);

    /// <summary>Lấy danh sách rút tiền Sàn (mới nhất trước).</summary>
    Task<List<Domain.Entities.PlatformWithdrawal>> GetWithdrawalsAsync();

    // ── Refund ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách các giao dịch có yêu cầu hoàn tiền đang chờ duyệt (status = 'refund_pending').
    /// </summary>
    Task<List<Domain.Entities.Transaction>> GetPendingRefundRequestsAsync();

    /// <summary>
    /// Lấy Transaction entity đầy đủ kèm InstructorPayouts + OrderItem → Course → Enrollment.
    /// Cần để orchestrate toàn bộ flow refund (reverse transfer, refund, revoke enrollment).
    /// </summary>
    Task<Domain.Entities.Transaction?> GetTransactionWithFullGraphAsync(int transactionId);

    /// <summary>
    /// Tìm Enrollment active của user cho khóa học (để revoke khi refund).
    /// </summary>
    Task<Domain.Entities.Enrollment?> GetActiveEnrollmentAsync(int userId, int courseId);

    /// <summary>
    /// Lấy Stripe Transfer ID gốc (tr_xxx) từ DestinationPayment (py_xxx).
    /// InstructorPayout lưu py_xxx, nhưng Transfer Reversal API cần tr_xxx.
    /// </summary>
    Task<string?> GetStripeTransferIdByDestinationPaymentAsync(string destinationPaymentId);

    /// <summary>
    /// Tìm Transaction theo Stripe PaymentIntent ID (pi_xxx).
    /// Dùng cho Webhook sync khi Admin refund trực tiếp trên Stripe Dashboard.
    /// </summary>
    Task<Domain.Entities.Transaction?> GetTransactionByPaymentIntentIdAsync(string paymentIntentId);

    /// <summary>Xóa bản ghi chia tiền giảng viên (dùng khi hoàn tiền).</summary>
    void RemoveInstructorPayout(Domain.Entities.InstructorPayout payout);

    /// <summary>Commit changes.</summary>
    Task<int> SaveChangesAsync();
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

    // ★ Cột bổ sung cho Webhook tracking
    public string? PayoutStatus { get; set; }
    public string? StripeTransferId { get; set; }
    public string? StripePayoutId { get; set; }
    public DateTime? PaidToBankAt { get; set; }
}
