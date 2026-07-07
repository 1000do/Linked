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
        // InitiateDirectCheckoutAsync
        // ═══════════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task InitiateDirectCheckoutAsync_CourseNotFound_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 1;

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns((Course?)null);

            //Act
            Func<Task> act = async () => await _sut.InitiateDirectCheckoutAsync(userId, courseId, null, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Course not found.");
            await _courseRepoMock.Received(1).GetCourseWithInstructorAsync(courseId);
        }

        [Fact]
        public async Task InitiateDirectCheckoutAsync_CourseNotPublished_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Draft.ToValue() };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.InitiateDirectCheckoutAsync(userId, courseId, null, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"The course \"{course.Title}\" is not published and cannot be purchased.");
        }

        [Fact]
        public async Task InitiateDirectCheckoutAsync_InstructorIsUser_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = userId };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.InitiateDirectCheckoutAsync(userId, courseId, null, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"You cannot purchase your own course \"{course.Title}\".");
        }

        [Fact]
        public async Task InitiateDirectCheckoutAsync_UserAlreadyEnrolled_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2 };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(true);

            //Act
            Func<Task> act = async () => await _sut.InitiateDirectCheckoutAsync(userId, courseId, null, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"You have already purchased the course \"{course.Title}\".");
            await _courseRepoMock.Received(1).IsEnrolledAsync(userId, courseId);
        }

        [Fact]
        public async Task InitiateDirectCheckoutAsync_InstructorStripeNotConnected_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2 };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns(string.Empty);

            //Act
            Func<Task> act = async () => await _sut.InitiateDirectCheckoutAsync(userId, courseId, null, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Instructor has not connected a Stripe payment account.");
            await _userRepoMock.Received(1).GetInstructorStripeAccountIdAsync(2);
        }

        [Fact]
        public async Task InitiateDirectCheckoutAsync_InvalidCouponCode_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2 };
            string couponCode = "INVALID";

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _couponRepoMock.GetByCodeAsync(couponCode).Returns((Coupon?)null);

            //Act
            Func<Task> act = async () => await _sut.InitiateDirectCheckoutAsync(userId, courseId, couponCode, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This coupon does not exist.");
        }

        [Fact]
        public async Task InitiateDirectCheckoutAsync_CouponExpired_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2 };
            string couponCode = "EXPIRED";
            var coupon = new Coupon { IsActive = false };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _couponRepoMock.GetByCodeAsync(couponCode).Returns(coupon);

            //Act
            Func<Task> act = async () => await _sut.InitiateDirectCheckoutAsync(userId, courseId, couponCode, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This coupon has expired or run out of usage.");
        }

        [Fact]
        public async Task InitiateDirectCheckoutAsync_CouponNotApplicableToCourse_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, CouponId = 1 };
            string couponCode = "NOT_APPLICABLE";
            var coupon = new Coupon { CouponId = 2, IsActive = true, StartDate = DateTime.Now.AddDays(-1), EndDate = DateTime.Now.AddDays(1) };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _couponRepoMock.GetByCodeAsync(couponCode).Returns(coupon);

            //Act
            Func<Task> act = async () => await _sut.InitiateDirectCheckoutAsync(userId, courseId, couponCode, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This coupon is not applicable to this course.");
        }

        [Fact]
        public async Task InitiateDirectCheckoutAsync_CoursePriceBelowCouponMinOrder_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 10m, CouponId = 1 };
            string couponCode = "HIGH_MIN_ORDER";
            var coupon = new Coupon { CouponId = 1, IsActive = true, MinOrderValue = 50m };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _couponRepoMock.GetByCodeAsync(couponCode).Returns(coupon);

            //Act
            Func<Task> act = async () => await _sut.InitiateDirectCheckoutAsync(userId, courseId, couponCode, "success", "cancel");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage($"This coupon requires a minimum course value of ${coupon.MinOrderValue:N2}.");
        }

        [Fact]
        public async Task InitiateDirectCheckoutAsync_ValidRequestWithCoupon_ReturnsCheckoutResponse()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m, CouponId = 1 };
            string couponCode = "DISCOUNT";
            var coupon = new Coupon { CouponId = 1, IsActive = true, CouponType = "percentage", DiscountValue = 20, MinOrderValue = 50m };
            string userEmail = "test@test.com";
            var paymentResult = new PaymentSessionResult { SessionUrl = "url", SessionId = "sess_1" };
            var expectedResponse = new CheckoutResponse { SessionUrl = "url", SessionId = "sess_1" };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _couponRepoMock.GetByCodeAsync(couponCode).Returns(coupon);
            _userRepoMock.GetUserEmailAsync(userId).Returns(userEmail);
            _userRepoMock.GetInstructorStripeCountryAsync(2).Returns("US");
            _paymentGatewayMock.CreateCheckoutSessionAsync(Arg.Any<List<PaymentLineItem>>(), "success", "cancel", userEmail, Arg.Any<string>(), "USD", null, null, Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            //Act
            var result = await _sut.InitiateDirectCheckoutAsync(userId, courseId, couponCode, "success", "cancel");

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _paymentGatewayMock.Received(1).CreateCheckoutSessionAsync(
                Arg.Is<List<PaymentLineItem>>(l => l.First().UnitPrice == 80m),
                "success", "cancel", userEmail, Arg.Any<string>(), "USD", null, null, Arg.Any<Dictionary<string, string>>());
        }

        [Fact]
        public async Task InitiateDirectCheckoutAsync_ValidRequestWithoutCoupon_ReturnsCheckoutResponse()
        {
            //Arrange 1
            int userId = 1;
            int courseId = 1;
            var course = new Course { Title = "Test", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 2, Price = 100m };
            string userEmail = "test@test.com";
            var paymentResult = new PaymentSessionResult { SessionUrl = "url", SessionId = "sess_1" };
            var expectedResponse = new CheckoutResponse { SessionUrl = "url", SessionId = "sess_1" };

            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns(course);
            _courseRepoMock.IsEnrolledAsync(userId, courseId).Returns(false);
            _userRepoMock.GetInstructorStripeAccountIdAsync(2).Returns("acct_123");
            _userRepoMock.GetUserEmailAsync(userId).Returns(userEmail);
            _userRepoMock.GetInstructorStripeCountryAsync(2).Returns("VN");
            _paymentGatewayMock.CreateCheckoutSessionAsync(Arg.Any<List<PaymentLineItem>>(), "success", "cancel", userEmail, Arg.Any<string>(), "VND", null, null, Arg.Any<Dictionary<string, string>>())
                .Returns(paymentResult);

            //Act
            var result = await _sut.InitiateDirectCheckoutAsync(userId, courseId, null, "success", "cancel");

            //Assert
            result.Should().BeEquivalentTo(expectedResponse);
            await _paymentGatewayMock.Received(1).CreateCheckoutSessionAsync(
                Arg.Is<List<PaymentLineItem>>(l => l.First().UnitPrice == 100m),
                "success", "cancel", userEmail, Arg.Any<string>(), "VND", null, null, Arg.Any<Dictionary<string, string>>());
        }
    }
}
