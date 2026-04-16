using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Share.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CourseMarketplaceBE.Application.IServices;

public class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepo;
    private readonly IFileUploadService _uploadService;

    public UserProfileService(IUserRepository userRepo, IFileUploadService uploadService)
    {
        _userRepo = userRepo;
        _uploadService = uploadService;
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
            PhoneNumber = user.UserNavigation.PhoneNumber
        };
    }

    public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        string? avatarUrl = null;

        if (request.AvatarFile != null)
        {
            avatarUrl = await _uploadService.UploadImageAsync(request.AvatarFile);
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