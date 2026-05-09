using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IWishlistRepository
{
    Task<List<WishlistItem>> GetByUserIdAsync(int userId);
    Task<WishlistItem?> GetByUserAndCourseAsync(int userId, int courseId);
    Task AddAsync(WishlistItem item);
    Task RemoveAsync(WishlistItem item);
    Task<bool> ExistsAsync(int userId, int courseId);
    Task SaveChangesAsync();
}
