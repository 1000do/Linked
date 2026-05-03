using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Stripe;
using Stripe.Checkout;

namespace CourseMarketplaceBE.Infrastructure.Services;

/// <summary>
/// SOLID — OCP (Open/Closed Principle):
/// Implement IPaymentGatewayService cho Stripe.
/// Sau này cần VNPay? Tạo VNPayPaymentService implement cùng interface, 
/// chỉ đổi DI registration trong Program.cs — KHÔNG SỬA code lõi.
///
/// SOLID — SRP: Class này CHỈ biết cách giao tiếp với Stripe SDK.
/// Không biết gì về Order, Transaction, Database.
///
/// ★ Stripe Connect Marketplace Flow:
///   1. CreateCheckoutSessionAsync → thu tiền vào platform (với transfer_group)
///   2. GetPaymentReferenceAsync → lấy PaymentIntent ID
///   3. CreateTransferAsync → chuyển tiền từ platform → instructor connected account
/// </summary>
public class StripePaymentService : IPaymentGatewayService
{
    /// <summary>
    /// Tạo Stripe Checkout Session.
    /// Sử dụng transfer_group để nhóm các Transfer sau khi thanh toán thành công.
    /// </summary>
    public async Task<PaymentSessionResult> CreateCheckoutSessionAsync(
        List<PaymentLineItem> lineItems,
        string successUrl,
        string cancelUrl,
        string? customerEmail = null,
        string? orderReference = null,
        string currency = "usd")
    {
        // Map DTO sang Stripe LineItem format
        // USD là two-decimal currency → Stripe yêu cầu giá tính bằng CENTS
        // Ví dụ: $19.99 → unit_amount = 1999
        var stripeLineItems = lineItems.Select(item => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                Currency = currency.ToLower(),
                UnitAmount = (long)Math.Round(item.UnitPrice * 100), // Đổi sang cents
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = item.CourseName,
                    Images = !string.IsNullOrEmpty(item.ThumbnailUrl)
                        ? new List<string> { item.ThumbnailUrl }
                        : null
                }
            },
            Quantity = 1
        }).ToList();

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = stripeLineItems,
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            CustomerEmail = customerEmail,
            // ★ transfer_group: nhóm các Transfer theo đơn hàng
            // Sau khi thanh toán xong, dùng group này để tạo Transfer cho từng instructor
            PaymentIntentData = !string.IsNullOrEmpty(orderReference)
                ? new SessionPaymentIntentDataOptions { TransferGroup = orderReference }
                : null
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return new PaymentSessionResult
        {
            SessionUrl = session.Url,
            SessionId = session.Id
        };
    }

    /// <summary>
    /// Lấy PaymentIntent ID từ Stripe Checkout Session đã hoàn tất.
    /// Cần thiết để liên kết Transfer với PaymentIntent gốc.
    /// </summary>
    public async Task<string?> GetPaymentReferenceAsync(string sessionId)
    {
        var service = new SessionService();
        var session = await service.GetAsync(sessionId);
        return session?.PaymentIntentId;
    }

    /// <summary>
    /// ★ Lấy Charge ID từ PaymentIntent.
    /// Stripe PaymentIntent chứa list Charges — lấy charge đầu tiên (thường chỉ có 1).
    /// Charge ID dùng làm source_transaction cho Transfer.
    /// </summary>
    public async Task<string?> GetChargeIdFromPaymentIntentAsync(string paymentIntentId)
    {
        var piService = new PaymentIntentService();
        var paymentIntent = await piService.GetAsync(paymentIntentId, new PaymentIntentGetOptions
        {
            Expand = new List<string> { "latest_charge" }
        });
        return paymentIntent?.LatestChargeId;
    }

    /// <summary>
    /// ★ STRIPE CONNECT: Chuyển tiền từ platform → instructor connected account.
    /// 
    /// Flow: Student trả $19.99 vào platform → platform giữ 30% ($5.99)
    ///       → Transfer 70% ($13.99) sang instructor connected account.
    ///
    /// ★ source_transaction: Liên kết Transfer với Charge gốc.
    ///   Khi có source_transaction, Stripe cho phép transfer từ PENDING balance
    ///   (không cần đợi available balance — giải quyết lỗi "insufficient funds" trong test mode).
    ///
    /// Yêu cầu: Instructor phải có StripeAccountId (đã onboard Stripe Connect).
    /// </summary>
    public async Task CreateTransferAsync(
        decimal amount,
        string currency,
        string destinationAccountId,
        string? orderReference = null,
        string? description = null,
        string? sourceTransaction = null)
    {
        var options = new TransferCreateOptions
        {
            Amount = (long)Math.Round(amount * 100), // Đổi sang cents
            Currency = currency.ToLower(),
            Destination = destinationAccountId,
            TransferGroup = orderReference,
            Description = description,
            // ★ source_transaction: Liên kết Transfer với Charge gốc
            // Cho phép transfer từ pending balance (không cần available balance)
            SourceTransaction = sourceTransaction
        };

        var service = new TransferService();
        await service.CreateAsync(options);
    }
}
