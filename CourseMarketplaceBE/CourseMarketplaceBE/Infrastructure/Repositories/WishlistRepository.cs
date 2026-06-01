using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly AppDbContext _context;

    public WishlistRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<WishlistItem>> GetByUserIdAsync(int userId)
    {
        return await _context.WishlistItems
            .Where(w => w.UserId == userId)
            .Include(w => w.Course)
            .ThenInclude(c => c!.Instructor)
            .ThenInclude(i => i!.InstructorNavigation)
            .ToListAsync();
    }

    public async Task<WishlistItem?> GetByUserAndCourseAsync(int userId, int courseId)
    {
        return await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.CourseId == courseId);
    }

    public async Task AddAsync(WishlistItem item)
    {
        await _context.WishlistItems.AddAsync(item);
    }

    public async Task RemoveAsync(WishlistItem item)
    {
        _context.WishlistItems.Remove(item);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(int userId, int courseId)
    {
        return await _context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.CourseId == courseId);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
