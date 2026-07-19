using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Share.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;
using Google.Apis.Auth;

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
        private readonly IGoogleTokenValidator _mockGoogleTokenValidator;
        
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
            _mockGoogleTokenValidator = Substitute.For<IGoogleTokenValidator>();

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
                _mockConfig,
                _mockGoogleTokenValidator
            );
        }

        [Fact]
        public async Task LoginAsync_WhenEmailDoesNotExist_ShouldReturnNull()
        {
            // Arrange (Thiết lập trạng thái ban đầu)
            var request = new LoginRequest { UsernameOrEmail = "notfound@example.com", Password = "Password123" };
            
            // Giả lập: Tìm kiếm email này trong DB trả về null
            _mockUserRepo.GetAccountByEmailOrUsernameAsync(request.UsernameOrEmail.ToLower())
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
            var request = new LoginRequest { UsernameOrEmail = "user@example.com", Password = "WrongPassword" };
            var existingAccount = new Account
            {
                AccountId = 1,
                Email = "user@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword") // Hash password đúng
            };

            _mockUserRepo.GetAccountByEmailOrUsernameAsync(request.UsernameOrEmail.ToLower())
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
            var request = new LoginRequest { UsernameOrEmail = "locked@example.com", Password = "CorrectPassword" };
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

            _mockUserRepo.GetAccountByEmailOrUsernameAsync(request.UsernameOrEmail.ToLower())
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
            var request = new LoginRequest { UsernameOrEmail = "valid@example.com", Password = "CorrectPassword" };
            var existingAccount = new Account
            {
                AccountId = 10,
                Email = "valid@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
                AvatarUrl = "http://example.com/avatar.png",
                User = new User { FullName = "Nguyễn Văn A" }
            };

            _mockUserRepo.GetAccountByEmailOrUsernameAsync(request.UsernameOrEmail.ToLower())
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

        // ─── RefreshTokenAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task RefreshTokenAsync_AccountNotFound_ReturnsNull()
        {
            //Arrange 1
            var refreshToken = "invalid-token";
            var expected = (LoginResponse?)null;

            //Arrange 2
            _mockUserRepo.GetAccountByRefreshTokenAsync(refreshToken).Returns(Task.FromResult<Account?>(null));

            //Act
            var result = await _authService.RefreshTokenAsync(refreshToken);

            //Assert
            result.Should().BeNull();
            await _mockUserRepo.Received(1).GetAccountByRefreshTokenAsync(refreshToken);
        }

        [Fact]
        public async Task RefreshTokenAsync_ExpiryTimeIsNull_ReturnsNull()
        {
            //Arrange 1
            var refreshToken = "valid-token";
            var account = new Account { RefreshTokenExpiryTime = null };

            //Arrange 2
            _mockUserRepo.GetAccountByRefreshTokenAsync(refreshToken).Returns(Task.FromResult<Account?>(account));

            //Act
            var result = await _authService.RefreshTokenAsync(refreshToken);

            //Assert
            result.Should().BeNull();
            await _mockUserRepo.Received(1).GetAccountByRefreshTokenAsync(refreshToken);
        }

        [Fact]
        public async Task RefreshTokenAsync_ExpiryTimeInPast_ReturnsNull()
        {
            //Arrange 1
            var refreshToken = "valid-token";
            var account = new Account { RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-5) };

            //Arrange 2
            _mockUserRepo.GetAccountByRefreshTokenAsync(refreshToken).Returns(Task.FromResult<Account?>(account));

            //Act
            var result = await _authService.RefreshTokenAsync(refreshToken);

            //Assert
            result.Should().BeNull();
            await _mockUserRepo.Received(1).GetAccountByRefreshTokenAsync(refreshToken);
        }

        [Fact]
        public async Task RefreshTokenAsync_ValidToken_ReturnsLoginResponse()
        {
            //Arrange 1
            var refreshToken = "valid-token";
            var account = new Account 
            { 
                AccountId = 1,
                Email = "test@gmail.com",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1),
                IsVerified = true,
                User = new User { FullName = "Test" }
            };

            //Arrange 2
            _mockUserRepo.GetAccountByRefreshTokenAsync(refreshToken).Returns(Task.FromResult<Account?>(account));
            _mockUserRepo.GetRoleByAccountIdAsync(account.AccountId).Returns(Task.FromResult("student"));

            //Act
            var result = await _authService.RefreshTokenAsync(refreshToken);

            //Assert
            result.Should().NotBeNull();
            result!.AccountId.Should().Be(1);
            result.Role.Should().Be("student");
            await _mockUserRepo.Received(1).GetAccountByRefreshTokenAsync(refreshToken);
            await _mockUserRepo.Received(1).SaveRefreshTokenAsync(account.AccountId, Arg.Any<string>(), Arg.Any<DateTime>());
        }

        // ─── LogoutAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task LogoutAsync_ValidAccountId_RevokesTokenAndReturnsTrue()
        {
            //Arrange 1
            var accountId = 1;

            //Arrange 2
            // No setup needed for void methods, just verify received

            //Act
            var result = await _authService.LogoutAsync(accountId);

            //Assert
            result.Should().BeTrue();
            await _mockUserRepo.Received(1).RevokeRefreshTokenAsync(accountId);
        }

        // ─── RegisterAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task RegisterAsync_InvalidEmailDomain_ReturnsErrorMessage()
        {
            //Arrange 1
            var request = new RegisterRequest { Email = "test@yahoo.com", Password = "Pass!123", Username = "user", FullName = "User" };

            //Arrange 2
            // None

            //Act
            var result = await _authService.RegisterAsync(request);

            //Assert
            result.Should().Be("Email must be a @gmail.com address");
        }

        [Fact]
        public async Task RegisterAsync_InvalidPasswordFormat_ReturnsErrorMessage()
        {
            //Arrange 1
            var request = new RegisterRequest { Email = "test@gmail.com", Password = "password", Username = "user", FullName = "User" };

            //Arrange 2
            // None

            //Act
            var result = await _authService.RegisterAsync(request);

            //Assert
            result.Should().Be("Password must contain at least 1 special character");
        }

        [Fact]
        public async Task RegisterAsync_EmailAlreadyExists_ReturnsErrorMessage()
        {
            //Arrange 1
            var request = new RegisterRequest { Email = "test@gmail.com", Password = "Pass!123", Username = "user", FullName = "User" };

            //Arrange 2
            _mockUserRepo.IsEmailExistsAsync("test@gmail.com").Returns(Task.FromResult(true));

            //Act
            var result = await _authService.RegisterAsync(request);

            //Assert
            result.Should().Be("Email already exists");
            await _mockUserRepo.Received(1).IsEmailExistsAsync("test@gmail.com");
        }

        [Fact]
        public async Task RegisterAsync_UsernameAlreadyExists_ReturnsErrorMessage()
        {
            //Arrange 1
            var request = new RegisterRequest { Email = "test@gmail.com", Password = "Pass!123", Username = "user", FullName = "User" };

            //Arrange 2
            _mockUserRepo.IsEmailExistsAsync("test@gmail.com").Returns(Task.FromResult(false));
            _mockUserRepo.IsUsernameExistsAsync("user").Returns(Task.FromResult(true));

            //Act
            var result = await _authService.RegisterAsync(request);

            //Assert
            result.Should().Be("Username already exists");
            await _mockUserRepo.Received(1).IsEmailExistsAsync("test@gmail.com");
            await _mockUserRepo.Received(1).IsUsernameExistsAsync("user");
        }

        [Fact]
        public async Task RegisterAsync_ValidRequestRegistrationFails_ReturnsError()
        {
            //Arrange 1
            var request = new RegisterRequest { Email = "test@gmail.com", Password = "Pass!123", Username = "user", FullName = "User" };

            //Arrange 2
            _mockUserRepo.IsEmailExistsAsync("test@gmail.com").Returns(Task.FromResult(false));
            _mockUserRepo.IsUsernameExistsAsync("user").Returns(Task.FromResult(false));
            _mockUserRepo.RegisterUserAsync(Arg.Any<Account>(), Arg.Any<User>()).Returns(Task.FromResult(false));

            //Act
            var result = await _authService.RegisterAsync(request);

            //Assert
            result.Should().Be("Error");
            await _mockUserRepo.Received(1).RegisterUserAsync(Arg.Any<Account>(), Arg.Any<User>());
        }

        [Fact]
        public async Task RegisterAsync_ValidRequestRegistrationSucceeds_ReturnsSuccess()
        {
            //Arrange 1
            var request = new RegisterRequest { Email = "test@gmail.com", Password = "Pass!123", Username = "user", FullName = "User" };

            //Arrange 2
            _mockUserRepo.IsEmailExistsAsync("test@gmail.com").Returns(Task.FromResult(false));
            _mockUserRepo.IsUsernameExistsAsync("user").Returns(Task.FromResult(false));
            _mockUserRepo.RegisterUserAsync(Arg.Any<Account>(), Arg.Any<User>()).Returns(Task.FromResult(true));

            //Act
            var result = await _authService.RegisterAsync(request);

            //Assert
            result.Should().Be("Success");
            await _mockUserRepo.Received(1).RegisterUserAsync(Arg.Any<Account>(), Arg.Any<User>());
        }

        // ─── GoogleLoginAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task GoogleLoginAsync_ValidTokenNewAccountCreationFails_ReturnsNull()
        {
            //Arrange 1
            var idToken = "id-token";
            var payload = new GoogleJsonWebSignature.Payload { Email = "test@gmail.com", Name = "Test", Picture = "pic" };

            //Arrange 2
            _mockGoogleTokenValidator.ValidateAsync(idToken).Returns(Task.FromResult(payload));
            _mockUserRepo.GetAccountByEmailAsync("test@gmail.com").Returns(Task.FromResult<Account?>(null));
            _mockUserRepo.IsUsernameExistsAsync("test").Returns(Task.FromResult(false));
            _mockUserRepo.RegisterUserAsync(Arg.Any<Account>(), Arg.Any<User>()).Returns(Task.FromResult(false));

            //Act
            var result = await _authService.GoogleLoginAsync(idToken);

            //Assert
            result.Should().BeNull();
            await _mockGoogleTokenValidator.Received(1).ValidateAsync(idToken);
            await _mockUserRepo.Received(1).RegisterUserAsync(Arg.Any<Account>(), Arg.Any<User>());
        }

        [Fact]
        public async Task GoogleLoginAsync_ValidTokenNewAccountCreationSucceeds_ReturnsLoginResponse()
        {
            //Arrange 1
            var idToken = "id-token";
            var payload = new GoogleJsonWebSignature.Payload { Email = "test@gmail.com", Name = "Test", Picture = "pic" };
            var account = new Account { AccountId = 1, Email = "test@gmail.com", IsVerified = true };

            //Arrange 2
            _mockGoogleTokenValidator.ValidateAsync(idToken).Returns(Task.FromResult(payload));
            
            // First call returns null, second call returns account (after creation)
            _mockUserRepo.GetAccountByEmailAsync("test@gmail.com").Returns(Task.FromResult<Account?>(null), Task.FromResult<Account?>(account));
            
            // Simulate username conflict: first "test" exists, then "test1" doesn't
            _mockUserRepo.IsUsernameExistsAsync("test").Returns(Task.FromResult(true));
            _mockUserRepo.IsUsernameExistsAsync("test1").Returns(Task.FromResult(false));
            
            _mockUserRepo.RegisterUserAsync(Arg.Any<Account>(), Arg.Any<User>()).Returns(Task.FromResult(true));
            _mockLockoutRepo.GetActiveLockoutAsync(1, "account").Returns(Task.FromResult<Lockout?>(null));
            _mockUserRepo.GetRoleByAccountIdAsync(1).Returns(Task.FromResult("student"));

            //Act
            var result = await _authService.GoogleLoginAsync(idToken);

            //Assert
            result.Should().NotBeNull();
            result!.AccountId.Should().Be(1);
            await _mockGoogleTokenValidator.Received(1).ValidateAsync(idToken);
            await _mockUserRepo.Received(1).IsUsernameExistsAsync("test");
            await _mockUserRepo.Received(1).IsUsernameExistsAsync("test1");
            await _mockUserRepo.Received(2).GetAccountByEmailAsync("test@gmail.com");
            await _mockUserRepo.Received(1).RegisterUserAsync(Arg.Any<Account>(), Arg.Any<User>());
            await _mockUserRepo.Received(1).UpdateLastLoginAsync(1);
        }

        [Fact]
        public async Task GoogleLoginAsync_ValidTokenExistingAccountLocked_ThrowsUnauthorizedAccessException()
        {
            //Arrange 1
            var idToken = "id-token";
            var payload = new GoogleJsonWebSignature.Payload { Email = "locked@gmail.com", Name = "Test", Picture = "pic" };
            var account = new Account { AccountId = 42, Email = "locked@gmail.com", IsVerified = true };
            var activeLockout = new Lockout
            {
                LockoutId = 1,
                AccountId = 42,
                LockoutType = "account",
                LockoutEnd = DateTime.UtcNow.AddHours(2)
            };

            //Arrange 2
            _mockGoogleTokenValidator.ValidateAsync(idToken).Returns(Task.FromResult(payload));
            _mockUserRepo.GetAccountByEmailAsync("locked@gmail.com").Returns(Task.FromResult<Account?>(account));
            _mockLockoutRepo.GetActiveLockoutAsync(42, "account").Returns(Task.FromResult<Lockout?>(activeLockout));

            //Act
            Func<Task> act = async () => await _authService.GoogleLoginAsync(idToken);

            //Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*Your account has been suspended until*");
            await _mockGoogleTokenValidator.Received(1).ValidateAsync(idToken);
            await _mockUserRepo.Received(1).GetAccountByEmailAsync("locked@gmail.com");
        }

        [Fact]
        public async Task GoogleLoginAsync_ValidTokenExistingAccountValid_ReturnsLoginResponse()
        {
            //Arrange 1
            var idToken = "id-token";
            var payload = new GoogleJsonWebSignature.Payload { Email = "valid@gmail.com", Name = "Test", Picture = "pic" };
            var managerAccount = new Account 
            { 
                AccountId = 2, 
                Email = "valid@gmail.com", 
                IsVerified = true,
                Manager = new Manager { DisplayName = "AdminManager" }
            };

            //Arrange 2
            _mockGoogleTokenValidator.ValidateAsync(idToken).Returns(Task.FromResult(payload));
            _mockUserRepo.GetAccountByEmailAsync("valid@gmail.com").Returns(Task.FromResult<Account?>(managerAccount));
            _mockLockoutRepo.GetActiveLockoutAsync(2, "account").Returns(Task.FromResult<Lockout?>(null));
            _mockUserRepo.GetRoleByAccountIdAsync(2).Returns(Task.FromResult("manager"));

            //Act
            var result = await _authService.GoogleLoginAsync(idToken);

            //Assert
            result.Should().NotBeNull();
            result!.AccountId.Should().Be(2);
            result.Role.Should().Be("manager");
            result.FullName.Should().Be("AdminManager");
            await _mockGoogleTokenValidator.Received(1).ValidateAsync(idToken);
            await _mockUserRepo.Received(1).GetAccountByEmailAsync("valid@gmail.com");
            await _mockUserRepo.Received(1).UpdateLastLoginAsync(2);
        }

        // ─── SendOtpAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task SendOtpAsync_AccountNotFound_ReturnsFalse()
        {
            //Arrange 1
            var email = "notfound@gmail.com";

            //Arrange 2
            _mockUserRepo.GetAccountByEmailAsync(email).Returns(Task.FromResult<Account?>(null));

            //Act
            var result = await _authService.SendOtpAsync(email);

            //Assert
            result.Should().BeFalse();
            await _mockUserRepo.Received(1).GetAccountByEmailAsync(email);
        }

        [Fact]
        public async Task SendOtpAsync_ValidEmail_SendsEmailAndReturnsTrue()
        {
            //Arrange 1
            var email = "valid@gmail.com";
            var account = new Account { Email = email };
            var otp = "123456";

            //Arrange 2
            _mockUserRepo.GetAccountByEmailAsync(email).Returns(Task.FromResult<Account?>(account));
            _mockOtpService.GenerateOtp(email, "verify").Returns(otp);

            //Act
            var result = await _authService.SendOtpAsync(email);

            //Assert
            result.Should().BeTrue();
            await _mockUserRepo.Received(1).GetAccountByEmailAsync(email);
            _mockOtpService.Received(1).GenerateOtp(email, "verify");
            await _mockEmailService.Received(1).SendEmailAsync(email, "Email Verification", Arg.Is<string>(s => s.Contains(otp)));
        }

        // ─── VerifyEmailAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task VerifyEmailAsync_InvalidOtp_ReturnsFalse()
        {
            //Arrange 1
            var email = "test@gmail.com";
            var otp = "123456";

            //Arrange 2
            _mockOtpService.ConsumeOtp(email, otp, "verify").Returns(false);

            //Act
            var result = await _authService.VerifyEmailAsync(email, otp);

            //Assert
            result.Should().BeFalse();
            _mockOtpService.Received(1).ConsumeOtp(email, otp, "verify");
        }

        [Fact]
        public async Task VerifyEmailAsync_ValidOtp_UpdatesEmailVerifiedAndReturnsTrue()
        {
            //Arrange 1
            var email = "test@gmail.com";
            var otp = "123456";

            //Arrange 2
            _mockOtpService.ConsumeOtp(email, otp, "verify").Returns(true);
            _mockUserRepo.UpdateEmailVerifiedAsync(email).Returns(Task.FromResult(true));

            //Act
            var result = await _authService.VerifyEmailAsync(email, otp);

            //Assert
            result.Should().BeTrue();
            _mockOtpService.Received(1).ConsumeOtp(email, otp, "verify");
            await _mockUserRepo.Received(1).UpdateEmailVerifiedAsync(email);
        }

        // ─── ForgotPasswordAsync ────────────────────────────────────────────────

        [Fact]
        public async Task ForgotPasswordAsync_AccountNotFound_ReturnsNotFoundMessage()
        {
            //Arrange 1
            var email = "notfound@gmail.com";

            //Arrange 2
            _mockUserRepo.GetAccountByEmailAsync(email).Returns(Task.FromResult<Account?>(null));

            //Act
            var result = await _authService.ForgotPasswordAsync(email);

            //Assert
            result.Should().Be("Email not found");
        }

        [Fact]
        public async Task ForgotPasswordAsync_GoogleAccount_ReturnsGoogleMessage()
        {
            //Arrange 1
            var email = "google@gmail.com";
            var account = new Account { AuthProvider = AuthProvider.Google.ToValue() };

            //Arrange 2
            _mockUserRepo.GetAccountByEmailAsync(email).Returns(Task.FromResult<Account?>(account));

            //Act
            var result = await _authService.ForgotPasswordAsync(email);

            //Assert
            result.Should().Be("This account uses Google login");
        }

        [Fact]
        public async Task ForgotPasswordAsync_UnverifiedAccount_ReturnsUnverifiedMessage()
        {
            //Arrange 1
            var email = "unverified@gmail.com";
            var account = new Account { AuthProvider = AuthProvider.Local.ToValue(), IsVerified = false };

            //Arrange 2
            _mockUserRepo.GetAccountByEmailAsync(email).Returns(Task.FromResult<Account?>(account));

            //Act
            var result = await _authService.ForgotPasswordAsync(email);

            //Assert
            result.Should().Be("Email is not verified");
        }

        [Fact]
        public async Task ForgotPasswordAsync_ValidVerifiedLocalAccount_SendsOtpAndReturnsOtpSentMessage()
        {
            //Arrange 1
            var email = "valid@gmail.com";
            var account = new Account { AuthProvider = AuthProvider.Local.ToValue(), IsVerified = true };
            var otp = "123456";

            //Arrange 2
            _mockUserRepo.GetAccountByEmailAsync(email).Returns(Task.FromResult<Account?>(account));
            _mockOtpService.GenerateOtp(email, "reset").Returns(otp);

            //Act
            var result = await _authService.ForgotPasswordAsync(email);

            //Assert
            result.Should().Be("OTP sent");
            _mockOtpService.Received(1).GenerateOtp(email, "reset");
            await _mockEmailService.Received(1).SendEmailAsync(email, "Reset Password", Arg.Is<string>(s => s.Contains(otp)));
        }

        // ─── ResetPasswordAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task ResetPasswordAsync_InvalidOtp_ReturnsFalse()
        {
            //Arrange 1
            var email = "test@gmail.com";
            var otp = "123456";
            var newPassword = "NewPassword123!";

            //Arrange 2
            _mockOtpService.ConsumeOtp(email, otp, "reset").Returns(false);

            //Act
            var result = await _authService.ResetPasswordAsync(email, otp, newPassword);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ResetPasswordAsync_AccountNotFound_ReturnsFalse()
        {
            //Arrange 1
            var email = "notfound@gmail.com";
            var otp = "123456";
            var newPassword = "NewPassword123!";

            //Arrange 2
            _mockOtpService.ConsumeOtp(email, otp, "reset").Returns(true);
            _mockUserRepo.GetAccountByEmailAsync(email).Returns(Task.FromResult<Account?>(null));

            //Act
            var result = await _authService.ResetPasswordAsync(email, otp, newPassword);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ResetPasswordAsync_GoogleAccount_ReturnsFalse()
        {
            //Arrange 1
            var email = "google@gmail.com";
            var otp = "123456";
            var newPassword = "NewPassword123!";
            var account = new Account { AuthProvider = AuthProvider.Google.ToValue() };

            //Arrange 2
            _mockOtpService.ConsumeOtp(email, otp, "reset").Returns(true);
            _mockUserRepo.GetAccountByEmailAsync(email).Returns(Task.FromResult<Account?>(account));

            //Act
            var result = await _authService.ResetPasswordAsync(email, otp, newPassword);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ResetPasswordAsync_ValidLocalAccountAndOtp_UpdatesPasswordAndReturnsTrue()
        {
            //Arrange 1
            var email = "valid@gmail.com";
            var otp = "123456";
            var newPassword = "NewPassword123!";
            var account = new Account { AuthProvider = AuthProvider.Local.ToValue() };

            //Arrange 2
            _mockOtpService.ConsumeOtp(email, otp, "reset").Returns(true);
            _mockUserRepo.GetAccountByEmailAsync(email).Returns(Task.FromResult<Account?>(account));

            //Act
            var result = await _authService.ResetPasswordAsync(email, otp, newPassword);

            //Assert
            result.Should().BeTrue();
            await _mockUserRepo.Received(1).UpdateAccountAsync(Arg.Is<Account>(a => a == account));
        }

        // ─── VerifyOtpForReset ──────────────────────────────────────────────────

        [Fact]
        public void VerifyOtpForReset_ValidOtp_ReturnsTrue()
        {
            //Arrange 1
            var email = "test@gmail.com";
            var otp = "123456";

            //Arrange 2
            _mockOtpService.ValidateOtp(email, otp, "reset").Returns(true);

            //Act
            var result = _authService.VerifyOtpForReset(email, otp);

            //Assert
            result.Should().BeTrue();
            _mockOtpService.Received(1).ValidateOtp(email, otp, "reset");
        }

        [Fact]
        public void VerifyOtpForReset_InvalidOtp_ReturnsFalse()
        {
            //Arrange 1
            var email = "test@gmail.com";
            var otp = "123456";

            //Arrange 2
            _mockOtpService.ValidateOtp(email, otp, "reset").Returns(false);

            //Act
            var result = _authService.VerifyOtpForReset(email, otp);

            //Assert
            result.Should().BeFalse();
            _mockOtpService.Received(1).ValidateOtp(email, otp, "reset");
        }

        // ─── IsEmailVerifiedAsync ───────────────────────────────────────────────

        [Fact]
        public async Task IsEmailVerifiedAsync_AccountNotFound_ReturnsFalse()
        {
            //Arrange 1
            var userId = 1;

            //Arrange 2
            _mockUserRepo.GetAccountByIdAsync(userId).Returns(Task.FromResult<Account?>(null));

            //Act
            var result = await _authService.IsEmailVerifiedAsync(userId);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsEmailVerifiedAsync_AccountFound_ReturnsIsVerifiedStatus()
        {
            //Arrange 1
            var userId = 1;
            var account = new Account { IsVerified = true };

            //Arrange 2
            _mockUserRepo.GetAccountByIdAsync(userId).Returns(Task.FromResult<Account?>(account));

            //Act
            var result = await _authService.IsEmailVerifiedAsync(userId);

            //Assert
            result.Should().BeTrue();
        }
    }
}
