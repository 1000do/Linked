using System;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
using Xunit;
using System.Reflection;

namespace CourseMarketplaceBE.Tests.Application.Services;

public class GiftServiceTests
{
    private readonly IGiftRepository _giftRepoMock;
    private readonly ICourseRepository _courseRepoMock;
    private readonly IUserRepository _userRepoMock;
    private readonly IEnrollmentRepository _enrollmentRepoMock;
    private readonly ITransactionRepository _transactionRepoMock;
    private readonly IHubContext<FinanceHub> _hubContextMock;
    private readonly IHubClients _hubClientsMock;
    private readonly IClientProxy _clientProxyMock;
    private readonly IDbContextTransaction _transactionMock;
    
    private readonly GiftService _sut;

    public GiftServiceTests()
    {
        _giftRepoMock = Substitute.For<IGiftRepository>();
        _courseRepoMock = Substitute.For<ICourseRepository>();
        _userRepoMock = Substitute.For<IUserRepository>();
        _enrollmentRepoMock = Substitute.For<IEnrollmentRepository>();
        _transactionRepoMock = Substitute.For<ITransactionRepository>();
        
        _hubContextMock = Substitute.For<IHubContext<FinanceHub>>();
        _hubClientsMock = Substitute.For<IHubClients>();
        _clientProxyMock = Substitute.For<IClientProxy>();
        
        _hubContextMock.Clients.Returns(_hubClientsMock);
        _hubClientsMock.Group(Arg.Any<string>()).Returns(_clientProxyMock);
        _hubClientsMock.All.Returns(_clientProxyMock);

        _transactionMock = Substitute.For<IDbContextTransaction>();
        _enrollmentRepoMock.BeginTransactionAsync().Returns(_transactionMock);

        _sut = new GiftService(
            _giftRepoMock,
            _courseRepoMock,
            _userRepoMock,
            _enrollmentRepoMock,
            _transactionRepoMock,
            _hubContextMock);
    }

    // ValidateGiftTokenAsync Tests
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task ValidateGiftTokenAsync_EmptyToken_ThrowsArgumentException(string? token)
    {
        //Arrange 1

        //Arrange 2
        
        //Act
        Func<Task> act = async () => await _sut.ValidateGiftTokenAsync(token);

        //Assert
        var ex = await act.Should().ThrowAsync<ArgumentException>();
        ex.WithMessage("Token cannot be empty.*");
    }

    [Fact]
    public async Task ValidateGiftTokenAsync_TokenNotFound_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var token = "valid_token";

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns((Gift)null);
        
