using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    public class AvatarFrameRepository : IAvatarFrameRepository
    {
        private readonly AppDbContext _context;

        public AvatarFrameRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AvatarFrame?> GetByIdAsync(int id)
        {
            return await _context.Set<AvatarFrame>().FindAsync(id);
        }

        public async Task<IEnumerable<AvatarFrame>> GetAllActiveAsync()
        {
            return await _context.Set<AvatarFrame>()
                .Where(f => f.IsActive)
                .ToListAsync();
        }

        public async Task<bool> CreateAsync(AvatarFrame frame)
        {
            _context.Set<AvatarFrame>().Add(frame);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UserHasFrameAsync(int userId, int frameId)
        {
            return await _context.Set<UserAvatarFrame>()
                .AnyAsync(f => f.UserId == userId && f.FrameId == frameId);
        }

        public async Task<bool> AddUserFrameAsync(UserAvatarFrame userFrame)
        {
            _context.Set<UserAvatarFrame>().Add(userFrame);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<UserAvatarFrame>> GetUserFramesWithDetailsAsync(int userId)
        {
            return await _context.Set<UserAvatarFrame>()
                .Where(f => f.UserId == userId)
                .Include(f => f.Frame)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserFramesEquippedStatusAsync(int userId, int frameId)
        {
            var userFrames = await _context.Set<UserAvatarFrame>()
                .Where(f => f.UserId == userId)
                .ToListAsync();

            foreach (var f in userFrames)
            {
                f.IsEquipped = (f.FrameId == frameId);
            }

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
