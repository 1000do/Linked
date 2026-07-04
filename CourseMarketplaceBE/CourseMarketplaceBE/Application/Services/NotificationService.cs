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

                await DispatchSingleNotificationAsync(noti);
                await NotifyRemainingManagersAsync(new[] { receiverId });
                
                return true;
            }
            catch (NotificationException ex)
            {
                throw new BadRequestException(ex.Message);
            }
        }

        public async Task<bool> DeleteNotificationAsync(int notiId, int userId)
        {
            var noti = await GetNotificationAndVerifyOwnershipAsync(notiId, userId);

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
            var noti = await GetNotificationAndVerifyOwnershipAsync(notificationId, userId);

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

        private async Task<Notification> GetNotificationAndVerifyOwnershipAsync(int notificationId, int userId)
        {
            var noti = await _repo.GetByIdAsync(notificationId);
            if (noti == null || noti.ReceiverId != userId)
                throw new KeyNotFoundException("Notification not found.");
            return noti;
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

            var dtos = MapToAdminResponseDtos(notifications);
            return new PagedResult<NotificationAdminResponseDto> { Items = dtos, TotalCount = count };
        }

        private List<NotificationAdminResponseDto> MapToAdminResponseDtos(IEnumerable<Notification> notifications)
        {
            return notifications.Select(n => new NotificationAdminResponseDto
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Content = n.Content,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReceiverEmail = n.Receiver?.Email ?? "all-users@system.com",
                ReceiverFullName = n.Receiver?.User?.FullName ?? "System",
                ReceiverRole = GetAccountRole(n.Receiver, "broadcast"),
                ReceiverId = n.ReceiverId,
                SenderId = n.SenderId,
                SenderRole = GetAccountRole(n.Sender, "system"),
                LinkAction = n.LinkAction
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
            var targetUserIds = await ResolveTargetUserIdsAsync(dto, senderId, senderRole);
            if (!targetUserIds.Any()) return 0;

            var now = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(7), DateTimeKind.Unspecified);
            var notifications = CreateAdvancedNotifications(targetUserIds, dto, senderId, now);

            await _repo.AddRangeAsync(notifications);
            int numberOfRowsAffected = await _repo.SaveChangesAsync();
            /* zero rows exception removed */

            await DispatchAdvancedNotificationsAsync(targetUserIds, dto, senderId, senderRole, now);
            await NotifyRemainingManagersAsync(targetUserIds);

            return targetUserIds.Count;
        }

        public async Task<bool> SendBulkNotificationsAsync(IEnumerable<NotificationBulkDto> dtos)
        {
            if (dtos == null || !dtos.Any()) return true;

            var now = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(7), DateTimeKind.Unspecified);
            var notifications = CreateBulkNotifications(dtos, now);

            await _repo.AddRangeAsync(notifications);
            int numberOfRowsAffected = await _repo.SaveChangesAsync();
            /* zero rows exception removed */

            await DispatchBulkNotificationsAsync(notifications);
            await NotifyRemainingManagersAsync(ExtractReceiverIds(notifications));

            return true;
        }

        private List<Notification> CreateBulkNotifications(IEnumerable<NotificationBulkDto> dtos, DateTime now)
        {
            return dtos.Select(dto => new Notification
            {
                ReceiverId = dto.ReceiverId,
                Title = dto.Title,
                Content = dto.Content,
                LinkAction = dto.LinkAction,
                IsRead = false,
                CreatedAt = now
            }).ToList();
        }

        private async Task DispatchBulkNotificationsAsync(IEnumerable<Notification> notifications)
        {
            foreach (var noti in notifications)
            {
                await DispatchSingleNotificationAsync(noti);
            }
        }

        private IEnumerable<int> ExtractReceiverIds(IEnumerable<Notification> notifications)
        {
            return notifications.Where(n => n.ReceiverId.HasValue).Select(n => n.ReceiverId.Value);
        }


        private string GetAccountRole(Account? account, string defaultRole)
        {
            if (account == null) return defaultRole;
            if (account.Manager != null) return account.Manager.Role?.ToLower() ?? "staff";
            if (account.User != null) return account.User.Instructor != null ? "instructor" : "student";
            return "student";
        }

        private async Task<List<int>> ResolveTargetUserIdsAsync(NotificationAdvancedDto dto, int senderId, string senderRole)
        {
            if (dto.TargetType == "ALL")
            {
                return await GetTargetUserIdsForAllTypeAsync(senderId, senderRole);
            }
            
            if (dto.Emails != null && dto.Emails.Any())
            {
                return await GetTargetUserIdsForEmailsAsync(dto.Emails, senderId, senderRole);
            }

            return new List<int>();
        }

        private async Task<List<int>> GetTargetUserIdsForAllTypeAsync(int senderId, string senderRole)
        {
            List<int> targetUserIds;

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
            return targetUserIds;
        }

        private async Task<List<int>> GetTargetUserIdsForEmailsAsync(IEnumerable<string> emails, int senderId, string senderRole)
        {
            var targetUserIds = new List<int>();
            
            foreach (var email in emails)
            {
                var acc = await _userRepo.GetAccountByEmailAsync(email);
                if (acc != null)
                {
                    var recipientRole = await _userRepo.GetRoleByAccountIdAsync(acc.AccountId);
                    ValidateRecipient(acc.AccountId, senderId, senderRole, recipientRole);
                    targetUserIds.Add(acc.AccountId);
                }
            }

            return targetUserIds;
        }

        private void ValidateRecipient(int recipientId, int senderId, string senderRole, string recipientRole)
        {
            if (recipientId == senderId)
            {
                throw new InvalidOperationException("You cannot send a notification to yourself.");
            }
            
            ValidateRecipientPermission(senderRole, recipientRole);
        }

        private void ValidateRecipientPermission(string senderRole, string recipientRole)
        {
            if (senderRole == "staff" && (recipientRole == "staff" || recipientRole == "admin"))
            {
                throw new InvalidOperationException("Staff cannot send notifications to staff or admin.");
            }
            if (senderRole == "admin" && recipientRole == "admin")
            {
                throw new InvalidOperationException("Admin cannot send notifications to other admins.");
            }
        }

        private List<Notification> CreateAdvancedNotifications(List<int> targetUserIds, NotificationAdvancedDto dto, int senderId, DateTime now)
        {
            return targetUserIds.Select(uid => new Notification
            {
                SenderId = senderId,
                ReceiverId = uid,
                Title = dto.Title,
                Content = dto.Content,
                LinkAction = dto.LinkAction,
                CreatedAt = now,
                IsRead = false
            }).ToList();
        }

        private async Task DispatchAdvancedNotificationsAsync(List<int> targetUserIds, NotificationAdvancedDto dto, int senderId, string senderRole, DateTime now)
        {
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
        }

        private async Task DispatchSingleNotificationAsync(Notification noti)
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

        private async Task NotifyRemainingManagersAsync(IEnumerable<int> explicitlyNotifiedUserIds)
        {
            var allManagerIds = await _userRepo.GetAllManagerIdsAsync();
            var explicitlyNotified = explicitlyNotifiedUserIds.ToHashSet();
            var remainingManagerIds = allManagerIds
                .Where(id => !explicitlyNotified.Contains(id))
                .Select(id => id.ToString())
                .ToList();

            if (remainingManagerIds.Any())
            {
                await _hubContext.Clients.Users(remainingManagerIds).SendAsync("ReceiveNotification");
            }
        }
    }
}
