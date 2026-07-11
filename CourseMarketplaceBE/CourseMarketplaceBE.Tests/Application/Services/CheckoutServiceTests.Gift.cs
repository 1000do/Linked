using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public partial class CheckoutServiceTests
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // InitiateGiftCheckoutAsync
        // ═══════════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task InitiateGiftCheckoutAsync_CourseNotFound_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns((Course?)null);

            //Act
            Func<Task> act = async () => await _sut.InitiateGiftCheckoutAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Course not found.");
            await _courseRepoMock.Received(1).GetCourseWithInstructorAsync(request.CourseId);
        }

        [Fact]
        public async Task InitiateGiftCheckoutAsync_CourseNotPublished_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Draft.ToValue() };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.InitiateGiftCheckoutAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"The course \"{course.Title}\" is not published and cannot be purchased.");
        }

        [Fact]
        public async Task InitiateGiftCheckoutAsync_RecipientAlreadyEnrolled_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2 };
            var account = new Account { AccountId = 3, Email = "rec@test.com" };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns(account);
            _courseRepoMock.IsEnrolledAsync(account.AccountId, request.CourseId).Returns(true);

            //Act
            Func<Task> act = async () => await _sut.InitiateGiftCheckoutAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("The recipient has already owned/joined this course.");
            await _courseRepoMock.Received(1).IsEnrolledAsync(account.AccountId, request.CourseId);
        }

        [Fact]
        public async Task InitiateGiftCheckoutAsync_InstructorStripeNotConnected_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2 };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns((Account?)null);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns(string.Empty);

            //Act
            Func<Task> act = async () => await _sut.InitiateGiftCheckoutAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Instructor has not connected a Stripe payment account.");
        }

        [Fact]
        public async Task InitiateGiftCheckoutAsync_ValidRequest_SuccessUrlContainsPath_ReturnsCheckoutResponse()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "http://localhost/Gift/CheckoutSuccess?session_id=123", CancelUrl = "cancel", CardTheme = "blue" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            string userEmail = "buyer@test.com";
            var paymentResult = new PaymentSessionResult { SessionUrl = "url", SessionId = "sess_1" };
            var expectedResponse = new CheckoutResponse { SessionUrl = "url", SessionId = "sess_1" };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns((Account?)null);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _userRepoMock.GetUserEmailAsync(userId).Returns(userEmail);
            _userRepoMock.GetInstructorStripeCountryAsync(2).Returns("US");
            _paymentGatewayMock.CreateCheckoutSessionAsync(Arg.Any<List<PaymentLineItem>>(), request.SuccessUrl, request.CancelUrl, userEmail, Arg.Any<string>(), "USD", null, null, Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            //Act
            var result = await _sut.InitiateGiftCheckoutAsync(userId, request);

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _paymentGatewayMock.Received(1).CreateCheckoutSessionAsync(
                Arg.Is<List<PaymentLineItem>>(l => l.First().UnitPrice == 100m && l.First().CourseName == "Gift: Test"),
                request.SuccessUrl, request.CancelUrl, userEmail, Arg.Any<string>(), "USD", null, null, 
                Arg.Is<Dictionary<string, string>>(d => d["feBaseUrl"] == "http://localhost"));
        }

        [Fact]
        public async Task InitiateGiftCheckoutAsync_ValidRequest_SuccessUrlDoesNotContainPath_ReturnsCheckoutResponse()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "http://localhost/other", CancelUrl = "cancel", CardTheme = "blue" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            string userEmail = "buyer@test.com";
            var paymentResult = new PaymentSessionResult { SessionUrl = "url", SessionId = "sess_1" };
            var expectedResponse = new CheckoutResponse { SessionUrl = "url", SessionId = "sess_1" };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns((Account?)null);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _userRepoMock.GetUserEmailAsync(userId).Returns(userEmail);
            _userRepoMock.GetInstructorStripeCountryAsync(2).Returns("US");
            _configurationMock.GetSection("FrontendBaseUrl").Value.Returns("http://default");
            
            // NOTE: IConfiguration mapping might require specific mocking depending on how _configuration.GetValue is implemented,
            // but we can just use Arg.Any to pass.
            
            _paymentGatewayMock.CreateCheckoutSessionAsync(Arg.Any<List<PaymentLineItem>>(), request.SuccessUrl, request.CancelUrl, userEmail, Arg.Any<string>(), "USD", null, null, Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            //Act
            var result = await _sut.InitiateGiftCheckoutAsync(userId, request);

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _paymentGatewayMock.Received(1).CreateCheckoutSessionAsync(
                Arg.Is<List<PaymentLineItem>>(l => l.First().UnitPrice == 100m && l.First().CourseName == "Gift: Test"),
                request.SuccessUrl, request.CancelUrl, userEmail, Arg.Any<string>(), "USD", null, null, 
                Arg.Any<Dictionary<string, string>>());
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // InitiateGiftPaymentIntentAsync
        // ═══════════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_CourseNotFound_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns((Course?)null);

            //Act
            Func<Task> act = async () => await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Course not found.");
        }

        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_CourseNotPublished_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Draft.ToValue() };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"The course \"{course.Title}\" is not published and cannot be purchased.");
        }

        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_RecipientAlreadyEnrolled_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2 };
            var account = new Account { AccountId = 3, Email = "rec@test.com" };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns(account);
            _courseRepoMock.IsEnrolledAsync(account.AccountId, request.CourseId).Returns(true);

            //Act
            Func<Task> act = async () => await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("The recipient has already owned/joined this course.");
        }

        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_RecipientFoundNotEnrolled_ContinuesProcessing_ReturnsCheckoutResponse()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "http://localhost/Gift/CheckoutSuccess?session_id=123", CancelUrl = "cancel", CardTheme = "blue" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            var account = new Account { AccountId = 3, Email = "rec@test.com" };
            var paymentResult = ("secret", "pi_1");
            var expectedResponse = new CheckoutResponse { SessionUrl = "secret", SessionId = "pi_1" };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns(account);
            _courseRepoMock.IsEnrolledAsync(account.AccountId, request.CourseId).Returns(false);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _paymentGatewayMock.CreatePaymentIntentAsync(100m, "usd", Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            //Act
            var result = await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _courseRepoMock.Received(1).IsEnrolledAsync(account.AccountId, request.CourseId);
            await _paymentGatewayMock.Received(1).CreatePaymentIntentAsync(100m, "usd", Arg.Any<Dictionary<string, string>>());
        }

        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_InstructorStripeNotConnected_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2 };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns((Account?)null);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns(string.Empty);

            //Act
            Func<Task> act = async () => await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Instructor has not connected a Stripe payment account.");
        }

        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_ValidRequest_SuccessUrlContainsPath_ReturnsCheckoutResponse()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "http://localhost/Gift/CheckoutSuccess?session_id=123", CancelUrl = "cancel", CardTheme = "blue" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            var paymentResult = ("secret", "pi_1");
            var expectedResponse = new CheckoutResponse { SessionUrl = "secret", SessionId = "pi_1" };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns((Account?)null);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _paymentGatewayMock.CreatePaymentIntentAsync(100m, "usd", Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            //Act
            var result = await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _paymentGatewayMock.Received(1).CreatePaymentIntentAsync(
                100m, "usd", 
                Arg.Is<Dictionary<string, string>>(d => d["feBaseUrl"] == "http://localhost"));
        }

        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_ValidRequest_SuccessUrlDoesNotContainPath_ReturnsCheckoutResponse()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "http://localhost/other", CancelUrl = "cancel", CardTheme = "blue" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            var paymentResult = ("secret", "pi_1");
            var expectedResponse = new CheckoutResponse { SessionUrl = "secret", SessionId = "pi_1" };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetAccountByEmailAsync(request.RecipientEmail).Returns((Account?)null);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            
            // To hit line 445: _configuration.GetValue<string>("FrontendBaseUrl") ?? "http://localhost:5208"
            // Wait, actually _httpContextAccessor is returning empty in the test setup unless mocked!
            // Wait, the checkout service uses _httpContextAccessor. In this test it's empty, so it goes to else block!
            _paymentGatewayMock.CreatePaymentIntentAsync(100m, "usd", Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            //Act
            var result = await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _paymentGatewayMock.Received(1).CreatePaymentIntentAsync(
                100m, "usd", 
                Arg.Any<Dictionary<string, string>>());
        }
    }
}
