using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Application.IServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class ChatServiceTests
    {
        private readonly IChatRepository _chatRepoMock;
        private readonly IUserRepository _userRepoMock;
        private readonly ICourseRepository _courseRepoMock;
        private readonly IRedisService _redisMock;
        private readonly IConfiguration _configMock;
        private readonly ChatService _sut;

        public ChatServiceTests()
        {
            _chatRepoMock = Substitute.For<IChatRepository>();
            _userRepoMock = Substitute.For<IUserRepository>();
            _courseRepoMock = Substitute.For<ICourseRepository>();
            _redisMock = Substitute.For<IRedisService>();

            var inMemorySettings = new Dictionary<string, string> {
                {"ChatSettings:EnableAttachments", "true"}
            };
            _configMock = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _sut = new ChatService(
                _chatRepoMock,
                _userRepoMock,
                _courseRepoMock,
                _redisMock,
                _configMock
            );
        }

        // GetMyChatsAsync
        [Fact]
        public async Task GetMyChatsAsync_ParticipantsFound_ReturnsValidList()
        {
            //Arrange 1
            var accountId = 1;
            var chat = new Chat
            {
                ChatId = 1,
                ChatName = "Test Chat",
                ChatType = "private",
                LastMessageAt = DateTime.Now,
                Messages = new List<Message> { new Message { Content = "Hello" } },
                ChatParticipants = new List<ChatParticipant>
                {
                    new ChatParticipant { AccountId = accountId },
                    new ChatParticipant { AccountId = 2, Account = new Account { User = new User { FullName = "Partner Name" } } }
                }
            };
            var participants = new List<ChatParticipant>
            {
                new ChatParticipant
                {
                    ChatId = 1,
                    AccountId = accountId,
                    Chat = chat,
                    UnreadCount = 0
                }
            };

            //Arrange 2
            _chatRepoMock.GetParticipantsByAccountIdAsync(accountId).Returns(participants);
            _redisMock.GetUnreadCountAsync(accountId, 1).Returns(2);
            _redisMock.IsUserOnlineAsync(2).Returns(true);

            //Act
            var result = await _sut.GetMyChatsAsync(accountId);

            //Assert
            result.Should().HaveCount(1);
            result.First().ChatId.Should().Be(1);
            result.First().UnreadCount.Should().Be(2);
            result.First().PartnerName.Should().Be("Partner Name");
            result.First().IsOnline.Should().BeTrue();

            await _chatRepoMock.Received(1).GetParticipantsByAccountIdAsync(accountId);
            await _redisMock.Received(1).GetUnreadCountAsync(accountId, 1);
            await _redisMock.Received(1).IsUserOnlineAsync(2);
        }

        [Fact]
        public async Task GetMyChatsAsync_ChatHasNoMessages_SkipsChat()
        {
            //Arrange 1
            var accountId = 1;
            var chat = new Chat
            {
                ChatId = 1,
                Messages = new List<Message>() // No messages
            };
            var participants = new List<ChatParticipant>
            {
                new ChatParticipant { ChatId = 1, AccountId = accountId, Chat = chat }
            };

            //Arrange 2
            _chatRepoMock.GetParticipantsByAccountIdAsync(accountId).Returns(participants);

            //Act
            var result = await _sut.GetMyChatsAsync(accountId);

            //Assert
            result.Should().BeEmpty();
            await _chatRepoMock.Received(1).GetParticipantsByAccountIdAsync(accountId);
            await _redisMock.DidNotReceive().GetUnreadCountAsync(Arg.Any<int>(), Arg.Any<int>());
        }

        [Fact]
        public async Task GetMyChatsAsync_MessageBeforeClearedAt_SkipsChat()
        {
            //Arrange 1
            var accountId = 1;
            var chat = new Chat
            {
                ChatId = 1,
                LastMessageAt = DateTime.Now.AddDays(-1),
                Messages = new List<Message> { new Message { Content = "Old" } }
            };
            var participants = new List<ChatParticipant>
            {
                new ChatParticipant 
                { 
                    ChatId = 1, 
                    AccountId = accountId, 
                    Chat = chat,
                    ClearedAt = DateTime.Now // Cleared after last message
                }
            };

            //Arrange 2
            _chatRepoMock.GetParticipantsByAccountIdAsync(accountId).Returns(participants);

            //Act
            var result = await _sut.GetMyChatsAsync(accountId);

            //Assert
            result.Should().BeEmpty();
            await _chatRepoMock.Received(1).GetParticipantsByAccountIdAsync(accountId);
        }

        // SearchChatsAsync
        [Fact]
        public async Task SearchChatsAsync_ChatsFound_ReturnsFilteredList()
        {
            //Arrange 1
            var accountId = 1;
            var query = "Test";
            var chat = new Chat
            {
                ChatId = 1,
                ChatName = "Test Chat",
                ChatType = "private",
                Messages = new List<Message> { new Message { Content = "Hello" } },
                ChatParticipants = new List<ChatParticipant>
                {
                    new ChatParticipant { AccountId = accountId },
                    new ChatParticipant { AccountId = 2, Account = new Account { Manager = new Manager { DisplayName = "Manager Name" } } }
                }
            };
            var participants = new List<ChatParticipant>
            {
                new ChatParticipant { ChatId = 1, AccountId = accountId, Chat = chat, UnreadCount = 1 }
            };

            //Arrange 2
            _chatRepoMock.SearchParticipantsByAccountIdAsync(accountId, query).Returns(participants);
            _redisMock.GetUnreadCountAsync(accountId, 1).Returns(0);
            _redisMock.IsUserOnlineAsync(2).Returns(false);

            //Act
            var result = await _sut.SearchChatsAsync(accountId, query);

            //Assert
            result.Should().HaveCount(1);
            result.First().PartnerName.Should().Be("Manager Name");
            result.First().UnreadCount.Should().Be(1);

            await _chatRepoMock.Received(1).SearchParticipantsByAccountIdAsync(accountId, query);
        }

        [Fact]
        public async Task SearchChatsAsync_ChatHasNoMessages_SkipsChat()
        {
            //Arrange 1
            var accountId = 1;
            var chat = new Chat
            {
                ChatId = 1,
                Messages = new List<Message>()
            };
            var participants = new List<ChatParticipant>
            {
                new ChatParticipant { ChatId = 1, AccountId = accountId, Chat = chat }
            };

            //Arrange 2
            _chatRepoMock.SearchParticipantsByAccountIdAsync(accountId, Arg.Any<string>()).Returns(participants);

            //Act
            var result = await _sut.SearchChatsAsync(accountId, "q");

            //Assert
            result.Should().BeEmpty();
            await _chatRepoMock.Received(1).SearchParticipantsByAccountIdAsync(accountId, "q");
        }

        [Fact]
        public async Task SearchChatsAsync_MessageBeforeClearedAt_SkipsChat()
        {
            //Arrange 1
            var accountId = 1;
            var chat = new Chat
            {
                ChatId = 1,
                LastMessageAt = DateTime.Now.AddDays(-1),
                Messages = new List<Message> { new Message { Content = "Old" } }
            };
            var participants = new List<ChatParticipant>
            {
                new ChatParticipant 
                { 
                    ChatId = 1, 
                    AccountId = accountId, 
                    Chat = chat,
                    ClearedAt = DateTime.Now
                }
            };

            //Arrange 2
            _chatRepoMock.SearchParticipantsByAccountIdAsync(accountId, Arg.Any<string>()).Returns(participants);

            //Act
            var result = await _sut.SearchChatsAsync(accountId, "q");

            //Assert
            result.Should().BeEmpty();
        }

        // GetChatHistoryAsync
        [Fact]
        public async Task GetChatHistoryAsync_NoAccess_ThrowsUnauthorizedAccessException()
        {
            //Arrange 1
            var chatId = 1;
            var accountId = 1;

            //Arrange 2
            _chatRepoMock.IsParticipantAsync(chatId, accountId).Returns(false);
            _userRepoMock.GetRoleByAccountIdAsync(accountId).Returns("user");

            //Act
            Func<Task> act = async () => await _sut.GetChatHistoryAsync(chatId, accountId);

            //Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not have permission to view this chat.");
            await _chatRepoMock.Received(1).IsParticipantAsync(chatId, accountId);
        }

        [Fact]
        public async Task GetChatHistoryAsync_HasAccessAndCleared_ReturnsMessagesAfterClearedAt()
        {
            //Arrange 1
            var chatId = 1;
            var accountId = 1;
            var clearedAt = DateTime.UtcNow.AddDays(-1);
            var participant = new ChatParticipant { ClearedAt = clearedAt };
            var messages = new List<Message>
            {
                new Message { MessageId = 1, SentAt = clearedAt.AddMinutes(-10), Attachments = new List<MessageAttachment>() },
                new Message { MessageId = 2, SentAt = clearedAt.AddMinutes(10), Attachments = new List<MessageAttachment>() }
            };

            //Arrange 2
            _chatRepoMock.IsParticipantAsync(chatId, accountId).Returns(true); // Has access
            _chatRepoMock.GetParticipantAsync(chatId, accountId).Returns(participant);
            _chatRepoMock.GetMessagesByChatIdAsync(chatId).Returns(messages);

            //Act
            var result = await _sut.GetChatHistoryAsync(chatId, accountId);

            //Assert
            result.Should().HaveCount(1);
            result.First().MessageId.Should().Be(2);

            await _chatRepoMock.Received(1).MarkAsReadAsync(chatId, accountId);
            await _redisMock.Received(1).ClearUnreadCountAsync(accountId, chatId);
        }

        [Fact]
        public async Task GetChatHistoryAsync_HasAccessNotCleared_ReturnsAllMessages()
        {
            //Arrange 1
            var chatId = 1;
            var accountId = 1;
            var participant = new ChatParticipant { ClearedAt = null };
            var messages = new List<Message>
            {
                new Message { MessageId = 1, SentAt = DateTime.UtcNow.AddDays(-2), Attachments = new List<MessageAttachment>() },
                new Message { MessageId = 2, SentAt = DateTime.UtcNow.AddDays(-1), Attachments = new List<MessageAttachment> { new MessageAttachment { AttachmentId = 1, FileName = "test.png", FileUrl = "url", FileType = "png", FileSize = 100 } } }
            };

            //Arrange 2
            _chatRepoMock.IsParticipantAsync(chatId, accountId).Returns(true);
            _chatRepoMock.GetParticipantAsync(chatId, accountId).Returns(participant);
            _chatRepoMock.GetMessagesByChatIdAsync(chatId).Returns(messages);

            //Act
            var result = await _sut.GetChatHistoryAsync(chatId, accountId);

            //Assert
            result.Should().HaveCount(2);
        }

        // SaveMessageAsync
        [Fact]
        public async Task SaveMessageAsync_NotParticipantNoActiveReport_ThrowsUnauthorizedAccessException()
        {
            //Arrange 1
            var senderId = 1;
            var dto = new SendMessageDto { ChatId = 1, Content = "Hi" };

            //Arrange 2
            _chatRepoMock.IsParticipantAsync(dto.ChatId, senderId).Returns(false);
            _chatRepoMock.HasActiveReportForChatAsync(dto.ChatId).Returns(false);

            //Act
            Func<Task> act = async () => await _sut.SaveMessageAsync(senderId, dto);

            //Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("You do not have permission to send messages in this chat.");
        }

        [Fact]
        public async Task SaveMessageAsync_ValidParticipantWithAttachments_SavesMessageAndUpdatesRedis()
        {
            //Arrange 1
            var senderId = 1;
            var dto = new SendMessageDto 
            { 
                ChatId = 1, 
                Content = "Hi", 
                Attachments = new List<AttachmentInputDto> { new AttachmentInputDto { FileName = "test.png" } } 
            };
            var chat = new Chat { ChatId = 1 };
            var participantIds = new List<int> { 1, 2, 3 };
            var senderAccount = new Account { AccountId = 1, User = new User { FullName = "Sender" } };

            //Arrange 2
            _chatRepoMock.IsParticipantAsync(dto.ChatId, senderId).Returns(true);
            _chatRepoMock.GetChatByIdAsync(dto.ChatId).Returns(chat);
            _chatRepoMock.GetParticipantIdsAsync(dto.ChatId).Returns(participantIds);
            _userRepoMock.GetAccountByIdAsync(senderId).Returns(senderAccount);
            _chatRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.SaveMessageAsync(senderId, dto);

            //Assert
            result.Content.Should().Be("Hi");
            result.SenderName.Should().Be("Sender");
            result.Attachments.Should().HaveCount(1);

            await _chatRepoMock.Received(1).AddMessageAsync(Arg.Is<Message>(m => m.Content == "Hi" && m.Attachments.Count == 1));
            await _chatRepoMock.Received(1).UpdateChatAsync(chat);
            await _redisMock.Received(1).IncrementUnreadCountAsync(2, dto.ChatId);
            await _redisMock.Received(1).IncrementUnreadCountAsync(3, dto.ChatId);
            await _chatRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task SaveMessageAsync_NotParticipantButHasReportWithoutAttachments_SavesMessage()
        {
            //Arrange 1
            var senderId = 1;
            var dto = new SendMessageDto { ChatId = 1, Content = "Admin reply" };
            var chat = new Chat { ChatId = 1 };
            var participantIds = new List<int> { 2 };
            var senderAccount = new Account { AccountId = 1, Manager = new Manager { DisplayName = "Admin" } };

            //Arrange 2
            _chatRepoMock.IsParticipantAsync(dto.ChatId, senderId).Returns(false);
            _chatRepoMock.HasActiveReportForChatAsync(dto.ChatId).Returns(true); // Has report access
            _chatRepoMock.GetChatByIdAsync(dto.ChatId).Returns(chat);
            _chatRepoMock.GetParticipantIdsAsync(dto.ChatId).Returns(participantIds);
            _userRepoMock.GetAccountByIdAsync(senderId).Returns(senderAccount);

            //Act
            var result = await _sut.SaveMessageAsync(senderId, dto);

            //Assert
            result.SenderName.Should().Be("Admin");
            await _chatRepoMock.Received(1).AddMessageAsync(Arg.Is<Message>(m => m.Content == "Admin reply"));
        }

        // GetOrCreateChatAsync
        [Fact]
        public async Task GetOrCreateChatAsync_RoleIsAdmin_CreatesChat()
        {
            //Arrange 1
            var senderId = 1;
            var dto = new CreateChatDto { TargetAccountId = 2, ContextType = "system" };
            var createdChat = new Chat { ChatId = 5 };

            //Arrange 2
            _userRepoMock.GetRoleByAccountIdAsync(senderId).Returns("admin");
            _userRepoMock.GetRoleByAccountIdAsync(dto.TargetAccountId).Returns("user");
            _chatRepoMock.FindPrivateChatAsync(senderId, dto.TargetAccountId, dto.ContextType, null).Returns((ChatParticipant?)null);
            _chatRepoMock.CreateChatAsync(Arg.Any<Chat>()).Returns(createdChat);

            //Act
            var result = await _sut.GetOrCreateChatAsync(senderId, dto);

            //Assert
            result.Should().Be(5);
            await _chatRepoMock.Received(1).CreateChatAsync(Arg.Any<Chat>());
        }

        [Fact]
        public async Task GetOrCreateChatAsync_RoleIsUserTargetIsAdmin_CreatesChat()
        {
            //Arrange 1
            var senderId = 1;
            var dto = new CreateChatDto { TargetAccountId = 2, ContextType = "system" };
            var createdChat = new Chat { ChatId = 6 };

            //Arrange 2
            _userRepoMock.GetRoleByAccountIdAsync(senderId).Returns("user");
            _userRepoMock.GetRoleByAccountIdAsync(dto.TargetAccountId).Returns("admin");
            _chatRepoMock.FindPrivateChatAsync(senderId, dto.TargetAccountId, dto.ContextType, null).Returns((ChatParticipant?)null);
            _chatRepoMock.CreateChatAsync(Arg.Any<Chat>()).Returns(createdChat);

            //Act
            var result = await _sut.GetOrCreateChatAsync(senderId, dto);

            //Assert
            result.Should().Be(6);
        }

        [Fact]
        public async Task GetOrCreateChatAsync_RoleIsUserTargetIsUserNotEnrolled_ThrowsUnauthorizedAccessException()
        {
            //Arrange 1
            var senderId = 1;
            var dto = new CreateChatDto { TargetAccountId = 2, ContextType = "course", ContextId = 100 };

            //Arrange 2
            _userRepoMock.GetRoleByAccountIdAsync(senderId).Returns("user");
            _userRepoMock.GetRoleByAccountIdAsync(dto.TargetAccountId).Returns("user");
            _courseRepoMock.IsEnrolledAsync(senderId, 100).Returns(false);
            _courseRepoMock.IsEnrolledAsync(dto.TargetAccountId, 100).Returns(false);

            //Act
            Func<Task> act = async () => await _sut.GetOrCreateChatAsync(senderId, dto);

            //Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task GetOrCreateChatAsync_RoleIsUserTargetIsUserEnrolled_CreatesChat()
        {
            //Arrange 1
            var senderId = 1;
            var dto = new CreateChatDto { TargetAccountId = 2, ContextType = "course", ContextId = 100 };
            var createdChat = new Chat { ChatId = 7 };

            //Arrange 2
            _userRepoMock.GetRoleByAccountIdAsync(senderId).Returns("user");
            _userRepoMock.GetRoleByAccountIdAsync(dto.TargetAccountId).Returns("user");
            _courseRepoMock.IsEnrolledAsync(senderId, 100).Returns(true); // Is enrolled
            _chatRepoMock.FindPrivateChatAsync(senderId, dto.TargetAccountId, dto.ContextType, 100).Returns((ChatParticipant?)null);
            _chatRepoMock.CreateChatAsync(Arg.Any<Chat>()).Returns(createdChat);

            //Act
            var result = await _sut.GetOrCreateChatAsync(senderId, dto);

            //Assert
            result.Should().Be(7);
        }

        [Fact]
        public async Task GetOrCreateChatAsync_ChatAlreadyExists_ReturnsExistingChatId()
        {
            //Arrange 1
            var senderId = 1;
            var dto = new CreateChatDto { TargetAccountId = 2, ContextType = "system" };
            var existingParticipant = new ChatParticipant { ChatId = 8 };

            //Arrange 2
            _userRepoMock.GetRoleByAccountIdAsync(senderId).Returns("admin");
            _chatRepoMock.FindPrivateChatAsync(senderId, dto.TargetAccountId, dto.ContextType, null).Returns(existingParticipant);

            //Act
            var result = await _sut.GetOrCreateChatAsync(senderId, dto);

            //Assert
            result.Should().Be(8);
            await _chatRepoMock.DidNotReceive().CreateChatAsync(Arg.Any<Chat>());
        }

        // HasAccessToChatAsync
        [Fact]
        public async Task HasAccessToChatAsync_IsParticipant_ReturnsTrue()
        {
            //Arrange 1
            var accountId = 1;
            var chatId = 1;

            //Arrange 2
            _chatRepoMock.IsParticipantAsync(chatId, accountId).Returns(true);

            //Act
            var result = await _sut.HasAccessToChatAsync(accountId, chatId);

            //Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasAccessToChatAsync_NotParticipantRoleIsAdminHasReport_ReturnsTrue()
        {
            //Arrange 1
            var accountId = 1;
            var chatId = 1;

            //Arrange 2
            _chatRepoMock.IsParticipantAsync(chatId, accountId).Returns(false);
            _userRepoMock.GetRoleByAccountIdAsync(accountId).Returns("admin");
            _chatRepoMock.HasActiveReportForChatAsync(chatId).Returns(true);

            //Act
            var result = await _sut.HasAccessToChatAsync(accountId, chatId);

            //Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasAccessToChatAsync_NotParticipantRoleIsAdminNoReport_ReturnsFalse()
        {
            //Arrange 1
            var accountId = 1;
            var chatId = 1;

            //Arrange 2
            _chatRepoMock.IsParticipantAsync(chatId, accountId).Returns(false);
            _userRepoMock.GetRoleByAccountIdAsync(accountId).Returns("admin");
            _chatRepoMock.HasActiveReportForChatAsync(chatId).Returns(false);

            //Act
            var result = await _sut.HasAccessToChatAsync(accountId, chatId);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasAccessToChatAsync_NotParticipantRoleIsUser_ReturnsFalse()
        {
            //Arrange 1
            var accountId = 1;
            var chatId = 1;

            //Arrange 2
            _chatRepoMock.IsParticipantAsync(chatId, accountId).Returns(false);
            _userRepoMock.GetRoleByAccountIdAsync(accountId).Returns("user");

            //Act
            var result = await _sut.HasAccessToChatAsync(accountId, chatId);

            //Assert
            result.Should().BeFalse();
        }

        // GetParticipantIdsAsync
        [Fact]
        public async Task GetParticipantIdsAsync_ValidChatId_ReturnsParticipantIds()
        {
            //Arrange 1
            var chatId = 1;
            var ids = new List<int> { 1, 2 };

            //Arrange 2
            _chatRepoMock.GetParticipantIdsAsync(chatId).Returns(ids);

            //Act
            var result = await _sut.GetParticipantIdsAsync(chatId);

            //Assert
            result.Should().BeEquivalentTo(ids);
            await _chatRepoMock.Received(1).GetParticipantIdsAsync(chatId);
        }

        // GrantAdminAccessAsync
        [Fact]
        public async Task GrantAdminAccessAsync_ValidInputs_UpdatesAccessAndReturnsTrue()
        {
            //Arrange 1
            var chatId = 1;
            var hours = 24;

            //Arrange 2
            _chatRepoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.GrantAdminAccessAsync(chatId, hours);

            //Assert
            result.Should().BeTrue();
            await _chatRepoMock.Received(1).UpdateAdminAccessAsync(chatId, Arg.Is<DateTime>(d => d > DateTime.Now.AddHours(23)));
            await _chatRepoMock.Received(1).SaveChangesAsync();
        }

        // ClearChatHistoryAsync
        [Fact]
        public async Task ClearChatHistoryAsync_ParticipantNotFound_ReturnsFalse()
        {
            //Arrange 1
            var chatId = 1;
            var accountId = 1;

            //Arrange 2
            _chatRepoMock.GetParticipantAsync(chatId, accountId).Returns((ChatParticipant?)null);

            //Act
            var result = await _sut.ClearChatHistoryAsync(chatId, accountId);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ClearChatHistoryAsync_ParticipantFound_ClearsHistoryAndReturnsTrue()
        {
            //Arrange 1
            var chatId = 1;
            var accountId = 1;
            var participant = new ChatParticipant { ChatId = chatId, AccountId = accountId };

            //Arrange 2
            _chatRepoMock.GetParticipantAsync(chatId, accountId).Returns(participant);

            //Act
            var result = await _sut.ClearChatHistoryAsync(chatId, accountId);

            //Assert
            result.Should().BeTrue();
            participant.ClearedAt.Should().NotBeNull();
            participant.UnreadCount.Should().Be(0);

            await _chatRepoMock.Received(1).UpdateParticipantAsync(participant);
            await _chatRepoMock.Received(1).SaveChangesAsync();
            await _redisMock.Received(1).ClearUnreadCountAsync(accountId, chatId);
        }

        // MarkChatAsReadAsync
        [Fact]
        public async Task MarkChatAsReadAsync_ValidInputs_MarksReadAndReturnsTrue()
        {
            //Arrange 1
            var chatId = 1;
            var accountId = 1;

            //Arrange 2

            //Act
            var result = await _sut.MarkChatAsReadAsync(chatId, accountId);

            //Assert
            result.Should().BeTrue();
            await _chatRepoMock.Received(1).MarkAsReadAsync(chatId, accountId);
            await _chatRepoMock.Received(1).SaveChangesAsync();
            await _redisMock.Received(1).ClearUnreadCountAsync(accountId, chatId);
        }

        // SubmitReportAsync
        [Fact]
        public async Task SubmitReportAsync_ValidInputs_SubmitsReportAndReturnsTrue()
        {
            //Arrange 1
            var reporterId = 1;
            var chatId = 1;
            var reason = "Spam";
            var description = "Spamming messages";

            //Arrange 2

            //Act
            var result = await _sut.SubmitReportAsync(reporterId, chatId, reason, description);

            //Assert
            result.Should().BeTrue();
            await _chatRepoMock.Received(1).AddReportAsync(Arg.Is<UserReport>(r => r.ReporterId == reporterId && r.ChatId == chatId && r.Reason == reason));
            await _chatRepoMock.Received(1).SaveChangesAsync();
        }

        // GetTotalUnreadCountAsync
        [Fact]
        public async Task GetTotalUnreadCountAsync_ValidAccountId_ReturnsCount()
        {
            //Arrange 1
            var accountId = 1;
            
            //Arrange 2
            _chatRepoMock.GetTotalUnreadCountAsync(accountId).Returns(5);

            //Act
            var result = await _sut.GetTotalUnreadCountAsync(accountId);

            //Assert
            result.Should().Be(5);
            await _chatRepoMock.Received(1).GetTotalUnreadCountAsync(accountId);
        }

        // LogActionAsync
        [Fact]
        public async Task LogActionAsync_ValidInputs_LogsAction()
        {
            //Arrange 1
            var actorId = 1;
            var action = "Delete";
            var targetType = "Message";
            int targetId = 10;
            var details = "Deleted a message";

            //Arrange 2

            //Act
            await _sut.LogActionAsync(actorId, action, targetType, targetId, details);

            //Assert
            await _chatRepoMock.Received(1).AddAuditLogAsync(Arg.Is<AuditLog>(l => l.ActorId == actorId && l.ActionType == action));
            await _chatRepoMock.Received(1).SaveChangesAsync();
        }

        // GetSupportAccountAsync
        [Fact]
        public async Task GetSupportAccountAsync_StaffNotFound_ReturnsNull()
        {
            //Arrange 1
            
            //Arrange 2
            _userRepoMock.GetStaffAccountIdAsync().Returns((int?)null);

            //Act
            var result = await _sut.GetSupportAccountAsync();

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSupportAccountAsync_StaffFound_ReturnsAccountDto()
        {
            //Arrange 1
            var staffId = 2;
            var staffAccount = new Account { User = new User { FullName = "Support Staff" } };

            //Arrange 2
            _userRepoMock.GetStaffAccountIdAsync().Returns(staffId);
            _userRepoMock.GetAccountByIdAsync(staffId).Returns(staffAccount);

            //Act
            var result = await _sut.GetSupportAccountAsync();

            //Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("Support Staff");
        }

        // GetAdminAccountAsync
        [Fact]
        public async Task GetAdminAccountAsync_AdminNotFound_ReturnsNull()
        {
            //Arrange 1

            //Arrange 2
            _userRepoMock.GetAdminIdAsync().Returns((int?)null);

            //Act
            var result = await _sut.GetAdminAccountAsync();

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAdminAccountAsync_AdminFound_ReturnsAccountDto()
        {
            //Arrange 1
            var adminId = 1;
            var adminAccount = new Account { Manager = new Manager { DisplayName = "Admin User" } };

            //Arrange 2
            _userRepoMock.GetAdminIdAsync().Returns(adminId);
            _userRepoMock.GetAccountByIdAsync(adminId).Returns(adminAccount);

            //Act
            var result = await _sut.GetAdminAccountAsync();

            //Assert
            result.Should().NotBeNull();
            result!.FullName.Should().Be("Admin User");
        }

        // CreateSupportRequestAsync
        [Fact]
        public async Task CreateSupportRequestAsync_SenderNotFound_ThrowsInvalidOperationException()
        {
            //Arrange 1
            var senderId = 1;
            var dto = new SupportRequestDto { Content = "Help" };

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(senderId).Returns((Account?)null);

            //Act
            Func<Task> act = async () => await _sut.CreateSupportRequestAsync(senderId, dto);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Sender not found");
        }

        [Fact]
        public async Task CreateSupportRequestAsync_ValidSender_CreatesTicketAndSavesToRedis()
        {
            //Arrange 1
            var senderId = 1;
            var dto = new SupportRequestDto { Content = "Help", TargetRole = "admin" };
            var account = new Account { Username = "TestUser" };
            var activeTickets = new List<SupportTicketDto>();

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(senderId).Returns(account);
            _redisMock.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets").Returns(activeTickets);

            //Act
            var result = await _sut.CreateSupportRequestAsync(senderId, dto);

            //Assert
            result.SenderName.Should().Be("TestUser");
            result.InitialMessage.Should().Be("Help");
            await _redisMock.Received(1).SetCacheAsync("ActiveSupportTickets", Arg.Is<List<SupportTicketDto>>(l => l.Count == 1), Arg.Any<TimeSpan>());
        }

        // AcceptSupportRequestAsync
        [Fact]
        public async Task AcceptSupportRequestAsync_TicketNotFound_ThrowsInvalidOperationException()
        {
            //Arrange 1
            var acceptorId = 1;
            var ticketId = "123";
            var activeTickets = new List<SupportTicketDto>();

            //Arrange 2
            _redisMock.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets").Returns(activeTickets);

            //Act
            Func<Task> act = async () => await _sut.AcceptSupportRequestAsync(acceptorId, ticketId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This support request has already been accepted or expired.");
        }

        [Fact]
        public async Task AcceptSupportRequestAsync_TicketTargetAdminButAcceptorNotAdmin_ThrowsUnauthorizedAccessException()
        {
            //Arrange 1
            var acceptorId = 1;
            var ticketId = "123";
            var activeTickets = new List<SupportTicketDto> { new SupportTicketDto { TicketId = ticketId, TargetRole = "admin" } };

            //Arrange 2
            _redisMock.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets").Returns(activeTickets);
            _userRepoMock.GetRoleByAccountIdAsync(acceptorId).Returns("staff");

            //Act
            Func<Task> act = async () => await _sut.AcceptSupportRequestAsync(acceptorId, ticketId);

            //Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Only admins can accept this request.");
        }

        [Fact]
        public async Task AcceptSupportRequestAsync_TicketTargetStaffButAcceptorNotStaffOrAdmin_ThrowsUnauthorizedAccessException()
        {
            //Arrange 1
            var acceptorId = 1;
            var ticketId = "123";
            var activeTickets = new List<SupportTicketDto> { new SupportTicketDto { TicketId = ticketId, TargetRole = "staff" } };

            //Arrange 2
            _redisMock.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets").Returns(activeTickets);
            _userRepoMock.GetRoleByAccountIdAsync(acceptorId).Returns("user");

            //Act
            Func<Task> act = async () => await _sut.AcceptSupportRequestAsync(acceptorId, ticketId);

            //Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Only staff or admins can accept this request.");
        }

        [Fact]
        public async Task AcceptSupportRequestAsync_ValidAcceptor_AcceptsTicketAndCreatesChat()
        {
            //Arrange 1
            var acceptorId = 1; // admin
            var ticketId = "123";
            var ticket = new SupportTicketDto { TicketId = ticketId, TargetRole = "admin", SenderId = 2, InitialMessage = "Need help" };
            var activeTickets = new List<SupportTicketDto> { ticket };
            
            var existingChatParticipant = new ChatParticipant { ChatId = 10 }; // To avoid testing GetOrCreateChat full logic here
            var senderAccount = new Account { AccountId = 2, User = new User { FullName = "User" } };

            //Arrange 2
            _redisMock.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets").Returns(activeTickets);
            _userRepoMock.GetRoleByAccountIdAsync(acceptorId).Returns("admin");
            
            // Mock for GetOrCreateChatAsync
            _userRepoMock.GetRoleByAccountIdAsync(2).Returns("user");
            _chatRepoMock.FindPrivateChatAsync(2, acceptorId, "system", null).Returns(existingChatParticipant);
            
            // Mock for SaveMessageAsync
            _chatRepoMock.IsParticipantAsync(10, 2).Returns(true);
            _chatRepoMock.GetChatByIdAsync(10).Returns(new Chat { ChatId = 10 });
            _chatRepoMock.GetParticipantIdsAsync(10).Returns(new List<int> { 1, 2 });
            _userRepoMock.GetAccountByIdAsync(2).Returns(senderAccount);

            //Act
            var result = await _sut.AcceptSupportRequestAsync(acceptorId, ticketId);

            //Assert
            result.Should().Be(10);
            await _redisMock.Received(1).SetCacheAsync("ActiveSupportTickets", Arg.Is<List<SupportTicketDto>>(l => l.Count == 0), Arg.Any<TimeSpan>());
            await _chatRepoMock.Received(1).AddMessageAsync(Arg.Is<Message>(m => m.Content == "Need help" && m.ChatId == 10));
        }

        // GetPendingRequestsAsync
        [Fact]
        public async Task GetPendingRequestsAsync_OldTickets_RemovesOldTickets()
        {
            //Arrange 1
            var accountId = 1;
            var currentRole = "admin";
            var tickets = new List<SupportTicketDto>
            {
                new SupportTicketDto { RequestedAt = DateTime.UtcNow.AddHours(-25) }, // Old
                new SupportTicketDto { RequestedAt = DateTime.UtcNow, TargetRole = "admin" } // New
            };

            //Arrange 2
            _redisMock.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets").Returns(tickets);

            //Act
            var result = await _sut.GetPendingRequestsAsync(accountId, currentRole);

            //Assert
            result.Should().HaveCount(1);
            await _redisMock.Received(1).SetCacheAsync("ActiveSupportTickets", Arg.Is<List<SupportTicketDto>>(l => l.Count == 1), Arg.Any<TimeSpan>());
        }

        [Fact]
        public async Task GetPendingRequestsAsync_RoleIsAdmin_ReturnsAdminTickets()
        {
            //Arrange 1
            var accountId = 1;
            var currentRole = "admin";
            var tickets = new List<SupportTicketDto>
            {
                new SupportTicketDto { RequestedAt = DateTime.UtcNow, TargetRole = "admin" },
                new SupportTicketDto { RequestedAt = DateTime.UtcNow, TargetRole = "staff" }
            };

            //Arrange 2
            _redisMock.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets").Returns(tickets);

            //Act
            var result = await _sut.GetPendingRequestsAsync(accountId, currentRole);

            //Assert
            result.Should().HaveCount(1);
            result.First().TargetRole.Should().Be("admin");
        }

        [Fact]
        public async Task GetPendingRequestsAsync_RoleIsStaff_ReturnsStaffTickets()
        {
            //Arrange 1
            var accountId = 1;
            var currentRole = "staff";
            var tickets = new List<SupportTicketDto>
            {
                new SupportTicketDto { RequestedAt = DateTime.UtcNow, TargetRole = "admin" },
                new SupportTicketDto { RequestedAt = DateTime.UtcNow, TargetRole = "staff" }
            };

            //Arrange 2
            _redisMock.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets").Returns(tickets);

            //Act
            var result = await _sut.GetPendingRequestsAsync(accountId, currentRole);

            //Assert
            result.Should().HaveCount(1);
            result.First().TargetRole.Should().Be("staff");
        }

        [Fact]
        public async Task GetPendingRequestsAsync_RoleIsUser_ReturnsUserTickets()
        {
            //Arrange 1
            var accountId = 1;
            var currentRole = "user";
            var tickets = new List<SupportTicketDto>
            {
                new SupportTicketDto { RequestedAt = DateTime.UtcNow, TargetRole = "admin", SenderId = 1 },
                new SupportTicketDto { RequestedAt = DateTime.UtcNow, TargetRole = "staff", SenderId = 2 }
            };

            //Arrange 2
            _redisMock.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets").Returns(tickets);

            //Act
            var result = await _sut.GetPendingRequestsAsync(accountId, currentRole);

            //Assert
            result.Should().HaveCount(1);
            result.First().SenderId.Should().Be(1);
        }
        [Fact]
        public async Task GetMyChatsAsync_PartnerIsNull_ReturnsSupportTeam()
        {
            var accountId = 1;
            var chat = new Chat
            {
                ChatId = 1,
                Messages = new List<Message> { new Message { Content = null } },
                ChatParticipants = new List<ChatParticipant>()
            };
            var participants = new List<ChatParticipant>
            {
                new ChatParticipant { ChatId = 1, AccountId = accountId, Chat = chat }
            };

            _chatRepoMock.GetParticipantsByAccountIdAsync(accountId).Returns(participants);
            _redisMock.GetUnreadCountAsync(accountId, 1).Returns(1);
            
            var result = await _sut.GetMyChatsAsync(accountId);

            result.Should().HaveCount(1);
            result.First().PartnerName.Should().Be("Support Team");
            result.First().LastMessage.Should().BeNull();
        }

        [Fact]
        public async Task GetMyChatsAsync_PartnerHasManagerOnly_ReturnsManagerName()
        {
            var accountId = 1;
            var chat = new Chat
            {
                ChatId = 1,
                Messages = new List<Message> { new Message { Content = "Hello" } },
                ChatParticipants = new List<ChatParticipant>
                {
                    new ChatParticipant { AccountId = 2, Account = new Account { Manager = new Manager { DisplayName = "Mgr" } } }
                }
            };
            var participants = new List<ChatParticipant> { new ChatParticipant { ChatId = 1, AccountId = accountId, Chat = chat } };

            _chatRepoMock.GetParticipantsByAccountIdAsync(accountId).Returns(participants);
            
            var result = await _sut.GetMyChatsAsync(accountId);
            result.First().PartnerName.Should().Be("Mgr");
        }

        [Fact]
        public async Task GetChatHistoryAsync_ParticipantNull_ReturnsMessagesUnfiltered()
        {
            var chatId = 1;
            var accountId = 1;
            
            _chatRepoMock.IsParticipantAsync(chatId, accountId).Returns(true);
            _chatRepoMock.GetParticipantAsync(chatId, accountId).Returns((ChatParticipant?)null);
            _chatRepoMock.GetMessagesByChatIdAsync(chatId).Returns(new List<Message> { new Message { MessageId = 1, Attachments = new List<MessageAttachment>() } });

            var result = await _sut.GetChatHistoryAsync(chatId, accountId);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetOrCreateChatAsync_UserToInstructor_NotEnrolled_ThrowsException()
        {
            var senderId = 1;
            var dto = new CreateChatDto { TargetAccountId = 2, ContextType = "course", ContextId = null };
            
            _userRepoMock.GetRoleByAccountIdAsync(senderId).Returns("user");
            _userRepoMock.GetRoleByAccountIdAsync(dto.TargetAccountId).Returns("instructor");
            
            Func<Task> act = async () => await _sut.GetOrCreateChatAsync(senderId, dto);
            
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task AcceptSupportRequestAsync_TicketNotFound_ThrowsException()
        {
            var acceptorId = 1;
            var ticketId = "123";
            
            _redisMock.GetCacheAsync<List<SupportTicketDto>>("ActiveSupportTickets").Returns((List<SupportTicketDto>?)null);
            
            Func<Task> act = async () => await _sut.AcceptSupportRequestAsync(acceptorId, ticketId);
            
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task GetSupportAccountAsync_NotEnoughParticipants_ReturnsNull()
        {
            var accountId = 1;
            var chat = new Chat { ChatParticipants = new List<ChatParticipant> { new ChatParticipant { AccountId = accountId } } };
            
            _chatRepoMock.GetChatByIdAsync(1).Returns(chat);
            
            var result = await _sut.GetSupportAccountAsync();
            
            result.Should().BeNull();
        }
        [Fact]
        public async Task GetMyChatsAsync_PartnerHasUsernameOnly_ReturnsUsername()
        {
            var accountId = 1;
            var chat = new Chat
            {
                ChatId = 1,
                Messages = new List<Message> { new Message { Content = "Hello" } },
                ChatParticipants = new List<ChatParticipant>
                {
                    new ChatParticipant { AccountId = 2, Account = new Account { Username = "user123" } }
                }
            };
            var participants = new List<ChatParticipant> { new ChatParticipant { ChatId = 1, AccountId = accountId, Chat = chat } };

            _chatRepoMock.GetParticipantsByAccountIdAsync(accountId).Returns(participants);
            
            var result = await _sut.GetMyChatsAsync(accountId);
            result.First().PartnerName.Should().Be("user123");
        }

        [Fact]
        public async Task GetMyChatsAsync_PartnerHasEmailOnly_ReturnsEmail()
        {
            var accountId = 1;
            var chat = new Chat
            {
                ChatId = 1,
                Messages = new List<Message> { new Message { Content = "Hello" } },
                ChatParticipants = new List<ChatParticipant>
                {
                    new ChatParticipant { AccountId = 2, Account = new Account { Email = "test@example.com" } }
                }
            };
            var participants = new List<ChatParticipant> { new ChatParticipant { ChatId = 1, AccountId = accountId, Chat = chat } };

            _chatRepoMock.GetParticipantsByAccountIdAsync(accountId).Returns(participants);
            
            var result = await _sut.GetMyChatsAsync(accountId);
            result.First().PartnerName.Should().Be("test@example.com");
        }

        [Fact]
        public async Task GetChatHistoryAsync_SenderHasManagerOnly_ReturnsManagerName()
        {
            var chatId = 1;
            var accountId = 1;
            var messages = new List<Message>
            {
                new Message { MessageId = 1, Sender = new Account { Manager = new Manager { DisplayName = "MgrName" } } }
            };

            _chatRepoMock.IsParticipantAsync(chatId, accountId).Returns(true);
            _chatRepoMock.GetParticipantAsync(chatId, accountId).Returns(new ChatParticipant { ClearedAt = null });
            _chatRepoMock.GetMessagesByChatIdAsync(chatId).Returns(messages);

            var result = await _sut.GetChatHistoryAsync(chatId, accountId);
            result.First().SenderName.Should().Be("MgrName");
        }

        [Fact]
        public async Task GetChatHistoryAsync_SenderIsNull_ReturnsUnknown()
        {
            var chatId = 1;
            var accountId = 1;
            var messages = new List<Message>
            {
                new Message { MessageId = 1, Sender = null }
            };

            _chatRepoMock.IsParticipantAsync(chatId, accountId).Returns(true);
            _chatRepoMock.GetParticipantAsync(chatId, accountId).Returns(new ChatParticipant { ClearedAt = null });
            _chatRepoMock.GetMessagesByChatIdAsync(chatId).Returns(messages);

            var result = await _sut.GetChatHistoryAsync(chatId, accountId);
            result.First().SenderName.Should().Be("Unknown");
        }

        [Fact]
        public async Task SaveMessageAsync_SenderHasEmailOnly_ReturnsEmail()
        {
            var senderId = 1;
            var dto = new SendMessageDto { ChatId = 1, Content = "Hi" };
            var account = new Account { AccountId = senderId, Email = "senderUser@test.com" };
            var chat = new Chat { ChatId = 1, ChatParticipants = new List<ChatParticipant> { new ChatParticipant { AccountId = senderId } } };

            _userRepoMock.GetAccountByIdAsync(senderId).Returns(account);
            _chatRepoMock.GetChatByIdAsync(dto.ChatId).Returns(chat);
            _chatRepoMock.IsParticipantAsync(dto.ChatId, senderId).Returns(true);
            _chatRepoMock.AddMessageAsync(Arg.Any<Message>()).Returns(Task.CompletedTask);
            _chatRepoMock.GetParticipantIdsAsync(dto.ChatId).Returns(new List<int>());

            var result = await _sut.SaveMessageAsync(senderId, dto);
            result.SenderName.Should().Be("senderUser@test.com");
        }
    }
}
