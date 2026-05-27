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
    Task<FinancialSummaryResponse> GetFinancialSummaryAsync(int? year = null, int? month = null);

    /// <summary>
    /// Lấy danh sách chi tiết chia tiền giảng viên.
    /// JOIN: instructor_payouts → transactions → courses → users.
    /// </summary>
    Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<PayoutDetailResponse>> GetInstructorPayoutsAsync(int? year = null, int? month = null, int page = 1, int pageSize = 10);

    /// <summary>
    /// Đánh dấu một khoản nợ giảng viên là đã thanh toán thủ công.
    /// </summary>
    Task MarkPayoutAsPaidAsync(int payoutId);

    /// <summary>
    /// Lấy tỷ lệ chia sẻ hiện tại từ system_configs.
    /// </summary>
    Task<decimal> GetCurrentTransferRateAsync();

    /// <summary>
    /// Lấy ngày thanh toán tự động định kỳ (ví dụ: "15").
    /// </summary>
    Task<string> GetPayoutDaysConfigAsync();

    /// <summary>
    /// Cập nhật ngày thanh toán tự động định kỳ.
    /// </summary>
    Task SetPayoutDaysConfigAsync(string payoutDays);

    /// <summary>
    /// Thực hiện chuyển tiền thật từ ví Sàn sang ví Giảng viên qua Stripe Connect.
    /// </summary>
    Task<string> PerformStripeTransferAsync(int payoutId);

    /// <summary>
    /// Quét và thanh toán tất cả công nợ đang chờ qua Stripe.
    /// </summary>
    Task<BulkPayoutResult> BulkPayAllViaStripeAsync();

    /// <summary>
    /// Đồng bộ trạng thái toàn bộ Payouts của tất cả giảng viên từ Stripe về Ngân hàng (Paid / In Transit / Failed).
    /// </summary>
    Task SyncAllPayoutsWithStripeAsync();

    /// <summary>
    /// Học viên gửi yêu cầu hoàn tiền cho một giao dịch (đơn hàng).
    /// </summary>
    Task RequestRefundAsync(int transactionId, int studentId, string reason);

    /// <summary>
    /// Lấy danh sách các yêu cầu hoàn tiền đang chờ duyệt.
    /// </summary>
    Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<Domain.Entities.Transaction>> GetPendingRefundRequestsAsync(int page = 1, int pageSize = 10);

    /// <summary>
    /// Admin duyệt chấp nhận yêu cầu hoàn tiền (gọi Stripe refund thực tế).
    /// </summary>
    Task ApproveRefundAsync(int transactionId, string adminNote);

    /// <summary>
    /// Admin từ chối yêu cầu hoàn tiền.
    /// </summary>
    Task RejectRefundAsync(int transactionId, string adminNote);

    // ═══════════════════════════════════════════════════════════════════════
    // PLATFORM WITHDRAWAL (Rút tiền lợi nhuận Sàn về ngân hàng)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Lấy số dư Stripe Platform (Available + Incoming).</summary>
    Task<PlatformBalanceResponse> GetPlatformBalanceAsync();

    /// <summary>Tạo lệnh rút tiền từ Stripe về ngân hàng Admin.</summary>
    Task<WithdrawResponse> CreateWithdrawalAsync(WithdrawRequest request, int managerId);

    /// <summary>Lấy danh sách lịch sử rút tiền.</summary>
    Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<WithdrawalHistoryItem>> GetWithdrawalHistoryAsync(int? year = null, int? month = null, int page = 1, int pageSize = 10);

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
