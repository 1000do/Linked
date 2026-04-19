using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;
        public NotificationRepository(AppDbContext context) => _context = context;

        public async Task<List<Notification>> GetByReceiverIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.ReceiverId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
        public async Task<List<Notification>> GetAllAsync()
        {
            return await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id) =>
            await _context.Notifications.FindAsync(id);

        public void Delete(Notification notification) =>
            _context.Notifications.Remove(notification);
        public async Task AddAsync(Notification notification) => await _context.Notifications.AddAsync(notification);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
