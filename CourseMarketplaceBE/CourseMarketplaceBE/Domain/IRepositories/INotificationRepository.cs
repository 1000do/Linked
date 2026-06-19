using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface INotificationRepository
    {
        Task<(List<Notification> Items, int TotalCount)> GetByReceiverIdAsync(int userId, int page = 1, int pageSize = int.MaxValue);
        Task<Notification?> GetByIdAsync(int id);

        // THÊM DÒNG NÀY VÀO
        Task AddAsync(Notification notification);
        Task AddRangeAsync(IEnumerable<Notification> notifications);
        void Delete(Notification notification);
        void Update(Notification notification);
        void UpdateRange(IEnumerable<Notification> notifications);
        Task<int> SaveChangesAsync();
        Task<(List<Notification> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = int.MaxValue);
        Task<List<Notification>> GetUnreadByReceiverIdAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<int> AutoCleanupAdminNotificationsAsync();
        Task<int> GetSentNotificationsCountAsync(int senderId);
    }
}
