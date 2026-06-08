using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetByReceiverIdAsync(int userId);
        Task<Notification?> GetByIdAsync(int id);

        // THÊM DÒNG NÀY VÀO
        Task AddAsync(Notification notification);
        Task AddRangeAsync(IEnumerable<Notification> notifications);
        void Delete(Notification notification);
        Task<int> SaveChangesAsync();
        Task<List<Notification>> GetAllAsync();
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task AutoCleanupAdminNotificationsAsync();
    }
}
