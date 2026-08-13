using CourseMarketplaceBE.Application.DTOs;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices;

public interface IGiftCheckoutService
{
    Task<string> CreateGiftCheckoutSessionAsync(int userId, GiftCheckoutSessionRequest request);
    Task<GiftCheckoutSessionDto> GetGiftCheckoutSessionAsync(int userId, string sessionId);
    Task<CheckoutResponse> InitiateGiftCheckoutAsync(int userId, ProcessGiftCheckoutRequest request);
    Task<CheckoutResponse> InitiateGiftPaymentIntentAsync(int userId, ProcessGiftCheckoutRequest request);
    Task ProcessPaymentSuccessAsync(string sessionId);
    Task ProcessPaymentIntentSuccessAsync(string paymentIntentId);
}
