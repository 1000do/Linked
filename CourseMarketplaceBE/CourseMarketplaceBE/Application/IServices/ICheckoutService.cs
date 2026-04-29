using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

/// <summary>
/// SOLID — SRP: Interface chuyên xử lý luồng nghiệp vụ Checkout.
/// Tách biệt khỏi ICartService (giỏ hàng) — mỗi interface 1 trách nhiệm.
/// </summary>
public interface ICheckoutService
{
    /// <summary>
    /// Bước 1: Tạo Order → Gọi Payment Gateway → Tạo Transaction.
    /// Toàn bộ nằm trong 1 DB transaction (Unit of Work).
    /// </summary>
    /// <param name="userId">ID người dùng (từ JWT Cookie).</param>
    /// <param name="couponCode">Mã giảm giá (null = không dùng).</param>
    /// <param name="successUrl">URL redirect khi Stripe thanh toán xong.</param>
    /// <param name="cancelUrl">URL redirect khi user hủy.</param>
    /// <returns>CheckoutResponse chứa Stripe Session URL.</returns>
    Task<CheckoutResponse> InitiateCheckoutAsync(
        int userId,
        string? couponCode,
        string successUrl,
        string cancelUrl);

    /// <summary>
    /// Bước 2: Xử lý khi Stripe báo thành công.
    /// Update Transaction → Update Order → Tạo Enrollment → Tính Payout → Xóa Cart.
    /// </summary>
    /// <param name="sessionId">Stripe Session ID từ success callback.</param>
    Task ProcessPaymentSuccessAsync(string sessionId);
}
