using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class GiftCheckoutServiceTests
    {
        private readonly ICheckoutRepository _repoMock;
        private readonly IPaymentGatewayService _paymentGatewayMock;
        private readonly ILogger<GiftCheckoutService> _loggerMock;
        private readonly IHubContext<FinanceHub> _hubContextMock;
        private readonly INotificationService _notificationServiceMock;
        private readonly ICourseRepository _courseRepoMock;
        private readonly IUserRepository _userRepoMock;
        private readonly IAdminFinanceService _adminFinanceServiceMock;
        private readonly IGiftRepository _giftRepoMock;
        private readonly IEmailService _emailServiceMock;
        private readonly IConfiguration _configurationMock;
        private readonly GiftCheckoutService _sut;

        public GiftCheckoutServiceTests()
        {
            _repoMock = Substitute.For<ICheckoutRepository>();
            _paymentGatewayMock = Substitute.For<IPaymentGatewayService>();
            _loggerMock = Substitute.For<ILogger<GiftCheckoutService>>();
            _hubContextMock = Substitute.For<IHubContext<FinanceHub>>();
            _notificationServiceMock = Substitute.For<INotificationService>();
            _courseRepoMock = Substitute.For<ICourseRepository>();
            _userRepoMock = Substitute.For<IUserRepository>();
            _adminFinanceServiceMock = Substitute.For<IAdminFinanceService>();
            _giftRepoMock = Substitute.For<IGiftRepository>();
            _emailServiceMock = Substitute.For<IEmailService>();
            _configurationMock = Substitute.For<IConfiguration>();

            _sut = new GiftCheckoutService(
                _repoMock,
                _paymentGatewayMock,
                _loggerMock,
                _hubContextMock,
                _adminFinanceServiceMock,
                _notificationServiceMock,
                _courseRepoMock,
                _userRepoMock,
                _giftRepoMock,
                _emailServiceMock,
                _configurationMock
            );
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // InitiateGiftCheckoutAsync
        // ═══════════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task InitiateGiftCheckoutAsync_CourseNotFound_ThrowsInvalidOperationException()
        {
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns((Course?)null);

            Func<Task> act = async () => await _sut.InitiateGiftCheckoutAsync(userId, request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Course not found.");
            await _courseRepoMock.Received(1).GetCourseWithInstructorAsync(request.CourseId);
        }

        [Fact]
        public async Task InitiateGiftCheckoutAsync_CourseNotPublished_ThrowsInvalidOperationException()
        {
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Draft.ToValue() };
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);

            Func<Task> act = async () => await _sut.InitiateGiftCheckoutAsync(userId, request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"The course \"{course.Title}\" is not published and cannot be purchased.");
        }

        [Fact]
        public async Task InitiateGiftCheckoutAsync_RecipientAlreadyEnrolled_ThrowsInvalidOperationException()
        {
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2 };
            var account = new Account { AccountId = 3, Email = "rec@test.com" };
            
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns(account);
            _courseRepoMock.IsEnrolledAsync(account.AccountId, request.CourseId).Returns(true);

            Func<Task> act = async () => await _sut.InitiateGiftCheckoutAsync(userId, request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("The recipient has already owned/joined this course.");
        }

        [Fact]
        public async Task InitiateGiftCheckoutAsync_InstructorStripeNotConnected_ThrowsInvalidOperationException()
        {
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2 };

            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns((Account?)null);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns(string.Empty);

            Func<Task> act = async () => await _sut.InitiateGiftCheckoutAsync(userId, request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Instructor has not connected a Stripe payment account.");
        }

        [Fact]
        public async Task InitiateGiftCheckoutAsync_ValidRequest_SuccessUrlContainsPath_ReturnsCheckoutResponse()
        {
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "http://localhost/Gift/CheckoutSuccess?session_id=123", CancelUrl = "cancel", CardTheme = "blue" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            string userEmail = "buyer@test.com";
            var paymentResult = new PaymentSessionResult { SessionUrl = "url", SessionId = "sess_1" };
            var expectedResponse = new CheckoutResponse { SessionUrl = "url", SessionId = "sess_1" };

            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns((Account?)null);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _userRepoMock.GetUserEmailAsync(userId).Returns(userEmail);
            _userRepoMock.GetInstructorStripeCountryAsync(2).Returns("US");
            _paymentGatewayMock.CreateCheckoutSessionAsync(Arg.Any<List<PaymentLineItem>>(), request.SuccessUrl, request.CancelUrl, userEmail, Arg.Any<string>(), "USD", null, null, Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            var result = await _sut.InitiateGiftCheckoutAsync(userId, request);

            result.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task InitiateGiftCheckoutAsync_ValidRequest_SuccessUrlDoesNotContainPath_ReturnsCheckoutResponse()
        {
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "http://localhost/other", CancelUrl = "cancel", CardTheme = "blue" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            string userEmail = "buyer@test.com";
            var paymentResult = new PaymentSessionResult { SessionUrl = "url", SessionId = "sess_1" };
            var expectedResponse = new CheckoutResponse { SessionUrl = "url", SessionId = "sess_1" };

            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns((Account?)null);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _userRepoMock.GetUserEmailAsync(userId).Returns(userEmail);
            _userRepoMock.GetInstructorStripeCountryAsync(2).Returns("US");
            _configurationMock.GetSection("FrontendBaseUrl").Value.Returns("http://default");
            
            _paymentGatewayMock.CreateCheckoutSessionAsync(Arg.Any<List<PaymentLineItem>>(), request.SuccessUrl, request.CancelUrl, userEmail, Arg.Any<string>(), "USD", null, null, Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            var result = await _sut.InitiateGiftCheckoutAsync(userId, request);

            result.Should().BeEquivalentTo(expectedResponse);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // InitiateGiftPaymentIntentAsync
        // ═══════════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_CourseNotFound_ThrowsInvalidOperationException()
        {
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns((Course?)null);

            Func<Task> act = async () => await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Course not found.");
        }

        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_CourseNotPublished_ThrowsInvalidOperationException()
        {
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Draft.ToValue() };
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);

            Func<Task> act = async () => await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"The course \"{course.Title}\" is not published and cannot be purchased.");
        }

        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_RecipientAlreadyEnrolled_ThrowsInvalidOperationException()
        {
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2 };
            var account = new Account { AccountId = 3, Email = "rec@test.com" };

            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns(account);
            _courseRepoMock.IsEnrolledAsync(account.AccountId, request.CourseId).Returns(true);

            Func<Task> act = async () => await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("The recipient has already owned/joined this course.");
        }

        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_RecipientFoundNotEnrolled_ContinuesProcessing_ReturnsCheckoutResponse()
        {
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "http://localhost/Gift/CheckoutSuccess?session_id=123", CancelUrl = "cancel", CardTheme = "blue" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            var account = new Account { AccountId = 3, Email = "rec@test.com" };
            var paymentResult = ("secret", "pi_1");
            var expectedResponse = new CheckoutResponse { SessionUrl = "secret", SessionId = "pi_1" };

            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns(account);
            _courseRepoMock.IsEnrolledAsync(account.AccountId, request.CourseId).Returns(false);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _paymentGatewayMock.CreatePaymentIntentAsync(100m, "usd", Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            var result = await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            result.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_InstructorStripeNotConnected_ThrowsInvalidOperationException()
        {
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2 };

            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns((Account?)null);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns(string.Empty);

            Func<Task> act = async () => await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Instructor has not connected a Stripe payment account.");
        }

        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_ValidRequest_SuccessUrlContainsPath_ReturnsCheckoutResponse()
        {
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "http://localhost/Gift/CheckoutSuccess?session_id=123", CancelUrl = "cancel", CardTheme = "blue" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            var paymentResult = ("secret", "pi_1");
            var expectedResponse = new CheckoutResponse { SessionUrl = "secret", SessionId = "pi_1" };

            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns((Account?)null);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _paymentGatewayMock.CreatePaymentIntentAsync(100m, "usd", Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            var result = await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            result.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_ValidRequest_SuccessUrlDoesNotContainPath_ReturnsCheckoutResponse()
        {
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "http://localhost/other", CancelUrl = "cancel", CardTheme = "blue" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            var paymentResult = ("secret", "pi_1");
            var expectedResponse = new CheckoutResponse { SessionUrl = "secret", SessionId = "pi_1" };

            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns((Account?)null);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            
            _paymentGatewayMock.CreatePaymentIntentAsync(100m, "usd", Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            var result = await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
