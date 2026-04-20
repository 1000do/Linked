using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface INotificationService
    {
        Task<List<Notification>> GetNotificationsForUserAsync(int userId);
        // Hàm này quan trọng: Vừa lưu DB, vừa bắn SignalR
        Task SendNotificationAsync(int receiverId, string title, string content, string? linkAction);
        Task<bool> DeleteNotificationAsync(int notiId, int userId);
        Task<List<Notification>> GetAllNotificationsAsync();
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<List<string>> SearchEmailsAsync(string query);
        /// <summary>Gửi thông báo nâng cao (ALL hoặc danh sách email). Trả -1 nếu dữ liệu không hợp lệ.</summary>
        Task<int> SendAdvancedAsync(NotificationAdvancedDto dto);
    }
}
