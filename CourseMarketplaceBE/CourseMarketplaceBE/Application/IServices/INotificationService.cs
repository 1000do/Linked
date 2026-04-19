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
    }
}
