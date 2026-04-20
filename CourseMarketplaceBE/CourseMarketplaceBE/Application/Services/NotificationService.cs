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
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(INotificationRepository repo, IHubContext<NotificationHub> hubContext)
        {
            _repo = repo;
            _hubContext = hubContext;
        }

        public async Task<List<Notification>> GetNotificationsForUserAsync(int userId)
            => await _repo.GetByReceiverIdAsync(userId);

        public async Task<List<Notification>> GetAllNotificationsAsync()
            => await _repo.GetAllAsync();

        public async Task SendNotificationAsync(int receiverId, string title, string content, string? linkAction)
        {
            var noti = new Notification
            {
                ReceiverId = receiverId,
                Title = title,
                Content = content,
                LinkAction = linkAction,
                IsRead = false,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            await _repo.AddAsync(noti);
            await _repo.SaveChangesAsync();

            // Bắn SignalR real-time
            await _hubContext.Clients.User(receiverId.ToString())
                .SendAsync("ReceiveNotification", new { title, content, createdAt = noti.CreatedAt });
        }

        public async Task<bool> DeleteNotificationAsync(int notiId, int userId)
        {
            var noti = await _repo.GetByIdAsync(notiId);
            if (noti == null || noti.ReceiverId != userId) return false;

            _repo.Delete(noti);
            await _repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
            => await _repo.MarkAsReadAsync(notificationId, userId);

        public async Task<List<string>> SearchEmailsAsync(string query)
            => await _repo.SearchEmailsByQueryAsync(query);

        public async Task<int> SendAdvancedAsync(NotificationAdvancedDto dto)
        {
            if (dto.TargetType == "ALL")
            {
                var allUserIds = await _repo.GetAllUserIdsAsync();
                foreach (var userId in allUserIds)
                    await SendNotificationAsync(userId, dto.Title, dto.Content, null);

                return allUserIds.Count;
            }

            if (dto.Emails != null && dto.Emails.Any())
            {
                int count = 0;
                foreach (var email in dto.Emails)
                {
                    var userId = await _repo.GetUserIdByEmailAsync(email);
                    if (userId.HasValue)
                    {
                        await SendNotificationAsync(userId.Value, dto.Title, dto.Content, null);
                        count++;
                    }
                }
                return count;
            }

            return -1; // dữ liệu không hợp lệ
        }
    }
}
