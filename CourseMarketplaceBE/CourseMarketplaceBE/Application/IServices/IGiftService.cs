using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IGiftService
{
    Task<GiftValidationResponse> ValidateGiftTokenAsync(string token);

    Task ClaimGiftAsync(int userId, string token);

    Task CreateGiftRecordAsync(int orderItemId, int senderId, string recipientEmail, string? recipientName, string? message, string theme, string token);

    Task<bool> IsRecipientEnrolledAsync(string email, int courseId);
}
