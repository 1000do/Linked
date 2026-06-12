using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    public class ManagerRepository : IManagerRepository
    {
        private readonly AppDbContext _context;

        public ManagerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Manager?> GetManagerByIdAsync(int managerId)
        {
            return await _context.Managers
                .Include(m => m.ManagerNavigation)
                    .ThenInclude(a => a.Lockouts)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);
        }

        public async Task<bool> UpdateManagerProfileAsync(int managerId, string displayName, string? fullName, string? phoneNumber, string? avatarUrl, string? bio)
        {
            var manager = await _context.Managers
                .Include(m => m.ManagerNavigation)
                .FirstOrDefaultAsync(m => m.ManagerId == managerId);
            if (manager == null) return false;

            manager.DisplayName = displayName;
            manager.FullName = fullName;
            manager.PhoneNumber = phoneNumber;
            manager.Bio = bio;

            if (avatarUrl != null)
            {
                manager.AvatarUrl = avatarUrl;
                manager.ManagerNavigation.AvatarUrl = avatarUrl;
            }
            if (phoneNumber != null)
            {
                manager.ManagerNavigation.PhoneNumber = phoneNumber;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAccountPasswordHashAsync(int accountId, string passwordHash)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return false;

            account.PasswordHash = passwordHash;
            account.AccountUpdatedAt = System.DateTime.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
