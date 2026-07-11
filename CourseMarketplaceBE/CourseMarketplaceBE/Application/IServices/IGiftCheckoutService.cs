using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

/// <summary>
/// Service dedicated to handling checkout and fulfillment logic for course gifts.
/// </summary>
public interface IGiftCheckoutService
{
    /// <summary>
    /// Khởi tạo checkout cho quà tặng: tạo Stripe Checkout Session với thông tin quà tặng trong metadata.
    /// </summary>
    Task<CheckoutResponse> InitiateGiftCheckoutAsync(
        int userId,
        GiftCheckoutRequest request);

    /// <summary>
    /// Khởi tạo thanh toán nhúng Stripe Elements cho Quà Tặng.
    /// </summary>
    Task<CheckoutResponse> InitiateGiftPaymentIntentAsync(
        int userId,
        GiftCheckoutRequest request);

    /// <summary>
    /// Xử lý khi thanh toán Stripe Session thành công.
    /// </summary>
    Task ProcessPaymentSuccessAsync(string sessionId, bool failOnTransferFailure = false);

    /// <summary>
    /// Xử lý khi thanh toán Stripe PaymentIntent thành công.
    /// </summary>
    Task ProcessPaymentIntentSuccessAsync(string paymentIntentId);
}
