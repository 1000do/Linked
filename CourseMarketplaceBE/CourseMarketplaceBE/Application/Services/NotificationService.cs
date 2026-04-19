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
        {
            return await _repo.GetByReceiverIdAsync(userId);
        }

        public async Task SendNotificationAsync(int receiverId, string title, string content, string? linkAction)
        {
            var noti = new Notification
            {
                ReceiverId = receiverId,
                Title = title,
                Content = content,
                LinkAction = linkAction,
                IsRead = false,
                // Sửa dòng này để tránh lỗi PostgreSQL
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            await _repo.AddAsync(noti);
            await _repo.SaveChangesAsync();

            // Bắn SignalR
            await _hubContext.Clients.User(receiverId.ToString())
                .SendAsync("ReceiveNotification", new { title, content, createdAt = noti.CreatedAt });
        }
        public async Task<List<Notification>> GetAllNotificationsAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<bool> DeleteNotificationAsync(int notiId, int userId)
        {
            var noti = await _repo.GetByIdAsync(notiId);
            // Kiểm tra đúng chủ sở hữu mới cho xóa
            if (noti == null || noti.ReceiverId != userId) return false;

            _repo.Delete(noti);
            await _repo.SaveChangesAsync();
            return true;
        }
    }
}
