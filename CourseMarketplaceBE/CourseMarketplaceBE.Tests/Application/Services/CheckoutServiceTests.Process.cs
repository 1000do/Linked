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
        // ProcessPaymentSuccessAsync
        // ═══════════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task ProcessPaymentSuccessAsync_AlreadyProcessed_ReturnsEarly()
        {
            //Arrange 1
            string sessionId = "sess_1";
            var transactions = new List<Transaction> 
            { 
                new Transaction { TransactionsStatus = "succeeded", InstructorPayouts = new List<InstructorPayout> { new InstructorPayout() } } 
            };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(transactions);

            //Act
            await _sut.ProcessPaymentSuccessAsync(sessionId);

            //Assert
            await _repoMock.Received(1).GetTransactionsBySessionIdAsync(sessionId);
            await _paymentGatewayMock.DidNotReceive().GetSessionMetadataAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_TransactionExistsButPending_ContinuesProcessingAndThrowsSinceNoMetadata()
        {
            //Arrange 1
            string sessionId = "sess_1";
            var transactions = new List<Transaction> 
            { 
                new Transaction { TransactionsStatus = "pending", InstructorPayouts = new List<InstructorPayout>() } 
            };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(transactions);
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns((Dictionary<string, string>?)null);

            //Act
            Func<Task> act = async () => await _sut.ProcessPaymentSuccessAsync(sessionId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Valid metadata or UserId was not found in the Stripe Session.");
            await _repoMock.Received(1).GetTransactionsBySessionIdAsync(sessionId);
            await _paymentGatewayMock.Received(1).GetSessionMetadataAsync(sessionId);
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_NoMetadataOrUserId_ThrowsInvalidOperationException()
        {
            //Arrange 1
            string sessionId = "sess_1";

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(new Dictionary<string, string>());

            //Act
            Func<Task> act = async () => await _sut.ProcessPaymentSuccessAsync(sessionId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Valid metadata or UserId was not found in the Stripe Session.");
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_NoCourseIds_ThrowsInvalidOperationException()
        {
            //Arrange 1
            string sessionId = "sess_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "" } };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);

            //Act
            Func<Task> act = async () => await _sut.ProcessPaymentSuccessAsync(sessionId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Valid Course ID list was not found in the Stripe Session.");
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_DirectCheckout_CourseValidWithoutCoupon_ProcessesSuccessfully()
        {
            //Arrange 1
            string sessionId = "sess_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "1" }, { "checkoutType", "direct" } };
            var dbTransactionMock = Substitute.For<IDbContextTransaction>();
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), Price = 100m, InstructorId = 2 };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);
            _adminFinanceServiceMock.GetCurrentTransferRateAsync().Returns(80m);
            _paymentGatewayMock.GetPaymentReferenceAsync(sessionId).Returns("pi_1");
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(1).Returns(course);
            _enrollmentRepoMock.GetEnrollmentIncludingRefundedAsync(1, 1).Returns((Enrollment?)null);

            //Act
            await _sut.ProcessPaymentSuccessAsync(sessionId);

            //Assert
            await _repoMock.Received(1).AddOrderAsync(Arg.Is<OrderInfo>(o => o.OrderStatus == "paid" && o.PaymentMethod == "stripe_direct"));
            await _repoMock.Received(1).AddOrderItemAsync(Arg.Is<OrderItem>(oi => oi.PurchasePrice == 100m && oi.CouponUsed == false));
            await _repoMock.Received(1).AddTransactionAsync(Arg.Is<Transaction>(t => t.TransactionsStatus == "succeeded" && t.Amount == 100m));
            await _enrollmentRepoMock.Received(1).AddEnrollmentAsync(Arg.Any<Enrollment>());
            await _repoMock.Received(1).AddInstructorPayoutAsync(Arg.Any<InstructorPayout>());
            await _repoMock.DidNotReceive().ClearCartAsync(Arg.Any<int>());
            await _notificationServiceMock.Received(1).SendNotificationAsync(2, "You have a new order", Arg.Any<string>(), Arg.Any<string>());
            await dbTransactionMock.Received(1).CommitAsync();
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_DirectCheckout_CourseNotPublished_ThrowsInvalidOperationException()
        {
            //Arrange 1
            string sessionId = "sess_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "1" }, { "checkoutType", "direct" } };
            var dbTransactionMock = Substitute.For<IDbContextTransaction>();
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Draft.ToValue(), Price = 100m, InstructorId = 2 };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(1).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.ProcessPaymentSuccessAsync(sessionId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"The course \"{course.Title}\" is not published and cannot be purchased.");
            await dbTransactionMock.Received(1).RollbackAsync();
        }



        [Fact]
        public async Task ProcessPaymentSuccessAsync_CartCheckout_ClearsCart_ProcessesSuccessfully()
        {
            //Arrange 1
            string sessionId = "sess_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "1" }, { "checkoutType", "cart" } };
            var dbTransactionMock = Substitute.For<IDbContextTransaction>();
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), Price = 100m, InstructorId = 2 };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(1).Returns(course);

            //Act
            await _sut.ProcessPaymentSuccessAsync(sessionId);

            //Assert
            await _repoMock.Received(1).ClearCartAsync(1);
            await dbTransactionMock.Received(1).CommitAsync();
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_WithCoupon_IncrementsUsage_ProcessesSuccessfully()
        {
            //Arrange 1
            string sessionId = "sess_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "1" }, { "couponCode", "CODE" } };
            var dbTransactionMock = Substitute.For<IDbContextTransaction>();
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), Price = 100m, InstructorId = 2, CouponId = 1 };
            var coupon = new Coupon { CouponId = 1, MinOrderValue = 50m, CouponType = "fixed", DiscountValue = 20 };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);
            _couponRepoMock.GetValidCouponAsync("CODE", Arg.Any<DateTime>()).Returns(coupon);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(1).Returns(course);

            //Act
            await _sut.ProcessPaymentSuccessAsync(sessionId);

            //Assert
            await _repoMock.Received(1).AddOrderItemAsync(Arg.Is<OrderItem>(oi => oi.CouponUsed == true && oi.PurchasePrice == 80m));
            await _repoMock.Received(1).IncrementCouponUsageAsync(1);
            await dbTransactionMock.Received(1).CommitAsync();
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_ExistingEnrollment_UpdatesStatus_ProcessesSuccessfully()
        {
            //Arrange 1
            string sessionId = "sess_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "1" } };
            var dbTransactionMock = Substitute.For<IDbContextTransaction>();
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), Price = 100m, InstructorId = 2 };
            var existingEnrollment = new Enrollment { EnrollmentId = 1, EnrollmentStatus = "refunded" };

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(1).Returns(course);
            _enrollmentRepoMock.GetEnrollmentIncludingRefundedAsync(1, 1).Returns(existingEnrollment);

            //Act
            await _sut.ProcessPaymentSuccessAsync(sessionId);

            //Assert
            existingEnrollment.EnrollmentStatus.Should().Be("active");
            await _enrollmentRepoMock.Received(1).ClearMaterialCompletionsAsync(1);
            await _enrollmentRepoMock.DidNotReceive().AddEnrollmentAsync(Arg.Any<Enrollment>());
            await dbTransactionMock.Received(1).CommitAsync();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ProcessPaymentCancelAsync
        // ═══════════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task ProcessPaymentSuccessAsync_DbException_RollsbackAndRethrows()
        {
            //Arrange 1
            string sessionId = "sess_1";
            var metadata = new Dictionary<string, string> { { "userId", "1" }, { "courseIds", "1" }, { "checkoutType", "direct" } };
            var dbTransactionMock = Substitute.For<IDbContextTransaction>();

            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _repoMock.When(x => x.AddOrderAsync(Arg.Any<OrderInfo>())).Throw(new Exception("DB Error"));

            //Act
            Func<Task> act = async () => await _sut.ProcessPaymentSuccessAsync(sessionId);

            //Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("DB Error");
            await dbTransactionMock.Received(1).RollbackAsync();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ProcessPaymentCancelAsync
        // ═══════════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task ProcessPaymentCancelAsync_Always_DoesNothing()
        {
            //Arrange 1
            int orderId = 1;

            //Act
            await _sut.ProcessPaymentCancelAsync(orderId);

            //Assert
            // No exceptions thrown, does nothing.
            await _repoMock.DidNotReceive().GetTransactionsByOrderIdAsync(orderId);
        }
    }
}
