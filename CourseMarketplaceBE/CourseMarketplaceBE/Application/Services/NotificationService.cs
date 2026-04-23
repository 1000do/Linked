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
            // SỬA TẠI ĐÂY: Ép kiểu DateTime về Unspecified để Npgsql không báo lỗi
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
            await _repo.SaveChangesAsync();

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
        }

        public async Task<bool> DeleteNotificationAsync(int notiId, int userId)
        {
            var noti = await _repo.GetByIdAsync(notiId);
            if (noti == null || noti.ReceiverId != userId) return false;

            _repo.Delete(noti);
            await _repo.SaveChangesAsync();

            // Bổ sung dòng này: Báo cho Admin biết để xóa dòng đó khỏi bảng lịch sử
            await _hubContext.Clients.All.SendAsync("ReceiveNotification");

            return true;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var result = await _repo.MarkAsReadAsync(notificationId, userId);
            if (result)
            {
                // Bổ sung dòng này: Gửi tín hiệu để trình duyệt Admin tự loadHistory()
                await _hubContext.Clients.All.SendAsync("ReceiveNotification");
            }
            return result;
        }
        public async Task<List<NotificationAdminResponseDto>> GetAllNotificationsForAdminAsync()
        {
            // Sử dụng Repository để lấy data đã bao gồm Include(n => n.Receiver).ThenInclude(u => u.User)
            var notifications = await _repo.GetAllAsync();

            return notifications.Select(n => new NotificationAdminResponseDto
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Content = n.Content,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                // Chống null để tránh lỗi runtime
                ReceiverEmail = n.Receiver?.Email ?? "all-users@system.com",
                ReceiverFullName = n.Receiver?.User?.FullName ?? "Hệ thống"
            }).ToList();
        }

        public async Task<List<string>> SearchEmailsAsync(string query)
            => await _repo.SearchEmailsByQueryAsync(query);

        public async Task<int> SendAdvancedAsync(NotificationAdvancedDto dto)
        {
            var targetUserIds = new List<int>();

            if (dto.TargetType == "ALL")
            {
                targetUserIds = await _repo.GetAllUserIdsAsync();
            }
            else if (dto.Emails != null && dto.Emails.Any())
            {
                foreach (var email in dto.Emails)
                {
                    var userId = await _repo.GetUserIdByEmailAsync(email);
                    if (userId.HasValue) targetUserIds.Add(userId.Value);
                }
            }

            if (!targetUserIds.Any()) return 0;

            // SỬA TẠI ĐÂY: Ép kiểu DateTime cho gửi hàng loạt
            var now = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(7), DateTimeKind.Unspecified);

            var notifications = targetUserIds.Select(uid => new Notification
            {
                ReceiverId = uid,
                Title = dto.Title,
                Content = dto.Content,
                CreatedAt = now,
                IsRead = false
            }).ToList();

            await _repo.AddRangeAsync(notifications);
            await _repo.SaveChangesAsync();

            // Trong SendAdvancedAsync và SendNotificationAsync
            foreach (var uid in targetUserIds)
            {
                await _hubContext.Clients.User(uid.ToString()).SendAsync("ReceiveNotification", new
                {
                    notificationId = 0, // Hoặc ID thật nếu bạn lưu từng cái
                    title = dto.Title,
                    content = dto.Content,
                    createdAt = now,
                    isRead = false,
                    receiverId = uid
                });
            }

            return targetUserIds.Count;
        }
    }
}