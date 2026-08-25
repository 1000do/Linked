using System;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Constants;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using CourseMarketplaceBE.Hubs;
using CourseMarketplaceBE.Application.IServices;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services;

public class GiftCheckoutServiceTests
{
    private readonly ICheckoutRepository _repoMock;
    private readonly IEnrollmentRepository _enrollmentRepoMock;
    private readonly IGiftCheckoutSessionRepository _sessionRepoMock;
    private readonly IPaymentGatewayService _paymentGatewayMock;
    private readonly ILogger<GiftCheckoutService> _loggerMock;
    private readonly IHubContext<FinanceHub> _hubContextMock;
    private readonly IAdminFinanceService _adminFinanceServiceMock;
    private readonly INotificationService _notificationServiceMock;
    private readonly ICourseRepository _courseRepoMock;
    private readonly IUserRepository _userRepoMock;
    private readonly IGiftRepository _giftRepoMock;
    private readonly IEmailService _emailServiceMock;
    private readonly IConfiguration _configurationMock;

    private readonly GiftCheckoutService _sut;

    public GiftCheckoutServiceTests()
    {
        _repoMock = Substitute.For<ICheckoutRepository>();
        _enrollmentRepoMock = Substitute.For<IEnrollmentRepository>();
        _sessionRepoMock = Substitute.For<IGiftCheckoutSessionRepository>();
        _paymentGatewayMock = Substitute.For<IPaymentGatewayService>();
        _loggerMock = Substitute.For<ILogger<GiftCheckoutService>>();
        _hubContextMock = Substitute.For<IHubContext<FinanceHub>>();
        _adminFinanceServiceMock = Substitute.For<IAdminFinanceService>();
        _notificationServiceMock = Substitute.For<INotificationService>();
        _courseRepoMock = Substitute.For<ICourseRepository>();
        _userRepoMock = Substitute.For<IUserRepository>();
        _giftRepoMock = Substitute.For<IGiftRepository>();
        _emailServiceMock = Substitute.For<IEmailService>();
        _configurationMock = Substitute.For<IConfiguration>();

        _sut = new GiftCheckoutService(
            _repoMock,
            _enrollmentRepoMock,
            _sessionRepoMock,
            _paymentGatewayMock,
            _loggerMock,
            _hubContextMock,
            _adminFinanceServiceMock,
            _notificationServiceMock,
            _courseRepoMock,
            _userRepoMock,
            _giftRepoMock,
            _emailServiceMock,
            _configurationMock);
    }

    [Fact]
    public async Task CreateGiftCheckoutSessionAsync_RecipientIsCourseInstructor_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var courseId = 10;
        var instructorId = 99;
        var recipientEmail = "instructor@example.com";
        var request = new GiftCheckoutSessionRequest
        {
            CourseId = courseId,
            RecipientEmail = recipientEmail
        };

        var course = new Course
        {
            CourseId = courseId,
            Title = "CapCut Video Editing",
            CourseStatus = CourseStatus.Published.ToValue(),
            InstructorId = instructorId
        };
        var instructorAccount = new Account
        {
            AccountId = instructorId,
            Email = recipientEmail
        };

        _courseRepoMock.GetCourseWithInstructorAsync(courseId).Returns(course);
        _userRepoMock.GetAccountByEmailAsync(recipientEmail).Returns(instructorAccount);

        // Act
        Func<Task> act = async () => await _sut.CreateGiftCheckoutSessionAsync(userId, request);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage($"The recipient {recipientEmail} already has access to this course.");
    }
}
