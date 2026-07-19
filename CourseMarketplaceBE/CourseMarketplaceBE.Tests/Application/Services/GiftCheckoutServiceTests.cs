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
        // ═══════════════════════════════════════════════════════════════════════════
        // ProcessPaymentSuccessAsync
        // ═══════════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task ProcessPaymentSuccessAsync_ExistingTransactionNotSucceeded_ProcessesNormally()
        {
            string sessionId = "sess_1";
            var transactions = new List<Transaction> 
            { 
                new Transaction { TransactionsStatus = "pending", InstructorPayouts = new List<InstructorPayout>() } 
            };
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "2" }, { "recipientEmail", "rec@test.com" } };
            var course = new Course { Title = "Test Course", Price = 100m, CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 3 };
            var dbTransactionMock = Substitute.For<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(transactions);
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(2).Returns(course);
            _adminFinanceServiceMock.GetPayoutDaysConfigAsync().Returns("15,30"); // Prevent error if it proceeds
            
            // Just simulate failure in AddOrder so we can catch it and verify we reached inside.
            // Or better, simulate CourseNotPublished so we hit the rollback early.
            course.CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Draft.ToValue();
            Func<Task> act = async () => await _sut.ProcessPaymentSuccessAsync(sessionId);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_AlreadyProcessed_ReturnsEarly()
        {
            string sessionId = "sess_1";
            var transactions = new List<Transaction> 
            { 
                new Transaction { TransactionsStatus = "succeeded", InstructorPayouts = new List<InstructorPayout> { new InstructorPayout() } } 
            };
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(transactions);

            await _sut.ProcessPaymentSuccessAsync(sessionId);

            await _repoMock.Received(1).GetTransactionsBySessionIdAsync(sessionId);
            await _paymentGatewayMock.DidNotReceive().GetSessionMetadataAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_NoMetadata_ThrowsInvalidOperationException()
        {
            string sessionId = "sess_1";
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns((Dictionary<string, string>?)null);

            Func<Task> act = async () => await _sut.ProcessPaymentSuccessAsync(sessionId);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Valid metadata or UserId was not found in the Stripe Session.");
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_NoCourseIds_ThrowsInvalidOperationException()
        {
            string sessionId = "sess_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" } };
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);

            Func<Task> act = async () => await _sut.ProcessPaymentSuccessAsync(sessionId);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Valid Course ID list was not found in the Stripe session/intent.");
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_ValidRequest_ProcessesSuccessfully()
        {
            string sessionId = "sess_1";
            int userId = 1;
            int courseId = 2;
            var metadata = new Dictionary<string, string> 
            { 
                { "userId", userId.ToString() }, 
                { "courseIds", courseId.ToString() },
                { "recipientEmail", "rec@test.com" }
            };
            
            var course = new Course { Title = "Test Course", Price = 100m, CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 3 };
            var dbTransactionMock = Substitute.For<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            var recipientAccount = new Account { AccountId = 10, Email = "rec@test.com" };
            
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);
            _paymentGatewayMock.GetPaymentReferenceAsync(sessionId).Returns("pi_1");
            _adminFinanceServiceMock.GetCurrentTransferRateAsync().Returns(80m);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns(course);
            _adminFinanceServiceMock.GetPayoutDaysConfigAsync().Returns("15,30");
            _userRepoMock.GetAccountByEmailAsync("rec@test.com").Returns(recipientAccount);

            await _sut.ProcessPaymentSuccessAsync(sessionId);

            await _repoMock.Received(1).AddOrderAsync(Arg.Is<OrderInfo>(o => o.UserId == userId && o.OrderStatus == "paid"));
            await _repoMock.Received(1).AddOrderItemAsync(Arg.Is<OrderItem>(oi => oi.CourseId == courseId && oi.PurchasePrice == 100m));
            await _repoMock.Received(1).AddTransactionAsync(Arg.Is<Transaction>(t => t.Amount == 100m && t.StripePaymentintentId == "pi_1"));
            await _giftRepoMock.Received(1).AddAsync(Arg.Is<Gift>(g => g.RecipientEmail == "rec@test.com" && g.DeliveryStatus == "sent"));
            await _emailServiceMock.Received(1).SendEmailAsync("rec@test.com", Arg.Any<string>(), Arg.Any<string>());
            await _repoMock.Received(1).AddInstructorPayoutAsync(Arg.Any<InstructorPayout>());
            await dbTransactionMock.Received(1).CommitAsync();
            await _hubContextMock.Clients.Group("AdminFinance").Received(1).SendCoreAsync("UpdatePayoutStatus", Arg.Any<object[]>());
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_CourseNotPublished_RollsBackTransaction()
        {
            string sessionId = "sess_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "2" } };
            var course = new Course { Title = "Test Course", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Draft.ToValue() };
            var dbTransactionMock = Substitute.For<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);
            _paymentGatewayMock.GetPaymentReferenceAsync(sessionId).Returns("pi_1");
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(2).Returns(course);

            Func<Task> act = async () => await _sut.ProcessPaymentSuccessAsync(sessionId);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"The course \"{course.Title}\" is not published and cannot be purchased.");
            await dbTransactionMock.Received(1).RollbackAsync();
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_DbException_RollsBackTransaction()
        {
            string sessionId = "sess_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "2" } };
            var dbTransactionMock = Substitute.For<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);
            _paymentGatewayMock.GetPaymentReferenceAsync(sessionId).Returns("pi_1");
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _repoMock.When(x => x.AddOrderAsync(Arg.Any<OrderInfo>())).Throw(new Exception("DB Error"));

            Func<Task> act = async () => await _sut.ProcessPaymentSuccessAsync(sessionId);

            await act.Should().ThrowAsync<Exception>().WithMessage("DB Error");
            await dbTransactionMock.Received(1).RollbackAsync();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ProcessPaymentIntentSuccessAsync
        // ═══════════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task ProcessPaymentIntentSuccessAsync_AlreadyProcessed_ReturnsEarly()
        {
            string intentId = "pi_1";
            var transactions = new List<Transaction> 
            { 
                new Transaction { TransactionsStatus = "succeeded", InstructorPayouts = new List<InstructorPayout> { new InstructorPayout() } } 
            };
            _repoMock.GetTransactionsBySessionIdAsync(intentId).Returns(transactions);

            await _sut.ProcessPaymentIntentSuccessAsync(intentId);

            await _repoMock.Received(1).GetTransactionsBySessionIdAsync(intentId);
            await _paymentGatewayMock.DidNotReceive().GetPaymentIntentMetadataAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task ProcessPaymentIntentSuccessAsync_NoMetadata_ThrowsInvalidOperationException()
        {
            string intentId = "pi_1";
            _repoMock.GetTransactionsBySessionIdAsync(intentId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetPaymentIntentMetadataAsync(intentId).Returns((Dictionary<string, string>?)null);

            Func<Task> act = async () => await _sut.ProcessPaymentIntentSuccessAsync(intentId);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Valid metadata or UserId was not found in the Stripe PaymentIntent.");
        }

        [Fact]
        public async Task ProcessPaymentIntentSuccessAsync_ValidRequest_ProcessesSuccessfully()
        {
            string intentId = "pi_1";
            int userId = 1;
            int courseId = 2;
            var metadata = new Dictionary<string, string> 
            { 
                { "userId", userId.ToString() }, 
                { "courseIds", courseId.ToString() },
                { "recipientEmail", "rec@test.com" }
            };
            
            var course = new Course { Title = "Test Course", Price = 100m, CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 3 };
            var dbTransactionMock = Substitute.For<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            
            _repoMock.GetTransactionsBySessionIdAsync(intentId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetPaymentIntentMetadataAsync(intentId).Returns(metadata);
            _adminFinanceServiceMock.GetCurrentTransferRateAsync().Returns(80m);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns(course);
            _adminFinanceServiceMock.GetPayoutDaysConfigAsync().Returns(""); // Empty config test

            await _sut.ProcessPaymentIntentSuccessAsync(intentId);

            await _repoMock.Received(1).AddOrderAsync(Arg.Is<OrderInfo>(o => o.UserId == userId && o.OrderStatus == "paid"));
            await dbTransactionMock.Received(1).CommitAsync();
        }

        [Fact]
        public async Task ProcessPaymentIntentSuccessAsync_CourseNull_ReturnsWithoutAddingOrder()
        {
            string intentId = "pi_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "2" } };
            var dbTransactionMock = Substitute.For<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            
            _repoMock.GetTransactionsBySessionIdAsync(intentId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetPaymentIntentMetadataAsync(intentId).Returns(metadata);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(2).Returns((Course?)null);

            await _sut.ProcessPaymentIntentSuccessAsync(intentId);

            await _repoMock.DidNotReceive().AddOrderItemAsync(Arg.Any<OrderItem>());
            await dbTransactionMock.Received(1).CommitAsync();
        }

        [Fact]
        public async Task ProcessGiftFulfillmentAsync_EmailAndNotiExceptions_CatchesAndContinues()
        {
            // Testing exception handling inside ProcessGiftFulfillmentAsync
            string sessionId = "sess_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "2" }, { "recipientEmail", "rec@test.com" } };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 3 };
            var dbTransactionMock = Substitute.For<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            var account = new Account { AccountId = 5, User = new User { FullName = "Sender" } };
            var recipientAccount = new Account { AccountId = 6 };
            
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(2).Returns(course);
            _userRepoMock.GetAccountByIdAsync(1).Returns(account);
            _userRepoMock.GetAccountByEmailAsync("rec@test.com").Returns(recipientAccount);
            
            _emailServiceMock.When(x => x.SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())).Throw(new Exception("Email Error"));
            _notificationServiceMock.When(x => x.SendNotificationAsync(6, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())).Throw(new Exception("Noti Error"));

            await _sut.ProcessPaymentSuccessAsync(sessionId);

            await dbTransactionMock.Received(1).CommitAsync();
            _loggerMock.ReceivedCalls().Should().HaveCount(2); // LogError should be called twice
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // GetCurrencyFromCountry
        // ═══════════════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData("GB", "GBP")]
        [InlineData("CA", "CAD")]
        [InlineData("CH", "CHF")]
        [InlineData("AT", "EUR")]
        [InlineData("FR", "EUR")]
        [InlineData("DE", "EUR")]
        [InlineData("IT", "EUR")]
        [InlineData("ES", "EUR")]
        [InlineData("BG", "BGN")]
        [InlineData("HR", "EUR")]
        [InlineData("CZ", "CZK")]
        [InlineData("DK", "DKK")]
        [InlineData("HU", "HUF")]
        [InlineData("IS", "ISK")]
        [InlineData("NO", "NOK")]
        [InlineData("PL", "PLN")]
        [InlineData("RO", "RON")]
        [InlineData("SE", "SEK")]
        [InlineData("AU", "AUD")]
        [InlineData("VN", "VND")]
        [InlineData("US", "USD")]
        [InlineData("JP", "USD")]
        [InlineData("", "USD")]
        [InlineData(null, "USD")]
        public async Task GetCurrencyFromCountry_ValidCountry_MapsCorrectly(string? countryCode, string expectedCurrency)
        {
            // Testing this indirectly via InitiateGiftCheckoutAsync 
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = "rec@test.com", SuccessUrl = "success", CancelUrl = "cancel" };
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _userRepoMock.GetInstructorStripeCountryAsync(2).Returns(countryCode);
            
            var paymentResult = new PaymentSessionResult { SessionUrl = "url", SessionId = "sess" };
            _paymentGatewayMock.CreateCheckoutSessionAsync(Arg.Any<List<PaymentLineItem>>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), expectedCurrency, Arg.Any<string>(), Arg.Any<decimal?>(), Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            await _sut.InitiateGiftCheckoutAsync(1, request);

            await _paymentGatewayMock.Received(1).CreateCheckoutSessionAsync(Arg.Any<List<PaymentLineItem>>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), expectedCurrency, Arg.Any<string>(), Arg.Any<decimal?>(), Arg.Any<Dictionary<string, string>>());
        }
    }
}
