using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetByReceiverIdAsync(int userId);
        Task<Notification?> GetByIdAsync(int id);

        // THÊM DÒNG NÀY VÀO
        Task AddAsync(Notification notification);

        void Delete(Notification notification);
        Task SaveChangesAsync();
        Task<List<Notification>> GetAllAsync();
    }
}
