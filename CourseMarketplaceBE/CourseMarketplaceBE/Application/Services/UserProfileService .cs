using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Share.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CourseMarketplaceBE.Application.IServices;

public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepo;
    private readonly IFileUploadService _uploadService;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(IUserRepository userRepo, IFileUploadService uploadService, ILogger<UserProfileService> logger)
    {
        _userRepo = userRepo;
        _uploadService = uploadService;
        _logger = logger;
    }

    public async Task<UserProfileResponse?> GetUserProfileAsync(int userId)
    {
        var user = await _userRepo.GetUserByIdAsync(userId);
        if (user == null) return null;

        return new UserProfileResponse
        {
            FullName = user.FullName,
            Email = user.UserNavigation.Email,
            Bio = user.Bio,
            DateOfBirth = user.DateOfBirth,
            AvatarUrl = user.UserNavigation.AvatarUrl,
            PhoneNumber = user.UserNavigation.PhoneNumber,
            IsVerified = user.UserNavigation?.IsVerified ?? false
        };
    }

    public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        string? avatarUrl = null;

        if (request.AvatarFile != null)
        {
            try
            {
                avatarUrl = await _uploadService.UploadImageAsync(request.AvatarFile);

                // If upload was attempted but failed, stop the update and return false
                if (avatarUrl == null)
                {
                    _logger.LogWarning("Avatar upload failed for user {userId}. Aborting profile update.", userId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while uploading avatar for user {userId}.", userId);
                return false;
            }
        }

        string fullName = $"{request.FirstName} {request.LastName}".Trim();

        DateOnly? dob = null;
        if (DateOnly.TryParse(request.DateOfBirth, out var parsed))
            dob = parsed;

        return await _userRepo.UpdateUserProfileAsync(
            userId,
            fullName,
            request.Bio,
            dob,
            avatarUrl,
            request.PhoneNumber
        );
    }
}