using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using System;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.Services
{
    public class ManagerProfileService : IManagerProfileService
    {
        private readonly IManagerRepository _managerRepo;
        private readonly IFileUploadService _uploadService;

        public ManagerProfileService(IManagerRepository managerRepo, IFileUploadService uploadService)
        {
            _managerRepo = managerRepo;
            _uploadService = uploadService;
        }

        public async Task<ManagerProfileResponse?> GetProfileAsync(int managerId)
        {
            var m = await _managerRepo.GetManagerByIdAsync(managerId);
            if (m == null) return null;

            var activeLockout = m.ManagerNavigation.Lockouts
                .Where(l => l.LockoutType == "account")
                .OrderByDescending(l => l.LockoutEnd)
                .FirstOrDefault();

            return new ManagerProfileResponse
            {
                ManagerId = m.ManagerId,
                Role = m.Role,
                DisplayName = m.DisplayName,
                FullName = m.FullName,
                PhoneNumber = m.PhoneNumber ?? m.ManagerNavigation.PhoneNumber,
                AvatarUrl = m.AvatarUrl ?? m.ManagerNavigation.AvatarUrl,
                Bio = m.Bio,
                AccountCreatedAt = m.ManagerNavigation.AccountCreatedAt,
                LockoutStart = activeLockout?.LockoutStart,
                LockoutEnd = activeLockout?.LockoutEnd,
                Email = m.ManagerNavigation.Email
            };
        }

        public async Task<bool> UpdateProfileAsync(int managerId, UpdateManagerProfileRequest request)
        {
            // Validation
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber.Length > 50)
            {
                throw new InvalidOperationException("Phone number cannot exceed 50 characters.");
            }

            string? avatarUrl = null;
            if (request.AvatarFile != null)
            {
                avatarUrl = await _uploadService.UploadImageAsync(request.AvatarFile);
                if (avatarUrl == null) return false;
            }

            return await _managerRepo.UpdateManagerProfileAsync(
                managerId,
                request.DisplayName,
                request.FullName,
                request.PhoneNumber,
                avatarUrl,
                request.Bio
            );
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int managerId, string currentPassword, string newPassword)
        {
            var m = await _managerRepo.GetManagerByIdAsync(managerId);
            if (m == null) return (false, "Manager profile not found.");

            var account = m.ManagerNavigation;
            if (string.IsNullOrEmpty(account.PasswordHash))
                return (false, "Account does not use password authentication.");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, account.PasswordHash))
                return (false, "Current password is incorrect.");

            var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            var success = await _managerRepo.UpdateAccountPasswordHashAsync(managerId, newHash);

            return success ? (true, "Password changed successfully.") : (false, "Failed to update password.");
        }
    }
}
