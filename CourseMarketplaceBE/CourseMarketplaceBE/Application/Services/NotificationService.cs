using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;
using AutoMapper;

namespace CourseMarketplaceBE.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IMapper _mapper;

        public NotificationService(
            INotificationRepository repo,
            IUserRepository userRepo,
            IHubContext<NotificationHub> hubContext,
            IMapper mapper)
        {
            _repo = repo;
            _userRepo = userRepo;
            _hubContext = hubContext;
            _mapper = mapper;
        }

        public async Task<PagedResult<NotificationResponseDto>> GetNotificationsForUserAsync(int userId, int page = 1, int pageSize = int.MaxValue)
        {
            var (entities, count) = await _repo.GetByReceiverIdAsync(userId, page, pageSize);
            if (!entities.Any()) throw new KeyNotFoundException("No notifications found.");
            var dtos = _mapper.Map<List<NotificationResponseDto>>(entities);
            return new PagedResult<NotificationResponseDto> { Items = dtos, TotalCount = count };
        }

        public async Task<PagedResult<NotificationResponseDto>> GetAllNotificationsAsync(int page = 1, int pageSize = int.MaxValue)
        {
            var (entities, count) = await _repo.GetAllAsync(page, pageSize);
            if (!entities.Any()) throw new KeyNotFoundException("No notifications found.");
            var dtos = _mapper.Map<List<NotificationResponseDto>>(entities);
            return new PagedResult<NotificationResponseDto> { Items = dtos, TotalCount = count };
        }

        public async Task<bool> SendNotificationAsync(int receiverId, string title, string content, string? linkAction)
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
            try
            {
                int numberOfRowsAffected = await _repo.SaveChangesAsync();
                /* zero rows exception removed */

                await _hubContext.Clients.User(receiverId.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        notificationId = noti.NotificationId,
                        title = noti.Title,
                        content = noti.Content,
                        linkAction = noti.LinkAction,
                        createdAt = noti.CreatedAt,
                        isRead = false,
                        receiverId = noti.ReceiverId
                    });
                
                var allManagerIds = await _userRepo.GetAllManagerIdsAsync();
                var remainingManagerIds = allManagerIds
                    .Where(id => id != receiverId)
                    .Select(id => id.ToString())
                    .ToList();

                if (remainingManagerIds.Any())
                {
                    await _hubContext.Clients.Users(remainingManagerIds).SendAsync("ReceiveNotification");
                }
                
                return true;
            }
            catch (NotificationException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        public async Task<bool> DeleteNotificationAsync(int notiId, int userId)
        {
            var noti = await _repo.GetByIdAsync(notiId);
            if (noti == null || noti.ReceiverId != userId)
                throw new KeyNotFoundException("Notification not found.");

            _repo.Delete(noti);
            try
            {
                int n = await _repo.SaveChangesAsync();
                /* zero rows exception removed */

                await _hubContext.Clients.All.SendAsync("ReceiveNotification");
                return true;
            }
            catch (NotificationException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var noti = await _repo.GetByIdAsync(notificationId);
            if (noti == null || noti.ReceiverId != userId)
                throw new KeyNotFoundException("Notification not found.");

            noti.IsRead = true;
            _repo.Update(noti);

            try
            {
                int n = await _repo.SaveChangesAsync();
                /* zero rows exception removed */

                await _hubContext.Clients.All.SendAsync("ReceiveNotification");
                return true;
            }
            catch (NotificationException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var unreads = await _repo.GetUnreadByReceiverIdAsync(userId);
            if (!unreads.Any()) return true;

            foreach (var noti in unreads)
            {
                noti.IsRead = true;
            }
            _repo.UpdateRange(unreads);

            try
            {
                int n = await _repo.SaveChangesAsync();
                /* zero rows exception removed */
                await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", null);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification");
                return true;
            }
            catch (NotificationException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        public async Task<PagedResult<NotificationAdminResponseDto>> GetAllNotificationsForAdminAsync(int page = 1, int pageSize = int.MaxValue)
        {
            await _repo.AutoCleanupAdminNotificationsAsync();

            var (notifications, count) = await _repo.GetAllAsync(page, pageSize);
            if (!notifications.Any()) throw new KeyNotFoundException("No notifications found.");

            var dtos = notifications.Select(n => {
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
                    SenderRole = senderRole,
                    LinkAction = n.LinkAction
                };
            }).ToList();
            return new PagedResult<NotificationAdminResponseDto> { Items = dtos, TotalCount = count };
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
                LinkAction = dto.LinkAction,
                CreatedAt = now,
                IsRead = false
            }).ToList();

            await _repo.AddRangeAsync(notifications);
            int numberOfRowsAffected = await _repo.SaveChangesAsync();
            /* zero rows exception removed */

            foreach (var uid in targetUserIds)
            {
                await _hubContext.Clients.User(uid.ToString()).SendAsync("ReceiveNotification", new
                {
                    notificationId = 0,
                    title = dto.Title,
                    content = dto.Content,
                    linkAction = dto.LinkAction,
                    createdAt = now,
                    isRead = false,
                    receiverId = uid,
                    senderId = senderId,
                    senderRole = senderRole
                });
            }

            var allManagerIds = await _userRepo.GetAllManagerIdsAsync();
            var explicitlyNotified = targetUserIds.ToHashSet();
            var remainingManagerIds = allManagerIds
                .Where(id => !explicitlyNotified.Contains(id))
                .Select(id => id.ToString())
                .ToList();

            if (remainingManagerIds.Any())
            {
                await _hubContext.Clients.Users(remainingManagerIds).SendAsync("ReceiveNotification");
            }

            return targetUserIds.Count;
        }

        public async Task<bool> SendBulkNotificationsAsync(IEnumerable<NotificationBulkDto> dtos)
        {
            if (dtos == null || !dtos.Any()) return true;

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
            /* zero rows exception removed */

            foreach (var noti in notifications)
            {
                await _hubContext.Clients.User(noti.ReceiverId.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        notificationId = noti.NotificationId,
                        title = noti.Title,
                        content = noti.Content,
                        linkAction = noti.LinkAction,
                        createdAt = noti.CreatedAt,
                        isRead = false,
                        receiverId = noti.ReceiverId
                    });
            }

            var allManagerIds = await _userRepo.GetAllManagerIdsAsync();
            var explicitlyNotifiedIds = notifications.Select(n => n.ReceiverId).ToHashSet();
            var remainingManagerIds = allManagerIds
                .Where(id => !explicitlyNotifiedIds.Contains(id))
                .Select(id => id.ToString())
                .ToList();

            if (remainingManagerIds.Any())
            {
                await _hubContext.Clients.Users(remainingManagerIds).SendAsync("ReceiveNotification");
            }

            return true;
        }

    }
}
