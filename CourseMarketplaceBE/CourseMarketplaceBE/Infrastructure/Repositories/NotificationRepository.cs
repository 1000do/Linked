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

        public async Task<(List<Notification> Items, int TotalCount)> GetByReceiverIdAsync(int userId, int page = 1, int pageSize = int.MaxValue)
        {
            var query = _context.Notifications
                .Where(n => n.ReceiverId == userId)
                .OrderByDescending(n => n.CreatedAt);
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }

        public async Task<(List<Notification> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = int.MaxValue)
        {
            var query = _context.Notifications
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
                .OrderByDescending(n => n.CreatedAt);
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }

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

        public void Update(Notification notification)
        {
            _context.Notifications.Update(notification);
        }

        public void UpdateRange(IEnumerable<Notification> notifications)
        {
            _context.Notifications.UpdateRange(notifications);
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new CourseMarketplaceBE.Domain.Exceptions.NotificationException("A database error occurred while saving notifications.", ex);
            }
        }

        public async Task<List<Notification>> GetUnreadByReceiverIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.ReceiverId == userId && (n.IsRead == false || n.IsRead == null))
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.ReceiverId == userId && (n.IsRead == false || n.IsRead == null));
        }

        public async Task<int> AutoCleanupAdminNotificationsAsync()
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
                return await SaveChangesAsync();
            }
            return 0;
        }

        public async Task<int> GetSentNotificationsCountAsync(int senderId)
        {
            return await _context.Notifications.CountAsync(n => n.SenderId == senderId);
        }
    }
}
