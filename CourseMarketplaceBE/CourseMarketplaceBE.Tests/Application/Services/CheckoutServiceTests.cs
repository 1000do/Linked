using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using NSubstitute.ExceptionExtensions;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public partial class CheckoutServiceTests
    {
        private readonly ICheckoutRepository _repoMock;
        private readonly IEnrollmentRepository _enrollmentRepoMock;
        private readonly IPaymentGatewayService _paymentGatewayMock;
        private readonly ILogger<CheckoutService> _loggerMock;
        private readonly IHubContext<FinanceHub> _hubContextMock;
        private readonly INotificationService _notificationServiceMock;
        private readonly ICourseRepository _courseRepoMock;
        private readonly ICouponRepository _couponRepoMock;
        private readonly IUserRepository _userRepoMock;
        private readonly IAdminFinanceService _adminFinanceServiceMock;
        private readonly IConfiguration _configurationMock;
        private readonly CheckoutService _sut;

        public CheckoutServiceTests()
        {
            _repoMock = Substitute.For<ICheckoutRepository>();
            _enrollmentRepoMock = Substitute.For<IEnrollmentRepository>();
            _paymentGatewayMock = Substitute.For<IPaymentGatewayService>();
            _loggerMock = Substitute.For<ILogger<CheckoutService>>();
            _hubContextMock = Substitute.For<IHubContext<FinanceHub>>();
            _notificationServiceMock = Substitute.For<INotificationService>();
            _courseRepoMock = Substitute.For<ICourseRepository>();
            _couponRepoMock = Substitute.For<ICouponRepository>();
            _userRepoMock = Substitute.For<IUserRepository>();
            _adminFinanceServiceMock = Substitute.For<IAdminFinanceService>();
            _configurationMock = Substitute.For<IConfiguration>();

            _sut = new CheckoutService(
                _repoMock,
                _enrollmentRepoMock,
                _paymentGatewayMock,
                _loggerMock,
                _hubContextMock,
                _adminFinanceServiceMock,
                _notificationServiceMock,
                _courseRepoMock,
                _couponRepoMock,
                _userRepoMock,
                _configurationMock
            );
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // InitiateCheckoutAsync
        // ═══════════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task InitiateCheckoutAsync_CartEmpty_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(new List<CartItem>());

            //Act
            Func<Task> act = async () => await _sut.InitiateCheckoutAsync(userId, null, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cart is empty. Cannot checkout.");
            await _repoMock.Received(1).GetCartItemsWithCourseAndInstructorAsync(userId);
        }

        [Fact]
        public async Task InitiateCheckoutAsync_CourseNotPublished_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseStatus.Draft.ToValue() };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course } };

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _adminFinanceServiceMock.GetCurrentTransferRateAsync().Returns(80m);

            //Act
            Func<Task> act = async () => await _sut.InitiateCheckoutAsync(userId, null, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"The course \"{course.Title}\" is not published and cannot be purchased.");
            await _repoMock.Received(1).GetCartItemsWithCourseAndInstructorAsync(userId);
        }

        [Fact]
        public async Task InitiateCheckoutAsync_InstructorIsUser_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseStatus.Published.ToValue(), InstructorId = userId };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course } };

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _adminFinanceServiceMock.GetCurrentTransferRateAsync().Returns(80m);

            //Act
            Func<Task> act = async () => await _sut.InitiateCheckoutAsync(userId, null, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"You cannot purchase your own course \"{course.Title}\".");
        }

        [Fact]
        public async Task InitiateCheckoutAsync_UserAlreadyEnrolled_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course } };

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _adminFinanceServiceMock.GetCurrentTransferRateAsync().Returns(80m);
            _courseRepoMock.IsEnrolledAsync(userId, 1).Returns(true);

            //Act
            Func<Task> act = async () => await _sut.InitiateCheckoutAsync(userId, null, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"You have already purchased the course \"{course.Title}\". Please remove it from the cart.");
            await _courseRepoMock.Received(1).IsEnrolledAsync(userId, 1);
        }

        [Fact]
        public async Task InitiateCheckoutAsync_InvalidCouponCode_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course } };
            string couponCode = "INVALID";

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _adminFinanceServiceMock.GetCurrentTransferRateAsync().Returns(80m);
            _courseRepoMock.IsEnrolledAsync(userId, 1).Returns(false);
            _couponRepoMock.GetByCodeAsync(couponCode).Returns((Coupon?)null);

            //Act
            Func<Task> act = async () => await _sut.InitiateCheckoutAsync(userId, couponCode, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This coupon does not exist.");
            await _couponRepoMock.Received(1).GetByCodeAsync(couponCode);
        }

        [Fact]
        public async Task InitiateCheckoutAsync_CouponExpiredOrLimitReached_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course } };
            string couponCode = "EXPIRED";
            var coupon = new Coupon { IsActive = false };

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _adminFinanceServiceMock.GetCurrentTransferRateAsync().Returns(80m);
            _courseRepoMock.IsEnrolledAsync(userId, 1).Returns(false);
            _couponRepoMock.GetByCodeAsync(couponCode).Returns(coupon);

            //Act
            Func<Task> act = async () => await _sut.InitiateCheckoutAsync(userId, couponCode, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This coupon has expired or run out of usage.");
        }

        [Fact]
        public async Task InitiateCheckoutAsync_ValidCartWithPercentageCoupon_ReturnsCheckoutResponse()
        {
            //Arrange 1
            int userId = 1;
            int couponId = 99;
            var course = new Course { Title = "Test", CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m, CouponId = couponId };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course, Price = 100m } };
            string couponCode = "DISCOUNT";
            var coupon = new Coupon { CouponId = couponId, IsActive = true, CouponType = "percentage", DiscountValue = 20, MinOrderValue = 50m };
            string userEmail = "test@test.com";
            var paymentResult = new PaymentSessionResult { SessionUrl = "url", SessionId = "sess_1" };
            var expectedResponse = new CheckoutResponse { SessionUrl = "url", SessionId = "sess_1" };

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _adminFinanceServiceMock.GetCurrentTransferRateAsync().Returns(80m);
            _courseRepoMock.IsEnrolledAsync(userId, 1).Returns(false);
            _couponRepoMock.GetByCodeAsync(couponCode).Returns(coupon);
            _userRepoMock.GetUserEmailAsync(userId).Returns(userEmail);
            _paymentGatewayMock.CreateCheckoutSessionAsync(Arg.Any<List<PaymentLineItem>>(), "success", "cancel", userEmail, Arg.Any<string>(), "usd", null, null, Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            //Act
            var result = await _sut.InitiateCheckoutAsync(userId, couponCode, "success", "cancel");

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _paymentGatewayMock.Received(1).CreateCheckoutSessionAsync(
                Arg.Is<List<PaymentLineItem>>(l => l.First().UnitPrice == 80m),
                "success", "cancel", userEmail, Arg.Any<string>(), "usd", null, null, Arg.Any<Dictionary<string, string>>());
        }

        [Fact]
        public async Task InitiateCheckoutAsync_ValidCartWithFixedCoupon_ReturnsCheckoutResponse()
        {
            //Arrange 1
            int userId = 1;
            int couponId = 99;
            var course = new Course { Title = "Test", CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m, CouponId = couponId };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course, Price = 100m } };
            string couponCode = "DISCOUNT";
            var coupon = new Coupon { CouponId = couponId, IsActive = true, CouponType = "fixed", DiscountValue = 20, MinOrderValue = 50m };
            string userEmail = "test@test.com";
            var paymentResult = new PaymentSessionResult { SessionUrl = "url", SessionId = "sess_1" };
            var expectedResponse = new CheckoutResponse { SessionUrl = "url", SessionId = "sess_1" };

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _adminFinanceServiceMock.GetCurrentTransferRateAsync().Returns(80m);
            _courseRepoMock.IsEnrolledAsync(userId, 1).Returns(false);
            _couponRepoMock.GetByCodeAsync(couponCode).Returns(coupon);
            _userRepoMock.GetUserEmailAsync(userId).Returns(userEmail);
            _paymentGatewayMock.CreateCheckoutSessionAsync(Arg.Any<List<PaymentLineItem>>(), "success", "cancel", userEmail, Arg.Any<string>(), "usd", null, null, Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            //Act
            var result = await _sut.InitiateCheckoutAsync(userId, couponCode, "success", "cancel");

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _paymentGatewayMock.Received(1).CreateCheckoutSessionAsync(
                Arg.Is<List<PaymentLineItem>>(l => l.First().UnitPrice == 80m),
                "success", "cancel", userEmail, Arg.Any<string>(), "usd", null, null, Arg.Any<Dictionary<string, string>>());
        }

        [Fact]
        public async Task InitiateCheckoutAsync_ValidCartWithoutCoupon_ReturnsCheckoutResponse()
        {
            //Arrange 1
            int userId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course, Price = 100m } };
            string userEmail = "test@test.com";
            var paymentResult = new PaymentSessionResult { SessionUrl = "url", SessionId = "sess_1" };
            var expectedResponse = new CheckoutResponse { SessionUrl = "url", SessionId = "sess_1" };

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _adminFinanceServiceMock.GetCurrentTransferRateAsync().Returns(80m);
            _courseRepoMock.IsEnrolledAsync(userId, 1).Returns(false);
            _userRepoMock.GetUserEmailAsync(userId).Returns(userEmail);
            _paymentGatewayMock.CreateCheckoutSessionAsync(Arg.Any<List<PaymentLineItem>>(), "success", "cancel", userEmail, Arg.Any<string>(), "usd", null, null, Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            //Act
            var result = await _sut.InitiateCheckoutAsync(userId, null, "success", "cancel");

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _paymentGatewayMock.Received(1).CreateCheckoutSessionAsync(
                Arg.Is<List<PaymentLineItem>>(l => l.First().UnitPrice == 100m),
                "success", "cancel", userEmail, Arg.Any<string>(), "usd", null, null, Arg.Any<Dictionary<string, string>>());
        }
    }
}
