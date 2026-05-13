using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

/// <summary>
/// SOLID — ISP: Interface chỉ chứa nghiệp vụ Admin Finance.
/// SOLID — DIP: Controller phụ thuộc interface, không biết implementation.
/// </summary>
public interface IAdminFinanceService
{
    /// <summary>
    /// UC-120: Cập nhật tỷ lệ chia sẻ doanh thu.
    /// Giá trị từ 1-100 (VD: 70 = 70% cho giảng viên, 30% cho sàn).
    /// </summary>
    Task SetTransferRateAsync(decimal rate);

    /// <summary>
    /// UC-112: Lấy tổng quan tài chính hệ thống.
    /// Bao gồm: Gross Revenue, Net Profit, Pending Escrow, Transfer Rate.
    /// </summary>
    Task<FinancialSummaryResponse> GetFinancialSummaryAsync();

    /// <summary>
    /// Lấy danh sách chi tiết chia tiền giảng viên.
    /// JOIN: instructor_payouts → transactions → courses → users.
    /// </summary>
    Task<List<PayoutDetailResponse>> GetInstructorPayoutsAsync();

    /// <summary>
    /// Đánh dấu một khoản nợ giảng viên là đã thanh toán thủ công.
    /// </summary>
    Task MarkPayoutAsPaidAsync(int payoutId);

    /// <summary>
    /// Lấy tỷ lệ chia sẻ hiện tại từ system_configs.
    /// </summary>
    Task<decimal> GetCurrentTransferRateAsync();

    /// <summary>
    /// Thực hiện chuyển tiền thật từ ví Sàn sang ví Giảng viên qua Stripe Connect.
    /// </summary>
    Task<string> PerformStripeTransferAsync(int payoutId);

    /// <summary>
    /// Quét và thanh toán tất cả công nợ đang chờ qua Stripe.
    /// </summary>
    Task<BulkPayoutResult> BulkPayAllViaStripeAsync();

    // ═══════════════════════════════════════════════════════════════════════
    // PLATFORM WITHDRAWAL (Rút tiền lợi nhuận Sàn về ngân hàng)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Lấy số dư Stripe Platform (Available + Incoming).</summary>
    Task<PlatformBalanceResponse> GetPlatformBalanceAsync();

    /// <summary>Tạo lệnh rút tiền từ Stripe về ngân hàng Admin.</summary>
    Task<WithdrawResponse> CreateWithdrawalAsync(WithdrawRequest request, int managerId);

    /// <summary>Lấy danh sách lịch sử rút tiền.</summary>
    Task<List<WithdrawalHistoryItem>> GetWithdrawalHistoryAsync();

    // ═══════════════════════════════════════════════════════════════════════
    // REFUND (Hoàn tiền cho khách hàng)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Hoàn tiền toàn bộ cho 1 giao dịch. Orchestrate:
    ///   1. Reverse Stripe Transfer (nếu đã chuyển cho GV)
    ///   2. Tạo Stripe Refund trên PaymentIntent gốc
    ///   3. Cập nhật trạng thái Transaction → refunded
    ///   4. Cập nhật trạng thái InstructorPayout → refunded
    ///   5. Thu hồi Enrollment của học viên
    /// </summary>
    /// <param name="transactionId">ID giao dịch cần hoàn tiền.</param>
    /// <param name="reason">Lý do hoàn tiền (requested_by_customer, duplicate, fraudulent).</param>
    /// <returns>Stripe Refund ID (re_xxx).</returns>
    Task<RefundResultResponse> RefundTransactionAsync(int transactionId, string? reason = null);
}
