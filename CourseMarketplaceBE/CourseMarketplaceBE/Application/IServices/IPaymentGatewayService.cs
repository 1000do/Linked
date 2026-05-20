using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

/// <summary>
/// SOLID — OCP + DIP:
/// Interface cho cổng thanh toán. Hiện tại dùng Stripe, sau này có thể
/// thêm VNPayPaymentService, MomoPaymentService... mà KHÔNG SỬA lõi.
/// Application layer chỉ biết interface này, không biết Stripe SDK.
/// </summary>
public interface IPaymentGatewayService
{
    /// <summary>
    /// Tạo phiên thanh toán trên cổng thanh toán.
    /// </summary>
    /// <param name="lineItems">Danh sách sản phẩm cần thanh toán.</param>
    /// <param name="successUrl">URL redirect khi thanh toán thành công.</param>
    /// <param name="cancelUrl">URL redirect khi hủy thanh toán.</param>
    /// <param name="customerEmail">Email người mua (optional).</param>
    /// <param name="orderReference">Mã tham chiếu đơn hàng, dùng để nhóm các lệnh chuyển tiền liên quan (Stripe: transfer_group).</param>
    /// <returns>SessionUrl + SessionId.</returns>
    Task<PaymentSessionResult> CreateCheckoutSessionAsync(
        List<PaymentLineItem> lineItems,
        string successUrl,
        string cancelUrl,
        string? customerEmail = null,
        string? orderReference = null,
        string currency = "usd",
        string? destinationAccountId = null,
        decimal? applicationFee = null,
        Dictionary<string, string>? metadata = null);

    /// <summary>
    /// Lấy mã giao dịch thanh toán từ phiên đã hoàn tất (Stripe: PaymentIntent ID từ Session).
    /// Cần để tạo lệnh chuyển tiền cho instructor.
    /// </summary>
    Task<string?> GetPaymentReferenceAsync(string sessionId);

    /// <summary>
    /// Lấy metadata của một Stripe Session.
    /// </summary>
    Task<Dictionary<string, string>?> GetSessionMetadataAsync(string sessionId);

    /// <summary>
    /// ★ Lấy Charge ID từ PaymentIntent ID.
    /// Cần cho source_transaction khi tạo Transfer (cho phép transfer từ pending balance).
    /// </summary>
    Task<string?> GetChargeIdFromPaymentIntentAsync(string paymentIntentId);

    /// <summary>
    /// Chuyển tiền cho đối tác (instructor) sau khi thanh toán thành công.
    /// Stripe: Tạo Transfer từ platform → connected account.
    /// </summary>
    /// <param name="amount">Số tiền chuyển (theo đơn vị tiền tệ gốc, VD: 13.99 USD).</param>
    /// <param name="currency">Mã tiền tệ (usd, vnd...).</param>
    /// <param name="destinationAccountId">Tài khoản đích (Stripe Connected Account ID).</param>
    /// <param name="orderReference">Mã tham chiếu đơn hàng (transfer_group).</param>
    /// <param name="description">Mô tả giao dịch.</param>
    /// <param name="sourceTransaction">
    /// ★ Stripe source_transaction (Charge ID): Liên kết Transfer với charge gốc.
    /// Khi có source_transaction, Stripe cho phép chuyển tiền từ PENDING balance
    /// (không cần available balance). Đây là cách chính thức cho marketplace.
    /// </param>
    Task CreateTransferAsync(
        decimal amount,
        string currency,
        string destinationAccountId,
        string? orderReference = null,
        string? description = null,
        string? sourceTransaction = null);

    /// <summary>
    /// Hoàn tiền cho khách hàng thông qua PaymentIntent ID.
    /// Stripe sẽ trả lại tiền vào phương thức thanh toán gốc.
    /// </summary>
    /// <param name="paymentIntentId">Mã PaymentIntent gốc (pi_xxx).</param>
    /// <param name="reason">Lý do hoàn tiền (duplicate, fraudulent, requested_by_customer).</param>
    /// <returns>Stripe Refund ID (re_xxx).</returns>
    Task<string> RefundAsync(string paymentIntentId, string? reason = null);

    /// <summary>
    /// Đảo ngược lệnh Transfer đã chuyển cho instructor.
    /// Stripe sẽ ghi nợ lại từ Connected Account về Platform.
    /// </summary>
    /// <param name="transferId">Mã Transfer gốc (tr_xxx hoặc py_xxx).</param>
    /// <returns>Stripe Transfer Reversal ID (trr_xxx).</returns>
    Task<string> ReverseTransferAsync(string transferId);
}
