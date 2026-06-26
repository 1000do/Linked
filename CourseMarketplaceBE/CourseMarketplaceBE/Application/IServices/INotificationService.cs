using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface INotificationService
    {
        Task<PagedResult<NotificationResponseDto>> GetNotificationsForUserAsync(int userId, int page = 1, int pageSize = int.MaxValue);
        // Hàm này quan trọng: Vừa lưu DB, vừa bắn SignalR
        Task<PagedResult<NotificationAdminResponseDto>> GetAllNotificationsForAdminAsync(int page = 1, int pageSize = int.MaxValue);
        Task<bool> SendNotificationAsync(int receiverId, string title, string content, string? linkAction);
        Task<bool> SendBulkNotificationsAsync(IEnumerable<NotificationBulkDto> dtos);

        Task<bool> DeleteNotificationAsync(int notiId, int userId);
        Task<PagedResult<NotificationResponseDto>> GetAllNotificationsAsync(int page = 1, int pageSize = int.MaxValue);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<List<string>> SearchEmailsAsync(string query, int senderId, string senderRole);
        Task<int> GetUnreadCountAsync(int userId);
        /// <summary>Gửi thông báo nâng cao (ALL hoặc danh sách email). Trả -1 nếu dữ liệu không hợp lệ.</summary>
        Task<int> SendAdvancedAsync(NotificationAdvancedDto dto, int senderId, string senderRole);
    }
}
