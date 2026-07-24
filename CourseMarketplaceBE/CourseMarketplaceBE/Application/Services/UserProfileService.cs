using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Share.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

using CourseMarketplaceBE.Application.IServices;

namespace CourseMarketplaceBE.Application.Services;

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

        return MapToUserProfileResponse(user);
    }

    public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _userRepo.GetUserByIdAsync(userId);
        if (user == null || user.UserNavigation == null) return false;

        ValidateEmailUpdate(user.UserNavigation, request.Email);

        string? avatarUrl = await TryUploadAvatarAsync(userId, request.AvatarFile);
        if (request.AvatarFile != null && avatarUrl == null)
        {
            return false;
        }

        string fullName = $"{request.FirstName} {request.LastName}".Trim();
        DateOnly? dob = ParseDateOfBirth(request.DateOfBirth);

        return await _userRepo.UpdateUserProfileAsync(
            userId,
            fullName,
            request.Bio,
            dob,
            avatarUrl,
            request.PhoneNumber,
            request.ProfessionalTitle,
            request.ExpertiseCategories,
            request.LinkedinUrl,
            request.YoutubeUrl,
            request.FacebookUrl,
            request.Email
        );
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepo.GetUserByIdAsync(userId);
        if (user == null || user.UserNavigation == null)
        {
            return (false, "User profile not found.");
        }

        var validationResult = ValidatePasswordChange(user.UserNavigation, currentPassword);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        bool updated = await UpdateAccountPasswordAsync(user.UserNavigation, newPassword);

        return updated 
            ? (true, "Password changed successfully.") 
            : (false, "An error occurred while changing password.");
    }

    // ─── PRIVATE HELPERS ──────────────────────────────────────────────────────

    private static UserProfileResponse MapToUserProfileResponse(User user)
    {
        var instructor = user.Instructor;

        return new UserProfileResponse
        {
            FullName = user.FullName,
            Email = user.UserNavigation.Email,
            Username = user.UserNavigation.Username,
            AuthProvider = user.UserNavigation.AuthProvider,
            Bio = user.Bio,
            DateOfBirth = user.DateOfBirth,
            AvatarUrl = user.UserNavigation.AvatarUrl,
            PhoneNumber = user.UserNavigation.PhoneNumber,
            IsVerified = user.UserNavigation?.IsVerified ?? false,

            IsInstructor = instructor != null,
            ProfessionalTitle = instructor?.ProfessionalTitle,
            ExpertiseCategories = instructor?.ExpertiseCategories,
            LinkedinUrl = instructor?.LinkedinUrl,
            ApprovalStatus = instructor?.ApprovalStatus
        };
    }

    private static void ValidateEmailUpdate(Account account, string? requestedEmail)
    {
        if (string.IsNullOrWhiteSpace(requestedEmail))
            return;

        if (account.AuthProvider == "google" && !requestedEmail.Equals(account.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Google Account email addresses cannot be modified.");
        }

        if (!requestedEmail.ToLower().EndsWith("@gmail.com"))
        {
            throw new InvalidOperationException("Email must be a @gmail.com address.");
        }
    }

    private async Task<string?> TryUploadAvatarAsync(int userId, IFormFile? avatarFile)
    {
        if (avatarFile == null) return null;

        try
        {
            var avatarUrl = await _uploadService.UploadImageAsync(avatarFile);
            if (avatarUrl == null)
            {
                _logger.LogWarning("Avatar upload failed for user {userId}. Aborting profile update.", userId);
            }
            return avatarUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while uploading avatar for user {userId}.", userId);
            return null;
        }
    }

    private static DateOnly? ParseDateOfBirth(string? dobString)
    {
        if (DateOnly.TryParse(dobString, out var parsed))
            return parsed;
        return null;
    }

    private static (bool Success, string Message) ValidatePasswordChange(Account account, string currentPassword)
    {
        if (string.IsNullOrEmpty(account.PasswordHash))
        {
            return (false, "Your account is linked to a third-party login and does not use a password.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(currentPassword, account.PasswordHash);
        if (!isPasswordValid)
        {
            return (false, "Current password is incorrect.");
        }

        return (true, string.Empty);
    }

    private async Task<bool> UpdateAccountPasswordAsync(Account account, string newPassword)
    {
        account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        account.AccountUpdatedAt = DateTime.Now;
        
        account.RefreshToken = null;
        account.RefreshTokenExpiryTime = null;

        return await _userRepo.UpdateAccountAsync(account);
    }
}
