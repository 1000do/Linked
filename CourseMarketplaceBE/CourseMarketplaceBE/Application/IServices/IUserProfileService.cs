using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IUserProfileService
{
    Task<UserProfileResponse?> GetUserProfileAsync(int userId);
    Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}