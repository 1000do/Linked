using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;
        public NotificationRepository(AppDbContext context) => _context = context;

        public async Task<List<Notification>> GetByReceiverIdAsync(int userId) =>
            await _context.Notifications
                .Where(n => n.ReceiverId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<List<Notification>> GetAllAsync() =>
            await _context.Notifications
                .Include(n => n.Receiver)
                    .ThenInclude(a => a.User)
                        .ThenInclude(u => u.Instructor)
                .Include(n => n.Receiver)
                    .ThenInclude(a => a.Manager)
                .Include(n => n.Sender)
                    .ThenInclude(a => a.User)
                        .ThenInclude(u => u.Instructor)
                .Include(n => n.Sender)
                    .ThenInclude(a => a.Manager)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<Notification?> GetByIdAsync(int id) =>
            await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id);

        public void Delete(Notification notification)
        {
            notification.IsRemoved = true;
            _context.Notifications.Update(notification);
        }

        public async Task AddAsync(Notification notification) =>
            await _context.Notifications.AddAsync(notification);
        public async Task AddRangeAsync(IEnumerable<Notification> notifications) =>
            await _context.Notifications.AddRangeAsync(notifications);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.ReceiverId == userId);

            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.ReceiverId == userId && (n.IsRead == false || n.IsRead == null))
                .ToListAsync();

            if (!unreadNotifications.Any()) return true;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.ReceiverId == userId && (n.IsRead == false || n.IsRead == null));
        }

        public async Task AutoCleanupAdminNotificationsAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-90);
            var oldNotifications = await _context.Notifications
                .Where(n => n.IsRead == true && n.CreatedAt < cutoffDate && n.IsRemoved != true)
                .ToListAsync();

            if (oldNotifications.Any())
            {
                foreach (var n in oldNotifications)
                {
                    n.IsRemoved = true;
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetSentNotificationsCountAsync(int senderId)
        {
            return await _context.Notifications.CountAsync(n => n.SenderId == senderId);
        }
    }
}
