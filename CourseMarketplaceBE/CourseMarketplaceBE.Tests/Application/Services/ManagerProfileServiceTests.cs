using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Application.IServices;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class ManagerProfileServiceTests
    {
        private readonly IManagerRepository _managerRepoMock;
        private readonly IFileUploadService _uploadServiceMock;
        private readonly ManagerProfileService _sut;

        public ManagerProfileServiceTests()
        {
            _managerRepoMock = Substitute.For<IManagerRepository>();
            _uploadServiceMock = Substitute.For<IFileUploadService>();
            _sut = new ManagerProfileService(_managerRepoMock, _uploadServiceMock);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // GetProfileAsync
        // ═══════════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetProfileAsync_ManagerNotFound_ReturnsNull()
        {
            // Arrange
            _managerRepoMock.GetManagerByIdAsync(Arg.Any<int>()).Returns((Manager?)null);

            // Act
            var result = await _sut.GetProfileAsync(1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProfileAsync_ManagerFound_ReturnsProfileWithLockout()
        {
            // Arrange
            int managerId = 1;
            var lockout = new Lockout
            {
                LockoutType = "account",
                LockoutStart = DateTime.UtcNow.AddDays(-1),
                LockoutEnd = DateTime.UtcNow.AddDays(1)
            };
            
            var account = new Account
            {
                AccountId = 1,
                PhoneNumber = "123456",
                AvatarUrl = "acc_avatar.jpg",
                AccountCreatedAt = DateTime.UtcNow.AddYears(-1),
                Email = "admin@test.com",
                Lockouts = new List<Lockout> { lockout }
            };

            var manager = new Manager
            {
                ManagerId = managerId,
                Role = "admin",
                DisplayName = "Admin User",
                FullName = "Admin Full Name",
                PhoneNumber = "654321", // overrides account phone
                AvatarUrl = "man_avatar.jpg", // overrides account avatar
                Bio = "Admin Bio",
                ManagerNavigation = account
            };

            _managerRepoMock.GetManagerByIdAsync(managerId).Returns(manager);

            // Act
            var result = await _sut.GetProfileAsync(managerId);

            // Assert
            result.Should().NotBeNull();
            result!.ManagerId.Should().Be(managerId);
            result.Role.Should().Be("admin");
            result.DisplayName.Should().Be("Admin User");
            result.FullName.Should().Be("Admin Full Name");
            result.PhoneNumber.Should().Be("654321");
            result.AvatarUrl.Should().Be("man_avatar.jpg");
            result.Bio.Should().Be("Admin Bio");
            result.Email.Should().Be("admin@test.com");
            result.LockoutStart.Should().Be(lockout.LockoutStart);
            result.LockoutEnd.Should().Be(lockout.LockoutEnd);
        }

        [Fact]
        public async Task GetProfileAsync_ManagerFoundNoLockout_FallbackToAccountDetails()
        {
            // Arrange
            int managerId = 1;
            
            var account = new Account
            {
                AccountId = 1,
                PhoneNumber = "123456",
                AvatarUrl = "acc_avatar.jpg",
                AccountCreatedAt = DateTime.UtcNow.AddYears(-1),
                Email = "admin@test.com",
                Lockouts = new List<Lockout>() // No lockouts
            };

            var manager = new Manager
            {
                ManagerId = managerId,
                Role = "moderator",
                DisplayName = "Mod User",
                FullName = "Mod Full Name",
                PhoneNumber = null, // fallback to account
                AvatarUrl = null, // fallback to account
                Bio = "Mod Bio",
                ManagerNavigation = account
            };

            _managerRepoMock.GetManagerByIdAsync(managerId).Returns(manager);

            // Act
            var result = await _sut.GetProfileAsync(managerId);

            // Assert
            result.Should().NotBeNull();
            result!.PhoneNumber.Should().Be("123456");
            result.AvatarUrl.Should().Be("acc_avatar.jpg");
            result.LockoutStart.Should().BeNull();
            result.LockoutEnd.Should().BeNull();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // UpdateProfileAsync
        // ═══════════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task UpdateProfileAsync_PhoneNumberExceeds50_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new UpdateManagerProfileRequest
            {
                PhoneNumber = new string('1', 51)
            };

            // Act
            Func<Task> act = async () => await _sut.UpdateProfileAsync(1, request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Phone number cannot exceed 50 characters.");
        }

        [Fact]
        public async Task UpdateProfileAsync_AvatarUploadFails_ReturnsFalse()
        {
            // Arrange
            var fileMock = Substitute.For<IFormFile>();
            var request = new UpdateManagerProfileRequest
            {
                PhoneNumber = "123",
                AvatarFile = fileMock
            };
            
            _uploadServiceMock.UploadImageAsync(fileMock).Returns((string?)null);

            // Act
            var result = await _sut.UpdateProfileAsync(1, request);

            // Assert
            result.Should().BeFalse();
            await _managerRepoMock.DidNotReceiveWithAnyArgs().UpdateManagerProfileAsync(default, default, default, default, default, default);
        }

        [Fact]
        public async Task UpdateProfileAsync_AvatarUploadSucceeds_UpdatesProfile()
        {
            // Arrange
            var fileMock = Substitute.For<IFormFile>();
            var request = new UpdateManagerProfileRequest
            {
                DisplayName = "New Name",
                FullName = "New FullName",
                PhoneNumber = "123",
                Bio = "New Bio",
                AvatarFile = fileMock
            };
            
            _uploadServiceMock.UploadImageAsync(fileMock).Returns("new_avatar_url");
            _managerRepoMock.UpdateManagerProfileAsync(1, "New Name", "New FullName", "123", "new_avatar_url", "New Bio").Returns(true);

            // Act
            var result = await _sut.UpdateProfileAsync(1, request);

            // Assert
            result.Should().BeTrue();
            await _managerRepoMock.Received(1).UpdateManagerProfileAsync(1, "New Name", "New FullName", "123", "new_avatar_url", "New Bio");
        }

        [Fact]
        public async Task UpdateProfileAsync_NoAvatar_UpdatesProfile()
        {
            // Arrange
            var request = new UpdateManagerProfileRequest
            {
                DisplayName = "Name",
                AvatarFile = null
            };
            
            _managerRepoMock.UpdateManagerProfileAsync(1, "Name", null, null, null, null).Returns(true);

            // Act
            var result = await _sut.UpdateProfileAsync(1, request);

            // Assert
            result.Should().BeTrue();
            await _managerRepoMock.Received(1).UpdateManagerProfileAsync(1, "Name", null, null, null, null);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ChangePasswordAsync
        // ═══════════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task ChangePasswordAsync_ManagerNotFound_ReturnsFalseAndMessage()
        {
            // Arrange
            _managerRepoMock.GetManagerByIdAsync(1).Returns((Manager?)null);

            // Act
            var (success, message) = await _sut.ChangePasswordAsync(1, "old", "new");

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Manager profile not found.");
        }

        [Fact]
        public async Task ChangePasswordAsync_NoPasswordHash_ReturnsFalseAndMessage()
        {
            // Arrange
            var manager = new Manager { ManagerNavigation = new Account { PasswordHash = null } };
            _managerRepoMock.GetManagerByIdAsync(1).Returns(manager);

            // Act
            var (success, message) = await _sut.ChangePasswordAsync(1, "old", "new");

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Account does not use password authentication.");
        }

        [Fact]
        public async Task ChangePasswordAsync_IncorrectPassword_ReturnsFalseAndMessage()
        {
            // Arrange
            string actualHash = BCrypt.Net.BCrypt.HashPassword("correct_old_pass");
            var manager = new Manager { ManagerNavigation = new Account { PasswordHash = actualHash } };
            _managerRepoMock.GetManagerByIdAsync(1).Returns(manager);

            // Act
            var (success, message) = await _sut.ChangePasswordAsync(1, "wrong_old_pass", "new");

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Current password is incorrect.");
        }

        [Fact]
        public async Task ChangePasswordAsync_UpdateFails_ReturnsFalseAndMessage()
        {
            // Arrange
            string actualHash = BCrypt.Net.BCrypt.HashPassword("correct_old_pass");
            var manager = new Manager { ManagerNavigation = new Account { PasswordHash = actualHash } };
            _managerRepoMock.GetManagerByIdAsync(1).Returns(manager);
            
            _managerRepoMock.UpdateAccountPasswordHashAsync(1, Arg.Any<string>()).Returns(false);

            // Act
            var (success, message) = await _sut.ChangePasswordAsync(1, "correct_old_pass", "new");

            // Assert
            success.Should().BeFalse();
            message.Should().Be("Failed to update password.");
        }

        [Fact]
        public async Task ChangePasswordAsync_Success_ReturnsTrueAndMessage()
        {
            // Arrange
            string actualHash = BCrypt.Net.BCrypt.HashPassword("correct_old_pass");
            var manager = new Manager { ManagerNavigation = new Account { PasswordHash = actualHash } };
            _managerRepoMock.GetManagerByIdAsync(1).Returns(manager);
            
            _managerRepoMock.UpdateAccountPasswordHashAsync(1, Arg.Any<string>()).Returns(true);

            // Act
            var (success, message) = await _sut.ChangePasswordAsync(1, "correct_old_pass", "new");

            // Assert
            success.Should().BeTrue();
            message.Should().Be("Password changed successfully.");
        }
    }
}
