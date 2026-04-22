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
                .Include(n => n.Receiver) // Kết nối bảng Account
                    .ThenInclude(a => a.User) // Kết nối bảng User từ Account
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<Notification?> GetByIdAsync(int id) =>
            await _context.Notifications.FindAsync(id);

        public void Delete(Notification notification) =>
            _context.Notifications.Remove(notification);

        public async Task AddAsync(Notification notification) =>
            await _context.Notifications.AddAsync(notification);
        public async Task AddRangeAsync(IEnumerable<Notification> notifications) =>
            await _context.Notifications.AddRangeAsync(notifications);

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.ReceiverId == userId);

            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> SearchEmailsByQueryAsync(string query, int take = 5) =>
            await _context.Accounts
                .Where(a => a.Email.Contains(query))
                .Select(a => a.Email)
                .Take(take)
                .ToListAsync();

        public async Task<List<int>> GetAllUserIdsAsync() =>
            await _context.Users
                .Select(u => u.UserId)
                .ToListAsync();

        public async Task<int?> GetUserIdByEmailAsync(string email)
        {
            var account = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Email == email.Trim());

            return account?.User?.UserId;
        }
    }
}
