using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Share.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Reflection;

namespace CourseMarketplaceBE.Tests.Application.Services;

public class UserProfileServiceTests
{
    private readonly IUserRepository _mockUserRepo;
    private readonly IFileUploadService _mockUploadService;
    private readonly ILogger<UserProfileService> _mockLogger;
    private readonly UserProfileService _sut;

    public UserProfileServiceTests()
    {
        _mockUserRepo = Substitute.For<IUserRepository>();
        _mockUploadService = Substitute.For<IFileUploadService>();
        _mockLogger = Substitute.For<ILogger<UserProfileService>>();

        _sut = new UserProfileService(_mockUserRepo, _mockUploadService, _mockLogger);
    }

    [Fact]
    public async Task GetUserProfileAsync_UserNotFound_ReturnsNull()
    {
        //Arrange 1
        int userId = 1;

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns((User?)null);

        //Act
        var result = await _sut.GetUserProfileAsync(userId);

        //Assert
        result.Should().BeNull();
        await _mockUserRepo.Received(1).GetUserByIdAsync(userId);
    }

    [Fact]
    public async Task GetUserProfileAsync_UserFoundWithoutInstructor_ReturnsMappedResponse()
    {
        //Arrange 1
        int userId = 1;
        var user = new User
        {
            UserId = userId,
            FullName = "John Doe",
            Bio = "Student Bio",
            DateOfBirth = new DateOnly(2000, 1, 1),
            UserNavigation = new Account
            {
                Email = "john@example.com",
                Username = "johndoe",
                AuthProvider = "local",
                AvatarUrl = "http://avatar.com/john",
                PhoneNumber = "1234567890",
                IsVerified = true
            },
            Instructor = null
        };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);

        //Act
        var result = await _sut.GetUserProfileAsync(userId);

