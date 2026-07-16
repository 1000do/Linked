using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public partial class CheckoutServiceTests
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // InitiatePaymentIntentAsync
        // ═══════════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task InitiatePaymentIntentAsync_CartEmpty_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(new List<CartItem>());

            //Act
            Func<Task> act = async () => await _sut.InitiatePaymentIntentAsync(userId, null);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cart is empty. Cannot checkout.");
        }

        [Fact]
        public async Task InitiatePaymentIntentAsync_CourseNotPublished_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Draft.ToValue() };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course } };

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);

            //Act
            Func<Task> act = async () => await _sut.InitiatePaymentIntentAsync(userId, null);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"The course \"{course.Title}\" is not published and cannot be purchased.");
        }

        [Fact]
        public async Task InitiatePaymentIntentAsync_UserAlreadyEnrolled_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue() };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course } };

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _courseRepoMock.IsEnrolledAsync(userId, 1).Returns(true);

            //Act
            Func<Task> act = async () => await _sut.InitiatePaymentIntentAsync(userId, null);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"You have already purchased the course \"{course.Title}\". Please remove it from the cart.");
        }

        [Fact]
        public async Task InitiatePaymentIntentAsync_InvalidCouponCode_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue() };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course } };
            string couponCode = "INVALID";

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _courseRepoMock.IsEnrolledAsync(userId, 1).Returns(false);
            _couponRepoMock.GetByCodeAsync(couponCode).Returns((Coupon?)null);

            //Act
            Func<Task> act = async () => await _sut.InitiatePaymentIntentAsync(userId, couponCode);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This coupon does not exist.");
        }

        [Fact]
        public async Task InitiatePaymentIntentAsync_CouponExpired_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue() };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course } };
            string couponCode = "EXPIRED";
            var coupon = new Coupon { IsActive = false };

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _courseRepoMock.IsEnrolledAsync(userId, 1).Returns(false);
            _couponRepoMock.GetByCodeAsync(couponCode).Returns(coupon);

            //Act
            Func<Task> act = async () => await _sut.InitiatePaymentIntentAsync(userId, couponCode);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This coupon has expired or run out of usage.");
        }

        [Fact]
        public async Task InitiatePaymentIntentAsync_ValidCartWithCoupon_ReturnsCheckoutResponse()
        {
            //Arrange 1
            int userId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), Price = 100m, CouponId = 1 };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course, Price = 100m } };
            string couponCode = "DISCOUNT";
            var coupon = new Coupon { CouponId = 1, IsActive = true, CouponType = "percentage", DiscountValue = 20, MinOrderValue = 50m };
            var paymentResult = ("secret", "pi_1");
            var expectedResponse = new CheckoutResponse { SessionUrl = "secret", SessionId = "pi_1" };

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _courseRepoMock.IsEnrolledAsync(userId, 1).Returns(false);
            _couponRepoMock.GetByCodeAsync(couponCode).Returns(coupon);
            _paymentGatewayMock.CreatePaymentIntentAsync(80m, "usd", Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            //Act
            var result = await _sut.InitiatePaymentIntentAsync(userId, couponCode);

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _paymentGatewayMock.Received(1).CreatePaymentIntentAsync(80m, "usd", Arg.Any<Dictionary<string, string>>());
        }

        [Fact]
        public async Task InitiatePaymentIntentAsync_ValidCartWithFixedCoupon_ReturnsCheckoutResponse()
        {
            //Arrange 1
            int userId = 1;
            int couponId = 99;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m, CouponId = couponId };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course, Price = 100m } };
            string couponCode = "DISCOUNT";
            var coupon = new Coupon { CouponId = couponId, IsActive = true, CouponType = "fixed", DiscountValue = 20, MinOrderValue = 50m };
            var paymentResult = ("secret", "pi_1");
            var expectedResponse = new CheckoutResponse { SessionUrl = "secret", SessionId = "pi_1" };

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _courseRepoMock.IsEnrolledAsync(userId, 1).Returns(false);
            _couponRepoMock.GetByCodeAsync(couponCode).Returns(coupon);
            _paymentGatewayMock.CreatePaymentIntentAsync(80m, "usd", Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            //Act
            var result = await _sut.InitiatePaymentIntentAsync(userId, couponCode);

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _paymentGatewayMock.Received(1).CreatePaymentIntentAsync(80m, "usd", Arg.Any<Dictionary<string, string>>());
        }

        [Fact]
        public async Task InitiatePaymentIntentAsync_ValidCartWithoutCoupon_ReturnsCheckoutResponse()
        {
            //Arrange 1
            int userId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            var cartItems = new List<CartItem> { new CartItem { CourseId = 1, Course = course, Price = 100m } };
            var paymentResult = ("secret", "pi_1");
            var expectedResponse = new CheckoutResponse { SessionUrl = "secret", SessionId = "pi_1" };

            //Arrange 2
            _repoMock.GetCartItemsWithCourseAndInstructorAsync(userId).Returns(cartItems);
            _courseRepoMock.IsEnrolledAsync(userId, 1).Returns(false);
            _paymentGatewayMock.CreatePaymentIntentAsync(100m, "usd", Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            //Act
            var result = await _sut.InitiatePaymentIntentAsync(userId, null);

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _paymentGatewayMock.Received(1).CreatePaymentIntentAsync(100m, "usd", Arg.Any<Dictionary<string, string>>());
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ProcessPaymentIntentSuccessAsync
        // ═══════════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task ProcessPaymentIntentSuccessAsync_AlreadyProcessed_ReturnsEarly()
        {
            //Arrange 1
            string piId = "pi_1";
            var transactions = new List<Transaction> 
            { 
                new Transaction { TransactionsStatus = "succeeded", InstructorPayouts = new List<InstructorPayout> { new InstructorPayout() } } 
            };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(piId).Returns(transactions);

            //Act
            await _sut.ProcessPaymentIntentSuccessAsync(piId);

            //Assert
            await _repoMock.Received(1).GetTransactionsBySessionIdAsync(piId);
            await _paymentGatewayMock.DidNotReceive().GetPaymentIntentMetadataAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task ProcessPaymentIntentSuccessAsync_NoMetadataOrUserId_ThrowsInvalidOperationException()
        {
            //Arrange 1
            string piId = "pi_1";

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(piId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetPaymentIntentMetadataAsync(piId).Returns(new Dictionary<string, string>());

            //Act
            Func<Task> act = async () => await _sut.ProcessPaymentIntentSuccessAsync(piId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Valid metadata or UserId was not found in the Stripe PaymentIntent.");
        }

        [Fact]
        public async Task ProcessPaymentIntentSuccessAsync_NoCourseIds_ThrowsInvalidOperationException()
        {
            //Arrange 1
            string piId = "pi_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "" } };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(piId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetPaymentIntentMetadataAsync(piId).Returns(metadata);

            //Act
            Func<Task> act = async () => await _sut.ProcessPaymentIntentSuccessAsync(piId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Valid Course ID list was not found in the Stripe PaymentIntent.");
        }

        [Fact]
        public async Task ProcessPaymentIntentSuccessAsync_DirectCheckout_ProcessesSuccessfully()
        {
            //Arrange 1
            string piId = "pi_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "1" }, { "checkoutType", "direct" } };
            var dbTransactionMock = Substitute.For<IDbContextTransaction>();
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), Price = 100m, InstructorId = 2 };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(piId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetPaymentIntentMetadataAsync(piId).Returns(metadata);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(1).Returns(course);
            _userRepoMock.GetAccountByIdAsync(1).Returns(new Account { AccountId = 1 });

            //Act
            await _sut.ProcessPaymentIntentSuccessAsync(piId);

            //Assert
            await _repoMock.Received(1).AddTransactionAsync(Arg.Is<Transaction>(t => t.StripeSessionId == piId && t.StripePaymentintentId == piId));
            await dbTransactionMock.Received(1).CommitAsync();
        }

        [Fact]
        public async Task ProcessPaymentIntentSuccessAsync_CourseNotPublished_ThrowsInvalidOperationException()
        {
            //Arrange 1
            string piId = "pi_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "1" }, { "checkoutType", "direct" } };
            var dbTransactionMock = Substitute.For<IDbContextTransaction>();
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Draft.ToValue(), Price = 100m, InstructorId = 2 };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(piId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetPaymentIntentMetadataAsync(piId).Returns(metadata);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(1).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.ProcessPaymentIntentSuccessAsync(piId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"The course \"{course.Title}\" is not published and cannot be purchased.");
            await dbTransactionMock.Received(1).RollbackAsync();
        }

        [Fact]
        public async Task ProcessPaymentIntentSuccessAsync_ExceptionThrown_RollsBackTransactionAndRethrows()
        {
            //Arrange 1
            string piId = "pi_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "1" } };
            var dbTransactionMock = Substitute.For<IDbContextTransaction>();

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(piId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetPaymentIntentMetadataAsync(piId).Returns(metadata);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _repoMock.When(x => x.AddOrderAsync(Arg.Any<OrderInfo>())).Throw(new Exception("DB Error"));

            //Act
            Func<Task> act = async () => await _sut.ProcessPaymentIntentSuccessAsync(piId);

            //Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("DB Error");
            await dbTransactionMock.Received(1).RollbackAsync();
        }



        [Fact]
        public async Task ProcessPaymentIntentSuccessAsync_CartCheckout_ProcessesSuccessfully()
        {
            //Arrange 1
            string piId = "pi_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "1" }, { "checkoutType", "cart" } };
            var dbTransactionMock = Substitute.For<IDbContextTransaction>();
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), Price = 100m, InstructorId = 2 };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(piId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetPaymentIntentMetadataAsync(piId).Returns(metadata);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(1).Returns(course);

            //Act
            await _sut.ProcessPaymentIntentSuccessAsync(piId);

            //Assert
            await _repoMock.Received(1).ClearCartAsync(1);
            await dbTransactionMock.Received(1).CommitAsync();
        }

        [Fact]
        public async Task ProcessPaymentIntentSuccessAsync_WithCoupon_IncrementsUsage_ProcessesSuccessfully()
        {
            //Arrange 1
            string piId = "pi_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "1" }, { "couponCode", "CODE" } };
            var dbTransactionMock = Substitute.For<IDbContextTransaction>();
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), Price = 100m, InstructorId = 2, CouponId = 1 };
            var coupon = new Coupon { CouponId = 1, MinOrderValue = 50m, CouponType = "fixed", DiscountValue = 20 };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(piId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetPaymentIntentMetadataAsync(piId).Returns(metadata);
            _couponRepoMock.GetValidCouponAsync("CODE", Arg.Any<DateTime>()).Returns(coupon);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(1).Returns(course);

            //Act
            await _sut.ProcessPaymentIntentSuccessAsync(piId);

            //Assert
            await _repoMock.Received(1).AddOrderItemAsync(Arg.Is<OrderItem>(oi => oi.CouponUsed == true && oi.PurchasePrice == 80m));
            await _repoMock.Received(1).IncrementCouponUsageAsync(1);
            await dbTransactionMock.Received(1).CommitAsync();
        }

        [Fact]
        public async Task ProcessPaymentIntentSuccessAsync_ExistingEnrollment_UpdatesStatus_ProcessesSuccessfully()
        {
            //Arrange 1
            string piId = "pi_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "1" } };
            var dbTransactionMock = Substitute.For<IDbContextTransaction>();
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), Price = 100m, InstructorId = 2 };
            var existingEnrollment = new Enrollment { EnrollmentId = 1, EnrollmentStatus = "refunded" };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(piId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetPaymentIntentMetadataAsync(piId).Returns(metadata);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(1).Returns(course);
            _enrollmentRepoMock.GetEnrollmentIncludingRefundedAsync(1, 1).Returns(existingEnrollment);

            //Act
            await _sut.ProcessPaymentIntentSuccessAsync(piId);

            //Assert
            existingEnrollment.EnrollmentStatus.Should().Be("active");
            await _enrollmentRepoMock.Received(1).ClearMaterialCompletionsAsync(1);
            await _enrollmentRepoMock.DidNotReceive().AddEnrollmentAsync(Arg.Any<Enrollment>());
            await dbTransactionMock.Received(1).CommitAsync();
        }
    }
}
