using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

/// <summary>
/// Cài đặt ICartRepository ở Infrastructure layer.
/// AppDbContext được đóng gói hoàn toàn tại đây, Application không bao giờ thấy nó.
/// </summary>
public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Cart Items ────────────────────────────────────────────────────────────

    public async Task<List<CartItem>> GetCartItemsWithDetailsAsync(int userId)
        => await _context.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Course)
                .ThenInclude(course => course!.Instructor)
                    .ThenInclude(i => i!.InstructorNavigation)
            .OrderBy(c => c.AddedDate)
            .ToListAsync();

    public async Task<bool> IsCourseInCartAsync(int userId, int courseId)
        => await _context.CartItems.AnyAsync(c => c.UserId == userId && c.CourseId == courseId);

    public async Task<bool> IsCourseInAnyCartAsync(int courseId)
        => await _context.CartItems.AnyAsync(c => c.CourseId == courseId);

    public async Task AddCartItemAsync(CartItem item)
        => await _context.CartItems.AddAsync(item);

    public async Task<CartItem?> GetCartItemAsync(int userId, int courseId)
        => await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId);

    public void RemoveCartItem(CartItem item)
        => _context.CartItems.Remove(item);

    // ── Persistence ────────────────────────────────────────────────────────────

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
