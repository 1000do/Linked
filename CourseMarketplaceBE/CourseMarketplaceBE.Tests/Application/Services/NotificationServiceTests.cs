using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using CourseMarketplaceBE.Application.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class NotificationServiceTests
    {
        private readonly INotificationRepository _repoMock;
        private readonly IUserRepository _userRepoMock;
        private readonly IHubContext<NotificationHub> _hubContextMock;
        private readonly IHubClients _hubClientsMock;
        private readonly IClientProxy _clientProxyMock;
        private readonly IMapper _mapperMock;
        private readonly NotificationService _sut;

        public NotificationServiceTests()
        {
            _repoMock = Substitute.For<INotificationRepository>();
            _userRepoMock = Substitute.For<IUserRepository>();
            _hubContextMock = Substitute.For<IHubContext<NotificationHub>>();
            _hubClientsMock = Substitute.For<IHubClients>();
            _clientProxyMock = Substitute.For<IClientProxy>();
            _mapperMock = Substitute.For<IMapper>();

            _hubContextMock.Clients.Returns(_hubClientsMock);
            _hubClientsMock.All.Returns(_clientProxyMock);
            _hubClientsMock.User(Arg.Any<string>()).Returns(_clientProxyMock);
            _hubClientsMock.Group(Arg.Any<string>()).Returns(_clientProxyMock);

            _sut = new NotificationService(_repoMock, _userRepoMock, _hubContextMock, _mapperMock);
        }

        // 1. GetNotificationsForUserAsync
        [Fact]
        public async Task GetNotificationsForUserAsync_WhenExists_ShouldReturnMappedNotifications()
        {
            //Arrange 1
            int userId = 1;
            var entities = new List<Notification> { new Notification { NotificationId = 1 } };
            var expectedDtos = new List<NotificationResponseDto> { new NotificationResponseDto { NotificationId = 1 } };

            //Arrange 2
            _repoMock.GetByReceiverIdAsync(userId, 1, int.MaxValue).Returns((entities, 1));
            _mapperMock.Map<List<NotificationResponseDto>>(entities).Returns(expectedDtos);

            //Act
            var result = await _sut.GetNotificationsForUserAsync(userId);

            //Assert
            result.Items.Should().BeEquivalentTo(expectedDtos);
            result.TotalCount.Should().Be(1);
            await _repoMock.Received(1).GetByReceiverIdAsync(userId, 1, int.MaxValue);
            _mapperMock.Received(1).Map<List<NotificationResponseDto>>(entities);
        }

        [Fact]
        public async Task GetNotificationsForUserAsync_WhenEmpty_ShouldReturnEmptyList()
        {
            //Arrange 1
            int userId = 1;
            var entities = new List<Notification>();
            var expectedDtos = new List<NotificationResponseDto>();

            //Arrange 2
            _repoMock.GetByReceiverIdAsync(userId, 1, int.MaxValue).Returns((entities, 0));

            //Act
            Func<Task> act = async () => await _sut.GetNotificationsForUserAsync(userId);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("No notifications found.");
            await _repoMock.Received(1).GetByReceiverIdAsync(userId, 1, int.MaxValue);
        }

        // 2. GetAllNotificationsAsync
        [Fact]
        public async Task GetAllNotificationsAsync_WhenExists_ShouldReturnMappedNotifications()
        {
            //Arrange 1
            var entities = new List<Notification> { new Notification { NotificationId = 1 } };
            var expectedDtos = new List<NotificationResponseDto> { new NotificationResponseDto { NotificationId = 1 } };

            //Arrange 2
            _repoMock.GetAllAsync(1, int.MaxValue).Returns((entities, 1));
            _mapperMock.Map<List<NotificationResponseDto>>(entities).Returns(expectedDtos);

            //Act
            var result = await _sut.GetAllNotificationsAsync();

            //Assert
            result.Items.Should().BeEquivalentTo(expectedDtos);
            result.TotalCount.Should().Be(1);
            await _repoMock.Received(1).GetAllAsync(1, int.MaxValue);
        }

        // 3. GetAllNotificationsForAdminAsync
        [Fact]
        public async Task GetAllNotificationsForAdminAsync_WhenExists_ShouldMapRolesAndReturnList()
        {
            //Arrange 1
            var entities = new List<Notification> 
            { 
                new Notification 
                { 
                    NotificationId = 1,
                    Receiver = new Account { User = new User { FullName = "Test" } } 
                } 
            };

            //Arrange 2
            _repoMock.AutoCleanupAdminNotificationsAsync().Returns(1);
            _repoMock.GetAllAsync(1, int.MaxValue).Returns((entities, 1));
            
            //Act
            var result = await _sut.GetAllNotificationsForAdminAsync();

            //Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().NotificationId.Should().Be(1);
            result.TotalCount.Should().Be(1);
            await _repoMock.Received(1).AutoCleanupAdminNotificationsAsync();
            await _repoMock.Received(1).GetAllAsync(1, int.MaxValue);
        }

        // 4. GetUnreadCountAsync
        [Fact]
        public async Task GetUnreadCountAsync_ShouldReturnIntegerCount()
        {
            //Arrange 1
            int userId = 1;
            int count = 5;

            //Arrange 2
            _repoMock.GetUnreadCountAsync(userId).Returns(count);

            //Act
            var result = await _sut.GetUnreadCountAsync(userId);

            //Assert
            result.Should().Be(count);
            await _repoMock.Received(1).GetUnreadCountAsync(userId);
        }

        // 5. SearchEmailsAsync
        [Fact]
        public async Task SearchEmailsAsync_ShouldReturnListOfEmailsFromRepository()
        {
            //Arrange 1
            string query = "test";
            int senderId = 1;
            string senderRole = "admin";
            var expectedEmails = new List<string> { "test@example.com" };

            //Arrange 2
            _userRepoMock.SearchEmailsByQueryAsync(query, senderId, senderRole).Returns(expectedEmails);

            //Act
            var result = await _sut.SearchEmailsAsync(query, senderId, senderRole);

            //Assert
            result.Should().BeEquivalentTo(expectedEmails);
            await _userRepoMock.Received(1).SearchEmailsByQueryAsync(query, senderId, senderRole);
        }

        // 6. SendNotificationAsync
        [Fact]
        public async Task SendNotificationAsync_WhenValid_ShouldAddNotificationAndTriggerSignalR()
        {
            //Arrange 1
            int receiverId = 1;
            string title = "Title";
            string content = "Content";
            string linkAction = "/link";

            //Arrange 2
            _repoMock.AddAsync(Arg.Any<Notification>()).Returns(Task.CompletedTask);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            await _sut.SendNotificationAsync(receiverId, title, content, linkAction);

            //Assert
            await _repoMock.Received(1).AddAsync(Arg.Is<Notification>(n => n.ReceiverId == receiverId && n.Title == title && n.Content == content));
            await _repoMock.Received(1).SaveChangesAsync();
            await _clientProxyMock.Received(2).SendCoreAsync("ReceiveNotification", Arg.Any<object[]>(), Arg.Any<System.Threading.CancellationToken>());
        }

        [Fact]
        public async Task SendNotificationAsync_WhenNotSaved_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int receiverId = 1;
            string title = "Title";
            string content = "Content";

            //Arrange 2
            _repoMock.AddAsync(Arg.Any<Notification>()).Returns(Task.CompletedTask);
            _repoMock.SaveChangesAsync().Returns(0);

            //Act
            Func<Task> act = async () => await _sut.SendNotificationAsync(receiverId, title, content, null);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failed to send notification.");
            await _repoMock.Received(1).AddAsync(Arg.Any<Notification>());
            await _repoMock.Received(1).SaveChangesAsync();
        }

        // 7. SendAdvancedAsync
        [Fact]
        public async Task SendAdvancedAsync_WhenTargetTypeAllAndSenderStaff_ShouldSendToStaffTargets()
        {
            //Arrange 1
            var dto = new NotificationAdvancedDto { TargetType = "ALL", Title = "T", Content = "C" };
            int senderId = 1;
            string senderRole = "staff";
            var targetIds = new List<int> { 2, 3, 1 }; // includes sender

            //Arrange 2
            _userRepoMock.GetUserIdsForStaffSenderAsync().Returns(targetIds);
            _repoMock.AddRangeAsync(Arg.Any<IEnumerable<Notification>>()).Returns(Task.CompletedTask);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.SendAdvancedAsync(dto, senderId, senderRole);

            //Assert
            result.Should().Be(2); // sender removed
            await _userRepoMock.Received(1).GetUserIdsForStaffSenderAsync();
            await _repoMock.Received(1).AddRangeAsync(Arg.Is<IEnumerable<Notification>>(x => x.Count() == 2));
        }

        [Fact]
        public async Task SendAdvancedAsync_WhenTargetTypeAllAndSenderAdmin_ShouldSendToAdminTargets()
        {
            //Arrange 1
            var dto = new NotificationAdvancedDto { TargetType = "ALL", Title = "T", Content = "C" };
            int senderId = 1;
            string senderRole = "admin";
            var targetIds = new List<int> { 2 };

            //Arrange 2
            _userRepoMock.GetUserIdsForAdminSenderAsync().Returns(targetIds);
            _repoMock.AddRangeAsync(Arg.Any<IEnumerable<Notification>>()).Returns(Task.CompletedTask);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.SendAdvancedAsync(dto, senderId, senderRole);

            //Assert
            result.Should().Be(1);
            await _userRepoMock.Received(1).GetUserIdsForAdminSenderAsync();
            await _repoMock.Received(1).AddRangeAsync(Arg.Is<IEnumerable<Notification>>(x => x.Count() == 1));
        }

        [Fact]
        public async Task SendAdvancedAsync_WhenTargetTypeAllAndSenderSystem_ShouldSendToAllTargets()
        {
            //Arrange 1
            var dto = new NotificationAdvancedDto { TargetType = "ALL", Title = "T", Content = "C" };
            int senderId = 1;
            string senderRole = "system";
            var targetIds = new List<int> { 2 };

            //Arrange 2
            _userRepoMock.GetAllUserIdsAsync().Returns(targetIds);
            _repoMock.AddRangeAsync(Arg.Any<IEnumerable<Notification>>()).Returns(Task.CompletedTask);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.SendAdvancedAsync(dto, senderId, senderRole);

            //Assert
            result.Should().Be(1);
            await _userRepoMock.Received(1).GetAllUserIdsAsync();
        }

        [Fact]
        public async Task SendAdvancedAsync_WhenEmailsProvided_ShouldSendToSpecificValidEmails()
        {
            //Arrange 1
            var dto = new NotificationAdvancedDto { TargetType = "SPECIFIC", Emails = new List<string> { "a@a.com" }, Title = "T", Content = "C" };
            int senderId = 1;
            string senderRole = "admin";
            var account = new Account { AccountId = 2 };

            //Arrange 2
            _userRepoMock.GetAccountByEmailAsync("a@a.com").Returns(account);
            _userRepoMock.GetRoleByAccountIdAsync(2).Returns("student");
            _repoMock.AddRangeAsync(Arg.Any<IEnumerable<Notification>>()).Returns(Task.CompletedTask);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.SendAdvancedAsync(dto, senderId, senderRole);

            //Assert
            result.Should().Be(1);
            await _repoMock.Received(1).AddRangeAsync(Arg.Is<IEnumerable<Notification>>(x => x.Count() == 1));
            await _repoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task SendAdvancedAsync_WhenSenderSendsToSelf_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            var dto = new NotificationAdvancedDto { TargetType = "SPECIFIC", Emails = new List<string> { "me@me.com" }, Title = "T" };
            int senderId = 1;
            var account = new Account { AccountId = 1 };

            //Arrange 2
            _userRepoMock.GetAccountByEmailAsync("me@me.com").Returns(account);

            //Act
            Func<Task> act = async () => await _sut.SendAdvancedAsync(dto, senderId, "admin");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You cannot send a notification to yourself.");
        }

        [Fact]
        public async Task SendAdvancedAsync_WhenStaffSendsToAdmin_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            var dto = new NotificationAdvancedDto { TargetType = "SPECIFIC", Emails = new List<string> { "admin@admin.com" }, Title = "T" };
            int senderId = 1;
            var account = new Account { AccountId = 2 };

            //Arrange 2
            _userRepoMock.GetAccountByEmailAsync("admin@admin.com").Returns(account);
            _userRepoMock.GetRoleByAccountIdAsync(2).Returns("admin");

            //Act
            Func<Task> act = async () => await _sut.SendAdvancedAsync(dto, senderId, "staff");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Staff cannot send notifications to staff or admin.");
        }

        [Fact]
        public async Task SendAdvancedAsync_WhenTargetListIsEmpty_ShouldReturnZero()
        {
            //Arrange 1
            var dto = new NotificationAdvancedDto { TargetType = "ALL", Title = "T", Content = "C" };
            int senderId = 1;
            string senderRole = "admin";
            var targetIds = new List<int> { 1 }; // Only sender

            //Arrange 2
            _userRepoMock.GetUserIdsForAdminSenderAsync().Returns(targetIds);

            //Act
            var result = await _sut.SendAdvancedAsync(dto, senderId, senderRole);

            //Assert
            result.Should().Be(0);
            await _repoMock.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<Notification>>());
        }

        [Fact]
        public async Task SendAdvancedAsync_WhenNotSaved_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            var dto = new NotificationAdvancedDto { TargetType = "ALL", Title = "T", Content = "C" };
            int senderId = 1;
            var targetIds = new List<int> { 2 };

            //Arrange 2
            _userRepoMock.GetUserIdsForAdminSenderAsync().Returns(targetIds);
            _repoMock.AddRangeAsync(Arg.Any<IEnumerable<Notification>>()).Returns(Task.CompletedTask);
            _repoMock.SaveChangesAsync().Returns(0);

            //Act
            Func<Task> act = async () => await _sut.SendAdvancedAsync(dto, senderId, "admin");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failed to save changes");
        }

        // 8. SendBulkNotificationsAsync
        [Fact]
        public async Task SendBulkNotificationsAsync_WhenValid_ShouldAddNotificationsAndTriggerSignalR()
        {
            //Arrange 1
            var dtos = new List<NotificationBulkDto> { new NotificationBulkDto { ReceiverId = 1, Title = "T", Content = "C" } };

            //Arrange 2
            _repoMock.AddRangeAsync(Arg.Any<IEnumerable<Notification>>()).Returns(Task.CompletedTask);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            await _sut.SendBulkNotificationsAsync(dtos);

            //Assert
            await _repoMock.Received(1).AddRangeAsync(Arg.Is<IEnumerable<Notification>>(x => x.Count() == 1));
            await _repoMock.Received(1).SaveChangesAsync();
            await _clientProxyMock.Received(2).SendCoreAsync("ReceiveNotification", Arg.Any<object[]>(), Arg.Any<System.Threading.CancellationToken>());
        }

        [Fact]
        public async Task SendBulkNotificationsAsync_WhenDtosIsEmpty_ShouldReturnTrueEarly()
        {
            //Arrange 1
            var dtos = new List<NotificationBulkDto>();

            //Arrange 2
            // No setup needed

            //Act
            await _sut.SendBulkNotificationsAsync(dtos);

            //Assert
            await _repoMock.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<Notification>>());
            await _repoMock.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task SendBulkNotificationsAsync_WhenNotSaved_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            var dtos = new List<NotificationBulkDto> { new NotificationBulkDto { ReceiverId = 1, Title = "T" } };

            //Arrange 2
            _repoMock.AddRangeAsync(Arg.Any<IEnumerable<Notification>>()).Returns(Task.CompletedTask);
            _repoMock.SaveChangesAsync().Returns(0);

            //Act
            Func<Task> act = async () => await _sut.SendBulkNotificationsAsync(dtos);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failed to save changes");
        }

        // 9. DeleteNotificationAsync
        [Fact]
        public async Task DeleteNotificationAsync_WhenValid_ShouldDeleteAndTriggerSignalR()
        {
            //Arrange 1
            int notiId = 1;
            int userId = 1;
            var noti = new Notification { NotificationId = notiId, ReceiverId = userId };

            //Arrange 2
            _repoMock.GetByIdAsync(notiId).Returns(noti);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.DeleteNotificationAsync(notiId, userId);

            //Assert
            result.Should().BeTrue();
            _repoMock.Received(1).Delete(noti);
            await _repoMock.Received(1).SaveChangesAsync();
            await _clientProxyMock.Received(1).SendCoreAsync("ReceiveNotification", Arg.Any<object[]>(), Arg.Any<System.Threading.CancellationToken>());
        }

        [Fact]
        public async Task DeleteNotificationAsync_WhenNotFoundOrWrongUser_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int notiId = 1;
            int userId = 1;

            //Arrange 2
            _repoMock.GetByIdAsync(notiId).Returns((Notification?)null);

            //Act
            Func<Task> act = async () => await _sut.DeleteNotificationAsync(notiId, userId);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Notification not found.");
        }

        [Fact]
        public async Task DeleteNotificationAsync_WhenNotSaved_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int notiId = 1;
            int userId = 1;
            var noti = new Notification { NotificationId = notiId, ReceiverId = userId };

            //Arrange 2
            _repoMock.GetByIdAsync(notiId).Returns(noti);
            _repoMock.SaveChangesAsync().Returns(0);

            //Act
            Func<Task> act = async () => await _sut.DeleteNotificationAsync(notiId, userId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failed to delete notification.");
        }

        // 10. MarkAsReadAsync
        [Fact]
        public async Task MarkAsReadAsync_WhenValid_ShouldUpdateAndTriggerSignalR()
        {
            //Arrange 1
            int notiId = 1;
            int userId = 1;
            var noti = new Notification { NotificationId = notiId, ReceiverId = userId, IsRead = false };

            //Arrange 2
            _repoMock.GetByIdAsync(notiId).Returns(noti);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.MarkAsReadAsync(notiId, userId);

            //Assert
            result.Should().BeTrue();
            noti.IsRead.Should().BeTrue();
            _repoMock.Received(1).Update(noti);
            await _repoMock.Received(1).SaveChangesAsync();
            await _clientProxyMock.Received(1).SendCoreAsync("ReceiveNotification", Arg.Any<object[]>(), Arg.Any<System.Threading.CancellationToken>());
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenNotFoundOrWrongUser_ShouldThrowKeyNotFoundException()
        {
            //Arrange 1
            int notiId = 1;
            int userId = 1;

            //Arrange 2
            _repoMock.GetByIdAsync(notiId).Returns((Notification?)null);

            //Act
            Func<Task> act = async () => await _sut.MarkAsReadAsync(notiId, userId);

            //Assert
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Notification not found.");
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenNotSaved_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int notiId = 1;
            int userId = 1;
            var noti = new Notification { NotificationId = notiId, ReceiverId = userId, IsRead = false };

            //Arrange 2
            _repoMock.GetByIdAsync(notiId).Returns(noti);
            _repoMock.SaveChangesAsync().Returns(0);

            //Act
            Func<Task> act = async () => await _sut.MarkAsReadAsync(notiId, userId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failed to mark as read.");
        }

        // 11. MarkAllAsReadAsync
        [Fact]
        public async Task MarkAllAsReadAsync_WhenHasUnreads_ShouldUpdateAllAndTriggerSignalR()
        {
            //Arrange 1
            int userId = 1;
            var unreads = new List<Notification> { new Notification { NotificationId = 1, IsRead = false } };

            //Arrange 2
            _repoMock.GetUnreadByReceiverIdAsync(userId).Returns(unreads);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.MarkAllAsReadAsync(userId);

            //Assert
            result.Should().BeTrue();
            unreads[0].IsRead.Should().BeTrue();
            _repoMock.Received(1).UpdateRange(unreads);
            await _repoMock.Received(1).SaveChangesAsync();
            await _clientProxyMock.Received(2).SendCoreAsync("ReceiveNotification", Arg.Any<object[]>(), Arg.Any<System.Threading.CancellationToken>());
        }

        [Fact]
        public async Task MarkAllAsReadAsync_WhenNoUnreads_ShouldReturnTrueEarly()
        {
            //Arrange 1
            int userId = 1;
            var emptyList = new List<Notification>();

            //Arrange 2
            _repoMock.GetUnreadByReceiverIdAsync(userId).Returns(emptyList);

            //Act
            var result = await _sut.MarkAllAsReadAsync(userId);

            //Assert
            result.Should().BeTrue();
            await _repoMock.Received(1).GetUnreadByReceiverIdAsync(userId);
            _repoMock.DidNotReceive().UpdateRange(Arg.Any<IEnumerable<Notification>>());
        }

        [Fact]
        public async Task MarkAllAsReadAsync_WhenNotSaved_ShouldThrowInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var unreads = new List<Notification> { new Notification { NotificationId = 1, IsRead = false } };

            //Arrange 2
            _repoMock.GetUnreadByReceiverIdAsync(userId).Returns(unreads);
            _repoMock.SaveChangesAsync().Returns(0);

            //Act
            Func<Task> act = async () => await _sut.MarkAllAsReadAsync(userId);

            //Assert
            // Wait! The current concrete implementation does not throw when n == 0. So this will fail! 
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
