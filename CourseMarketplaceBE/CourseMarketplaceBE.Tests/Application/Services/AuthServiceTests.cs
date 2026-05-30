using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Share.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class AuthServiceTests
    {
        // Các Mock Dependencies (Giả lập)
        private readonly IUserRepository _mockUserRepo;
        private readonly ILockoutRepository _mockLockoutRepo;
        private readonly IOtpService _mockOtpService;
        private readonly IEmailService _mockEmailService;
        private readonly IConfiguration _mockConfig;
        
        // Concrete settings
        private readonly JwtSettings _jwtSettings;

        // Service cần kiểm thử (System Under Test - SUT)
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // 1. Khởi tạo các Mock (Giả lập) bằng NSubstitute
            _mockUserRepo = Substitute.For<IUserRepository>();
            _mockLockoutRepo = Substitute.For<ILockoutRepository>();
            _mockOtpService = Substitute.For<IOtpService>();
            _mockEmailService = Substitute.For<IEmailService>();
            _mockConfig = Substitute.For<IConfiguration>();

            // 2. Khởi tạo dữ liệu cấu hình JWT thực tế
            _jwtSettings = new JwtSettings
            {
                Key = "ThisIsASecretKeyForTestingThatNeedsToBeLongEnough123!",
                Issuer = "CourseMarketplaceBE",
                Audience = "CourseMarketplaceBEClients",
                AccessTokenExpirationMinutes = 60,
                RefreshTokenExpirationDays = 7
            };

            // 3. Khởi tạo đối tượng AuthService cần kiểm thử
            _authService = new AuthService(
                _mockUserRepo,
                _mockLockoutRepo,
                _jwtSettings,
                _mockOtpService,
                _mockEmailService,
                _mockConfig
            );
        }

        [Fact]
        public async Task LoginAsync_WhenEmailDoesNotExist_ShouldReturnNull()
        {
            // Arrange (Thiết lập trạng thái ban đầu)
            var request = new LoginRequest { Email = "notfound@example.com", Password = "Password123" };
            
            // Giả lập: Tìm kiếm email này trong DB trả về null
            _mockUserRepo.GetAccountByEmailAsync(request.Email.ToLower())
                .Returns(Task.FromResult<Account?>(null));

            // Act (Thực hiện hành động kiểm thử)
            var result = await _authService.LoginAsync(request);

            // Assert (Kiểm tra kết quả)
            result.Should().BeNull();
            
            // Đảm bảo không gọi các phương thức tiếp theo khi tài khoản không tồn tại
            await _mockLockoutRepo.DidNotReceiveWithAnyArgs().GetActiveLockoutAsync(default, default!);
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIsIncorrect_ShouldReturnNull()
        {
            // Arrange
            var request = new LoginRequest { Email = "user@example.com", Password = "WrongPassword" };
            var existingAccount = new Account
            {
                AccountId = 1,
                Email = "user@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword") // Hash password đúng
            };

            _mockUserRepo.GetAccountByEmailAsync(request.Email.ToLower())
                .Returns(Task.FromResult<Account?>(existingAccount));

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_WhenAccountIsLocked_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var request = new LoginRequest { Email = "locked@example.com", Password = "CorrectPassword" };
            var existingAccount = new Account
            {
                AccountId = 42,
                Email = "locked@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword")
            };
            var activeLockout = new Lockout
            {
                LockoutId = 1,
                AccountId = 42,
                LockoutType = "account",
                LockoutEnd = DateTime.UtcNow.AddHours(2)
            };

            _mockUserRepo.GetAccountByEmailAsync(request.Email.ToLower())
                .Returns(Task.FromResult<Account?>(existingAccount));

            // Giả lập: Tài khoản này có một Lockout đang hoạt động
            _mockLockoutRepo.GetActiveLockoutAsync(existingAccount.AccountId, "account")
                .Returns(Task.FromResult<Lockout?>(activeLockout));

            // Act
            Func<Task> act = async () => await _authService.LoginAsync(request);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*Your account has been suspended until*");
        }

        [Fact]
        public async Task LoginAsync_WhenCredentialsAreValid_ShouldReturnLoginResponseWithTokens()
        {
            // Arrange
            var request = new LoginRequest { Email = "valid@example.com", Password = "CorrectPassword" };
            var existingAccount = new Account
            {
                AccountId = 10,
                Email = "valid@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
                AvatarUrl = "http://example.com/avatar.png",
                User = new User { FullName = "Nguyễn Văn A" }
            };

            _mockUserRepo.GetAccountByEmailAsync(request.Email.ToLower())
                .Returns(Task.FromResult<Account?>(existingAccount));

            // Giả lập: Không có Lockout nào
            _mockLockoutRepo.GetActiveLockoutAsync(existingAccount.AccountId, "account")
                .Returns(Task.FromResult<Lockout?>(null));

            // Giả lập: Role là "student"
            _mockUserRepo.GetRoleByAccountIdAsync(existingAccount.AccountId)
                .Returns(Task.FromResult("student"));

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result!.AccountId.Should().Be(existingAccount.AccountId);
            result.FullName.Should().Be("Nguyễn Văn A");
            result.Role.Should().Be("student");
            result.AvatarUrl.Should().Be(existingAccount.AvatarUrl);
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();

            // Xác minh (Verify) các tương tác với DB đã diễn ra đúng kỳ vọng:
            // 1. Phải cập nhật thời gian Last Login
            await _mockUserRepo.Received(1).UpdateLastLoginAsync(existingAccount.AccountId);
            
            // 2. Phải lưu Refresh Token mới vào DB
            await _mockUserRepo.Received(1).SaveRefreshTokenAsync(
                existingAccount.AccountId, 
                Arg.Any<string>(), 
                Arg.Any<DateTime>()
            );
        }
    }
}
