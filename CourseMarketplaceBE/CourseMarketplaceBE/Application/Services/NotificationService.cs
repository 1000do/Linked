using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CourseMarketplaceBE.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            INotificationRepository repo,
            IUserRepository userRepo,
            IHubContext<NotificationHub> hubContext)
        {
            _repo = repo;
            _userRepo = userRepo;
            _hubContext = hubContext;
        }

        public async Task<List<Notification>> GetNotificationsForUserAsync(int userId)
            => await _repo.GetByReceiverIdAsync(userId);

        public async Task<List<Notification>> GetAllNotificationsAsync()
            => await _repo.GetAllAsync();

        public async Task SendNotificationAsync(int receiverId, string title, string content, string? linkAction)
        {
            var now = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(7), DateTimeKind.Unspecified);

            var noti = new Notification
            {
                ReceiverId = receiverId,
                Title = title,
                Content = content,
                LinkAction = linkAction,
                IsRead = false,
                CreatedAt = now
            };

            await _repo.AddAsync(noti);
            int numberOfRowsAffected = await _repo.SaveChangesAsync();
            if (numberOfRowsAffected > 0)
            {
                await _hubContext.Clients.User(receiverId.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        notificationId = noti.NotificationId,
                        title = noti.Title,
                        content = noti.Content,
                        createdAt = noti.CreatedAt,
                        isRead = false,
                        receiverId = noti.ReceiverId
                    });
                await _hubContext.Clients.Group("managers").SendAsync("ReceiveNotification");
            }
            else
            {
                throw new InvalidOperationException("Failed to save changes");
            }
        }

        public async Task<bool> DeleteNotificationAsync(int notiId, int userId)
        {
            var noti = await _repo.GetByIdAsync(notiId);
            if (noti == null || noti.ReceiverId != userId) return false;

            _repo.Delete(noti);
            int numberOfRowsAffected = await _repo.SaveChangesAsync();
            if (numberOfRowsAffected <= 0)
            {
                throw new InvalidOperationException("Failed to save changes");
            }

            await _hubContext.Clients.All.SendAsync("ReceiveNotification");
            return true;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var result = await _repo.MarkAsReadAsync(notificationId, userId);
            if (result)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification");
            }
            return result;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var result = await _repo.MarkAllAsReadAsync(userId);
            if (result)
            {
                await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", null);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification");
            }
            return result;
        }

        public async Task<List<NotificationAdminResponseDto>> GetAllNotificationsForAdminAsync()
        {
            await _repo.AutoCleanupAdminNotificationsAsync();

            var notifications = await _repo.GetAllAsync();

            return notifications.Select(n => {
                string role = "broadcast";
                if (n.Receiver != null)
                {
                    if (n.Receiver.Manager != null)
                    {
                        role = n.Receiver.Manager.Role?.ToLower() ?? "staff";
                    }
                    else if (n.Receiver.User != null)
                    {
                        role = n.Receiver.User.Instructor != null ? "instructor" : "student";
                    }
                    else
                    {
                        role = "student";
                    }
                }

                string senderRole = "system";
                if (n.Sender != null)
                {
                    if (n.Sender.Manager != null)
                    {
                        senderRole = n.Sender.Manager.Role?.ToLower() ?? "staff";
                    }
                    else if (n.Sender.User != null)
                    {
                        senderRole = n.Sender.User.Instructor != null ? "instructor" : "student";
                    }
                    else
                    {
                        senderRole = "student";
                    }
                }

                return new NotificationAdminResponseDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Content = n.Content,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ReceiverEmail = n.Receiver?.Email ?? "all-users@system.com",
                    ReceiverFullName = n.Receiver?.User?.FullName ?? "System",
                    ReceiverRole = role,
                    ReceiverId = n.ReceiverId,
                    SenderId = n.SenderId,
                    SenderRole = senderRole
                };
            }).ToList();
        }

        public async Task<List<string>> SearchEmailsAsync(string query, int senderId, string senderRole)
            => await _userRepo.SearchEmailsByQueryAsync(query, senderId, senderRole);

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _repo.GetUnreadCountAsync(userId);
        }

        public async Task<int> SendAdvancedAsync(NotificationAdvancedDto dto, int senderId, string senderRole)
        {
            var targetUserIds = new List<int>();

            if (dto.TargetType == "ALL")
            {
                if (senderRole == "staff")
                {
                    targetUserIds = await _userRepo.GetUserIdsForStaffSenderAsync();
                }
                else if (senderRole == "admin")
                {
                    targetUserIds = await _userRepo.GetUserIdsForAdminSenderAsync();
                }
                else
                {
                    targetUserIds = await _userRepo.GetAllUserIdsAsync();
                }

                targetUserIds.Remove(senderId);
            }
            else if (dto.Emails != null && dto.Emails.Any())
            {
                foreach (var email in dto.Emails)
                {
                    var acc = await _userRepo.GetAccountByEmailAsync(email);
                    if (acc != null)
                    {
                        if (acc.AccountId == senderId)
                        {
                            throw new InvalidOperationException("You cannot send a notification to yourself.");
                        }

                        var recipientRole = await _userRepo.GetRoleByAccountIdAsync(acc.AccountId);
                        if (senderRole == "staff")
                        {
                            if (recipientRole == "staff" || recipientRole == "admin")
                            {
                                throw new InvalidOperationException("Staff cannot send notifications to staff or admin.");
                            }
                        }
                        else if (senderRole == "admin")
                        {
                            if (recipientRole == "admin")
                            {
                                throw new InvalidOperationException("Admin cannot send notifications to other admins.");
                            }
                        }

                        targetUserIds.Add(acc.AccountId);
                    }
                }
            }

            if (!targetUserIds.Any()) return 0;

            var now = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(7), DateTimeKind.Unspecified);

            var notifications = targetUserIds.Select(uid => new Notification
            {
                SenderId = senderId,
                ReceiverId = uid,
                Title = dto.Title,
                Content = dto.Content,
                CreatedAt = now,
                IsRead = false
            }).ToList();

            await _repo.AddRangeAsync(notifications);
            int numberOfRowsAffected = await _repo.SaveChangesAsync();
            if (numberOfRowsAffected <= 0)
            {
                throw new InvalidOperationException("Failed to save changes");
            }

            foreach (var uid in targetUserIds)
            {
                await _hubContext.Clients.User(uid.ToString()).SendAsync("ReceiveNotification", new
                {
                    notificationId = 0,
                    title = dto.Title,
                    content = dto.Content,
                    createdAt = now,
                    isRead = false,
                    receiverId = uid,
                    senderId = senderId,
                    senderRole = senderRole
                });
            }

            await _hubContext.Clients.Group("managers").SendAsync("ReceiveNotification");

            return targetUserIds.Count;
        }

        public async Task SendBulkNotificationsAsync(IEnumerable<NotificationBulkDto> dtos)
        {
            if (dtos == null || !dtos.Any()) return;

            var now = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(7), DateTimeKind.Unspecified);

            var notifications = dtos.Select(dto => new Notification
            {
                ReceiverId = dto.ReceiverId,
                Title = dto.Title,
                Content = dto.Content,
                LinkAction = dto.LinkAction,
                IsRead = false,
                CreatedAt = now
            }).ToList();

            await _repo.AddRangeAsync(notifications);
            int numberOfRowsAffected = await _repo.SaveChangesAsync();
            if (numberOfRowsAffected <= 0)
            {
                throw new InvalidOperationException("Failed to save changes");
            }

            foreach (var noti in notifications)
            {
                await _hubContext.Clients.User(noti.ReceiverId.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        notificationId = noti.NotificationId,
                        title = noti.Title,
                        content = noti.Content,
                        createdAt = noti.CreatedAt,
                        isRead = false,
                        receiverId = noti.ReceiverId
                    });
            }

            await _hubContext.Clients.Group("managers").SendAsync("ReceiveNotification");
        }
    }
}