        //Act
        Func<Task> act = async () => await _sut.ValidateGiftTokenAsync(token);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Gift token is invalid or does not exist.");
    }

    [Fact]
    public async Task ValidateGiftTokenAsync_GiftRefunded_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var token = "valid_token";
        var gift = new Gift { DeliveryStatus = "refunded" };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        
        //Act
        Func<Task> act = async () => await _sut.ValidateGiftTokenAsync(token);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("This gift has been refunded and is no longer active.");
    }

    [Fact]
    public async Task ValidateGiftTokenAsync_CourseNotFound_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var token = "valid_token";
        var gift = new Gift { DeliveryStatus = "active" }; // OrderItem is null

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        
        //Act
        Func<Task> act = async () => await _sut.ValidateGiftTokenAsync(token);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Course associated with this gift was not found.");
    }

    [Fact]
    public async Task ValidateGiftTokenAsync_SenderHasFullName_ReturnsWithFullName()
    {
        //Arrange 1
        var token = "valid_token";
        var gift = new Gift 
        { 
            GiftId = 1,
            SenderId = 2,
            DeliveryStatus = "active", 
            OrderItem = new OrderItem { Course = new Course { Title = "Course 1", CourseThumbnailUrl = "url" } },
            RecipientName = "Recipient",
            GiftMessage = "Message",
            CardTheme = "theme",
            IsClaimed = false
        };
        var senderAccount = new Account { User = new User { FullName = "John Doe" }, Username = "john", Email = "j@d.com" };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        _userRepoMock.GetAccountByIdAsync(2).Returns(senderAccount);
        
        //Act
        var result = await _sut.ValidateGiftTokenAsync(token);

        //Assert
        result.SenderName.Should().Be("John Doe");
        result.CourseTitle.Should().Be("Course 1");
        result.CardTheme.Should().Be("theme");
    }

    [Fact]
    public async Task ValidateGiftTokenAsync_SenderHasUsernameOnly_ReturnsWithUsername()
    {
        //Arrange 1
        var token = "valid_token";
        var gift = new Gift 
        { 
            SenderId = 2,
            DeliveryStatus = "active", 
            OrderItem = new OrderItem { Course = new Course { Title = "Course 1" } }
        };
        var senderAccount = new Account { Username = "john", Email = "j@d.com" };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        _userRepoMock.GetAccountByIdAsync(2).Returns(senderAccount);
        
        //Act
        var result = await _sut.ValidateGiftTokenAsync(token);

        //Assert
        result.SenderName.Should().Be("john");
    }

    [Fact]
    public async Task ValidateGiftTokenAsync_SenderHasEmailOnly_ReturnsWithEmail()
    {
        //Arrange 1
        var token = "valid_token";
        var gift = new Gift 
        { 
            SenderId = 2,
            DeliveryStatus = "active", 
            OrderItem = new OrderItem { Course = new Course { Title = "Course 1" } }
        };
        var senderAccount = new Account { Email = "j@d.com" };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        _userRepoMock.GetAccountByIdAsync(2).Returns(senderAccount);
        
        //Act
        var result = await _sut.ValidateGiftTokenAsync(token);

        //Assert
        result.SenderName.Should().Be("j@d.com");
    }

    [Fact]
    public async Task ValidateGiftTokenAsync_SenderAccountNotFound_ReturnsDefaultName()
    {
        //Arrange 1
        var token = "valid_token";
        var gift = new Gift 
        { 
            SenderId = 2,
            DeliveryStatus = "active", 
            OrderItem = new OrderItem { Course = new Course { Title = "Course 1" } }
        };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        _userRepoMock.GetAccountByIdAsync(2).Returns((Account)null);
        
        //Act
        var result = await _sut.ValidateGiftTokenAsync(token);

        //Assert
        result.SenderName.Should().Be("A Friend");
    }

    [Fact]
    public async Task ValidateGiftTokenAsync_NoSenderId_ReturnsDefaultName()
    {
        //Arrange 1
        var token = "valid_token";
        var gift = new Gift 
        { 
            SenderId = null,
            DeliveryStatus = "active", 
            OrderItem = new OrderItem { Course = new Course { Title = "Course 1" } },
            CardTheme = null // default to classic
        };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        
        //Act
        var result = await _sut.ValidateGiftTokenAsync(token);

        //Assert
        result.SenderName.Should().Be("A Friend");
        result.CardTheme.Should().Be("classic");
    }

    // ClaimGiftAsync Tests
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task ClaimGiftAsync_EmptyToken_ThrowsArgumentException(string? token)
    {
        //Arrange 1
        var userId = 1;

        //Arrange 2
        
        //Act
        Func<Task> act = async () => await _sut.ClaimGiftAsync(userId, token);

        //Assert
        var ex = await act.Should().ThrowAsync<ArgumentException>();
        ex.WithMessage("Token cannot be empty.*");
    }

    [Fact]
    public async Task ClaimGiftAsync_GiftNotFound_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var userId = 1;
        var token = "valid_token";

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns((Gift)null);
        
        //Act
        Func<Task> act = async () => await _sut.ClaimGiftAsync(userId, token);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Gift token is invalid or does not exist.");
    }

    [Fact]
    public async Task ClaimGiftAsync_GiftAlreadyClaimed_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var userId = 1;
        var token = "valid_token";
        var gift = new Gift { IsClaimed = true };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        
        //Act
        Func<Task> act = async () => await _sut.ClaimGiftAsync(userId, token);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("This gift has already been claimed.");
    }

    [Fact]
    public async Task ClaimGiftAsync_GiftRefunded_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var userId = 1;
        var token = "valid_token";
        var gift = new Gift { IsClaimed = false, DeliveryStatus = "refunded" };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        
        //Act
        Func<Task> act = async () => await _sut.ClaimGiftAsync(userId, token);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("This gift has been refunded and cannot be claimed.");
    }

    [Fact]
    public async Task ClaimGiftAsync_CourseIdNull_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var userId = 1;
        var token = "valid_token";
        var gift = new Gift { IsClaimed = false, DeliveryStatus = "active", OrderItem = new OrderItem { CourseId = null } };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        
        //Act
        Func<Task> act = async () => await _sut.ClaimGiftAsync(userId, token);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Course associated with this gift is invalid.");
    }
    
    [Fact]
    public async Task ClaimGiftAsync_CourseIdNull_BecauseOrderItemNull_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var userId = 1;
        var token = "valid_token";
        var gift = new Gift { IsClaimed = false, DeliveryStatus = "active", OrderItem = null };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        
        //Act
        Func<Task> act = async () => await _sut.ClaimGiftAsync(userId, token);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Course associated with this gift is invalid.");
    }

    [Fact]
    public async Task ClaimGiftAsync_CourseNotFound_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var userId = 1;
        var token = "valid_token";
        var gift = new Gift { IsClaimed = false, DeliveryStatus = "active", OrderItem = new OrderItem { CourseId = 2 } };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        _courseRepoMock.GetByIdAsync(2).Returns((Course)null);
        
        //Act
        Func<Task> act = async () => await _sut.ClaimGiftAsync(userId, token);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Course associated with this gift was not found.");
    }

    [Fact]
    public async Task ClaimGiftAsync_UserAlreadyEnrolled_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var userId = 1;
        var token = "valid_token";
        var gift = new Gift { IsClaimed = false, DeliveryStatus = "active", OrderItem = new OrderItem { CourseId = 2 } };
        var course = new Course { CourseId = 2 };
        var enrollment = new Enrollment { EnrollmentStatus = "active" };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        _courseRepoMock.GetByIdAsync(2).Returns(course);
        _enrollmentRepoMock.GetEnrollmentAsync(userId, 2).Returns(enrollment);
        
        //Act
        Func<Task> act = async () => await _sut.ClaimGiftAsync(userId, token);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("You already own this course in your library.");
    }

    [Fact]
    public async Task ClaimGiftAsync_ValidClaim_ProcessesTransactionAndSendsNotification()
    {
        //Arrange 1
        var userId = 1;
        var token = "valid_token";
        var gift = new Gift { IsClaimed = false, DeliveryStatus = "active", OrderItem = new OrderItem { CourseId = 2 }, OrderItemId = 3 };
        var course = new Course { CourseId = 2, Title = "Title", Description = "Desc" };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        _courseRepoMock.GetByIdAsync(2).Returns(course);
        _enrollmentRepoMock.GetEnrollmentAsync(userId, 2).Returns((Enrollment)null);
        _transactionRepoMock.RejectPendingRefundForGiftClaimedAsync(3).Returns(true);
        
        //Act
        await _sut.ClaimGiftAsync(userId, token);

        //Assert
        await _enrollmentRepoMock.Received(1).AddEnrollmentAsync(Arg.Is<Enrollment>(e => e.UserId == 1 && e.CourseId == 2 && e.EnrollmentStatus == "active"));
        await _enrollmentRepoMock.Received(1).SaveChangesAsync();
        _giftRepoMock.Received(1).Update(gift);
        await _giftRepoMock.Received(1).SaveChangesAsync();
        await _transactionRepoMock.Received(1).RejectPendingRefundForGiftClaimedAsync(3);
        await _transactionMock.Received(1).CommitAsync();
        
        gift.IsClaimed.Should().BeTrue();
        gift.ClaimedByUserId.Should().Be(1);
        gift.DeliveryStatus.Should().Be("sent");

        await _clientProxyMock.Received(2).SendCoreAsync("UpdatePayoutStatus", Arg.Any<object[]>());
    }

    [Fact]
    public async Task ClaimGiftAsync_RefundNotRejected_DoesNotSendNotification()
    {
        //Arrange 1
        var userId = 1;
        var token = "valid_token";
        var gift = new Gift { IsClaimed = false, DeliveryStatus = "active", OrderItem = new OrderItem { CourseId = 2 }, OrderItemId = 3 };
        var course = new Course { CourseId = 2 };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        _courseRepoMock.GetByIdAsync(2).Returns(course);
        _enrollmentRepoMock.GetEnrollmentAsync(userId, 2).Returns((Enrollment)null);
        _transactionRepoMock.RejectPendingRefundForGiftClaimedAsync(3).Returns(false);
        
        //Act
        await _sut.ClaimGiftAsync(userId, token);

        //Assert
        await _clientProxyMock.DidNotReceive().SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>());
    }

    [Fact]
    public async Task ClaimGiftAsync_NotificationFails_SwallowsExceptionAndCompletes()
    {
        //Arrange 1
        var userId = 1;
        var token = "valid_token";
        var gift = new Gift { IsClaimed = false, DeliveryStatus = "active", OrderItem = new OrderItem { CourseId = 2 }, OrderItemId = 3 };
        var course = new Course { CourseId = 2 };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        _courseRepoMock.GetByIdAsync(2).Returns(course);
        _enrollmentRepoMock.GetEnrollmentAsync(userId, 2).Returns((Enrollment)null);
        _transactionRepoMock.RejectPendingRefundForGiftClaimedAsync(3).Returns(true);
        _clientProxyMock.When(x => x.SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>())).Throw(new Exception("Hub error"));
        
        //Act
        await _sut.ClaimGiftAsync(userId, token);

        //Assert
        await _transactionMock.Received(1).CommitAsync();
        gift.IsClaimed.Should().BeTrue(); // Confirms it completed despite the swallowed exception
    }

    [Fact]
    public async Task ClaimGiftAsync_RepositoryThrowsException_RollsBackTransactionAndRethrows()
    {
        //Arrange 1
        var userId = 1;
        var token = "valid_token";
        var gift = new Gift { IsClaimed = false, DeliveryStatus = "active", OrderItem = new OrderItem { CourseId = 2 } };
        var course = new Course { CourseId = 2 };

        //Arrange 2
        _giftRepoMock.GetByTokenAsync(token).Returns(gift);
        _courseRepoMock.GetByIdAsync(2).Returns(course);
        _enrollmentRepoMock.GetEnrollmentAsync(userId, 2).Returns((Enrollment)null);
        _enrollmentRepoMock.When(x => x.AddEnrollmentAsync(Arg.Any<Enrollment>())).Throw(new Exception("DB Error"));
        
        //Act
        Func<Task> act = async () => await _sut.ClaimGiftAsync(userId, token);

        //Assert
        var ex = await act.Should().ThrowAsync<Exception>();
        ex.WithMessage("DB Error");
        await _transactionMock.Received(1).RollbackAsync();
    }

    // CreateGiftRecordAsync Tests
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task CreateGiftRecordAsync_EmptyEmail_ThrowsArgumentException(string? email)
    {
        //Arrange 1

        //Arrange 2
        
        //Act
        Func<Task> act = async () => await _sut.CreateGiftRecordAsync(1, 1, email, "Name", "Msg", "theme", "token");

        //Assert
        var ex = await act.Should().ThrowAsync<ArgumentException>();
        ex.WithMessage("Recipient email is required.*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task CreateGiftRecordAsync_EmptyToken_ThrowsArgumentException(string? token)
    {
        //Arrange 1

        //Arrange 2
        
        //Act
        Func<Task> act = async () => await _sut.CreateGiftRecordAsync(1, 1, "test@test.com", "Name", "Msg", "theme", token);

        //Assert
        var ex = await act.Should().ThrowAsync<ArgumentException>();
        ex.WithMessage("Redemption token is required.*");
    }

    [Fact]
    public async Task CreateGiftRecordAsync_GiftAlreadyExists_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var orderItemId = 1;

        //Arrange 2
        _giftRepoMock.GetByOrderItemIdAsync(orderItemId).Returns(new Gift());
        
        //Act
        Func<Task> act = async () => await _sut.CreateGiftRecordAsync(orderItemId, 1, "test@test.com", "Name", "Msg", "theme", "token");

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("A gift record already exists for this order item.");
    }

    [Fact]
    public async Task CreateGiftRecordAsync_ValidInput_CreatesGiftAndSaves()
    {
        //Arrange 1
        var orderItemId = 1;

        //Arrange 2
        _giftRepoMock.GetByOrderItemIdAsync(orderItemId).Returns((Gift)null);
        _giftRepoMock.SaveChangesAsync().Returns(1);
        
        //Act
        await _sut.CreateGiftRecordAsync(orderItemId, 2, "test@test.com", "Name", "Msg", "theme", "token");

        //Assert
        await _giftRepoMock.Received(1).AddAsync(Arg.Is<Gift>(g => g.OrderItemId == 1 && g.SenderId == 2 && g.RecipientEmail == "test@test.com" && g.RedemptionToken == "token"));
        await _giftRepoMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CreateGiftRecordAsync_NoSenderId_SetsSenderIdToNull()
    {
        //Arrange 1
        var orderItemId = 1;

        //Arrange 2
        _giftRepoMock.GetByOrderItemIdAsync(orderItemId).Returns((Gift)null);
        _giftRepoMock.SaveChangesAsync().Returns(1);
        
        //Act
        await _sut.CreateGiftRecordAsync(orderItemId, 0, "test@test.com", "Name", "Msg", "theme", "token");

        //Assert
        await _giftRepoMock.Received(1).AddAsync(Arg.Is<Gift>(g => g.SenderId == null));
    }

    // IsRecipientEnrolledAsync Tests
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task IsRecipientEnrolledAsync_EmptyEmail_ReturnsFalse(string? email)
    {
        //Arrange 1

        //Arrange 2
        
        //Act
        var result = await _sut.IsRecipientEnrolledAsync(email, 1);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsRecipientEnrolledAsync_AccountNotFound_ReturnsFalse()
    {
        //Arrange 1
        var email = "test@test.com";

        //Arrange 2
        _userRepoMock.GetAccountByEmailAsync(email).Returns((Account)null);
        
        //Act
        var result = await _sut.IsRecipientEnrolledAsync(email, 1);

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsRecipientEnrolledAsync_AccountFound_ReturnsCourseRepoResult()
    {
        //Arrange 1
        var email = "test@test.com";
        var account = new Account { AccountId = 1 };

        //Arrange 2
        _userRepoMock.GetAccountByEmailAsync(email).Returns(account);
        _courseRepoMock.IsEnrolledAsync(1, 2).Returns(true);
        
        //Act
        var result = await _sut.IsRecipientEnrolledAsync(email, 2);

        //Assert
        result.Should().BeTrue();
    }
}