        //Assert
        result.Should().NotBeNull();
        result!.FullName.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
        result.Username.Should().Be("johndoe");
        result.AuthProvider.Should().Be("local");
        result.Bio.Should().Be("Student Bio");
        result.DateOfBirth.Should().Be(new DateOnly(2000, 1, 1));
        result.AvatarUrl.Should().Be("http://avatar.com/john");
        result.PhoneNumber.Should().Be("1234567890");
        result.IsVerified.Should().BeTrue();
        result.IsInstructor.Should().BeFalse();
        result.ProfessionalTitle.Should().BeNull();
    }

    [Fact]
    public async Task GetUserProfileAsync_UserFoundWithInstructor_ReturnsMappedResponse()
    {
        //Arrange 1
        int userId = 2;
        var user = new User
        {
            UserId = userId,
            FullName = "Jane Doe",
            UserNavigation = new Account
            {
                Email = "jane@example.com",
            },
            Instructor = new Instructor
            {
                ProfessionalTitle = "Pro",
                ExpertiseCategories = "Coding",
                LinkedinUrl = "http://linkedin.com/jane",
                ApprovalStatus = "approved"
            }
        };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);

        //Act
        var result = await _sut.GetUserProfileAsync(userId);

        //Assert
        result.Should().NotBeNull();
        result!.FullName.Should().Be("Jane Doe");
        result.IsInstructor.Should().BeTrue();
        result.ProfessionalTitle.Should().Be("Pro");
        result.ExpertiseCategories.Should().Be("Coding");
        result.LinkedinUrl.Should().Be("http://linkedin.com/jane");
        result.ApprovalStatus.Should().Be("approved");
    }

    [Fact]
    public async Task UpdateProfileAsync_UserNotFound_ReturnsFalse()
    {
        //Arrange 1
        int userId = 1;
        var request = new UpdateProfileRequest();

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns((User?)null);

        //Act
        var result = await _sut.UpdateProfileAsync(userId, request);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateProfileAsync_UserNavigationNull_ReturnsFalse()
    {
        //Arrange 1
        int userId = 1;
        var request = new UpdateProfileRequest();
        var user = new User { UserId = userId, UserNavigation = null! };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);

        //Act
        var result = await _sut.UpdateProfileAsync(userId, request);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateProfileAsync_GoogleUserChangesEmail_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int userId = 1;
        var request = new UpdateProfileRequest { Email = "new@gmail.com" };
        var user = new User
        {
            UserId = userId,
            UserNavigation = new Account { Email = "old@gmail.com", AuthProvider = "google" }
        };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);

        //Act
        Func<Task> act = async () => await _sut.UpdateProfileAsync(userId, request);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Google Account email addresses cannot be modified.");
    }

    [Fact]
    public async Task UpdateProfileAsync_InvalidEmailDomain_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int userId = 1;
        var request = new UpdateProfileRequest { Email = "new@yahoo.com" };
        var user = new User
        {
            UserId = userId,
            UserNavigation = new Account { Email = "old@gmail.com", AuthProvider = "local" }
        };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);

        //Act
        Func<Task> act = async () => await _sut.UpdateProfileAsync(userId, request);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Email must be a @gmail.com address.");
    }

    [Fact]
    public async Task UpdateProfileAsync_AvatarUploadReturnsNull_ReturnsFalse()
    {
        //Arrange 1
        int userId = 1;
        var request = new UpdateProfileRequest { AvatarFile = Substitute.For<IFormFile>() };
        var user = new User
        {
            UserId = userId,
            UserNavigation = new Account { Email = "old@gmail.com", AuthProvider = "local" }
        };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);
        _mockUploadService.UploadImageAsync(request.AvatarFile).Returns((string?)null);

        //Act
        var result = await _sut.UpdateProfileAsync(userId, request);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateProfileAsync_AvatarUploadThrowsException_CatchesExceptionAndReturnsFalse()
    {
        //Arrange 1
        int userId = 1;
        var request = new UpdateProfileRequest { AvatarFile = Substitute.For<IFormFile>() };
        var user = new User
        {
            UserId = userId,
            UserNavigation = new Account { Email = "old@gmail.com", AuthProvider = "local" }
        };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);
        _mockUploadService.UploadImageAsync(request.AvatarFile).Throws(new Exception("Upload failed"));

        //Act
        var result = await _sut.UpdateProfileAsync(userId, request);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateProfileAsync_ValidRequestWithAllFields_UpdatesProfileSuccessfully()
    {
        //Arrange 1
        int userId = 1;
        var request = new UpdateProfileRequest 
        { 
            FirstName = "John",
            LastName = "Doe",
            Email = "john@gmail.com",
            Bio = "New Bio",
            DateOfBirth = "2000-01-01",
            PhoneNumber = "111222333",
            ProfessionalTitle = "Pro",
            ExpertiseCategories = "Cat",
            LinkedinUrl = "lnk",
            YoutubeUrl = "yt",
            FacebookUrl = "fb",
            AvatarFile = Substitute.For<IFormFile>()
        };
        var user = new User
        {
            UserId = userId,
            UserNavigation = new Account { Email = "old@gmail.com", AuthProvider = "local" }
        };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);
        _mockUploadService.UploadImageAsync(request.AvatarFile).Returns("http://avatar.com/new");
        _mockUserRepo.UpdateUserProfileAsync(
            userId, "John Doe", "New Bio", new DateOnly(2000, 1, 1), "http://avatar.com/new", 
            "111222333", "Pro", "Cat", "lnk", "yt", "fb", "john@gmail.com"
        ).Returns(true);

        //Act
        var result = await _sut.UpdateProfileAsync(userId, request);

        //Assert
        result.Should().BeTrue();
        await _mockUserRepo.Received(1).UpdateUserProfileAsync(
            userId, "John Doe", "New Bio", new DateOnly(2000, 1, 1), "http://avatar.com/new", 
            "111222333", "Pro", "Cat", "lnk", "yt", "fb", "john@gmail.com"
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_ValidRequestWithoutAvatarFile_UpdatesProfileSuccessfully()
    {
        //Arrange 1
        int userId = 1;
        var request = new UpdateProfileRequest 
        { 
            FirstName = "Jane",
            LastName = "Doe",
            AvatarFile = null // no file
        };
        var user = new User
        {
            UserId = userId,
            UserNavigation = new Account { Email = "old@gmail.com", AuthProvider = "local" }
        };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);
        _mockUserRepo.UpdateUserProfileAsync(
            userId, "Jane Doe", null, null, null, 
            null, null, null, null, null, null, null
        ).Returns(true);

        //Act
        var result = await _sut.UpdateProfileAsync(userId, request);

        //Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProfileAsync_InvalidDateOfBirthFormat_ParsesDobAsNullAndContinues()
    {
        //Arrange 1
        int userId = 1;
        var request = new UpdateProfileRequest 
        { 
            FirstName = "Jane",
            LastName = "Doe",
            DateOfBirth = "invalid-date",
            AvatarFile = null
        };
        var user = new User
        {
            UserId = userId,
            UserNavigation = new Account { Email = "old@gmail.com", AuthProvider = "local" }
        };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);
        _mockUserRepo.UpdateUserProfileAsync(
            userId, "Jane Doe", null, null, null, 
            null, null, null, null, null, null, null
        ).Returns(true);

        //Act
        var result = await _sut.UpdateProfileAsync(userId, request);

        //Assert
        result.Should().BeTrue();
        await _mockUserRepo.Received(1).UpdateUserProfileAsync(
            userId, "Jane Doe", null, null, null, 
            null, null, null, null, null, null, null
        ); // dob is null
    }

    [Fact]
    public async Task ChangePasswordAsync_UserNotFound_ReturnsFalseAndErrorMessage()
    {
        //Arrange 1
        int userId = 1;

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns((User?)null);

        //Act
        var result = await _sut.ChangePasswordAsync(userId, "old", "new");

        //Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User profile not found.");
    }

    [Fact]
    public async Task ChangePasswordAsync_UserNavigationNull_ReturnsFalseAndErrorMessage()
    {
        //Arrange 1
        int userId = 1;
        var user = new User { UserId = userId, UserNavigation = null! };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);

        //Act
        var result = await _sut.ChangePasswordAsync(userId, "old", "new");

        //Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("User profile not found.");
    }

    [Fact]
    public async Task ChangePasswordAsync_UserHasNoPasswordHash_ReturnsFalseAndErrorMessage()
    {
        //Arrange 1
        int userId = 1;
        var user = new User { UserId = userId, UserNavigation = new Account { PasswordHash = null } };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);

        //Act
        var result = await _sut.ChangePasswordAsync(userId, "old", "new");

        //Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Your account is linked to a third-party login and does not use a password.");
    }

    [Fact]
    public async Task ChangePasswordAsync_IncorrectCurrentPassword_ReturnsFalseAndErrorMessage()
    {
        //Arrange 1
        int userId = 1;
        // Hash for "correctpassword"
        var hash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = new User { UserId = userId, UserNavigation = new Account { PasswordHash = hash } };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);

        //Act
        var result = await _sut.ChangePasswordAsync(userId, "wrongpassword", "new");

        //Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Current password is incorrect.");
    }

    [Fact]
    public async Task ChangePasswordAsync_CorrectPasswordAndRepoReturnsTrue_UpdatesPasswordAndReturnsTrue()
    {
        //Arrange 1
        int userId = 1;
        var hash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = new User { UserId = userId, UserNavigation = new Account { PasswordHash = hash, RefreshToken = "token", RefreshTokenExpiryTime = DateTime.UtcNow } };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);
        _mockUserRepo.UpdateAccountAsync(Arg.Any<Account>()).Returns(true);

        //Act
        var result = await _sut.ChangePasswordAsync(userId, "correctpassword", "newpassword");

        //Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Password changed successfully.");
        
        user.UserNavigation.RefreshToken.Should().BeNull();
        user.UserNavigation.RefreshTokenExpiryTime.Should().BeNull();
        BCrypt.Net.BCrypt.Verify("newpassword", user.UserNavigation.PasswordHash).Should().BeTrue();
        user.UserNavigation.AccountUpdatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
        
        await _mockUserRepo.Received(1).UpdateAccountAsync(user.UserNavigation);
    }

    [Fact]
    public async Task ChangePasswordAsync_CorrectPasswordAndRepoReturnsFalse_ReturnsFalseAndErrorMessage()
    {
        //Arrange 1
        int userId = 1;
        var hash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = new User { UserId = userId, UserNavigation = new Account { PasswordHash = hash } };

        //Arrange 2
        _mockUserRepo.GetUserByIdAsync(userId).Returns(user);
        _mockUserRepo.UpdateAccountAsync(Arg.Any<Account>()).Returns(false);

        //Act
        var result = await _sut.ChangePasswordAsync(userId, "correctpassword", "newpassword");

        //Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred while changing password.");
    }
}
