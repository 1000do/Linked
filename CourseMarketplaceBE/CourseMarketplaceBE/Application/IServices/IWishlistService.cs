using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IWishlistService
{
    Task<List<WishlistResponse>> GetWishlistAsync(int userId);
    Task AddToWishlistAsync(int userId, int courseId);
    Task RemoveFromWishlistAsync(int userId, int courseId);
    Task<bool> ToggleWishlistAsync(int userId, int courseId);
    Task<bool> IsInWishlistAsync(int userId, int courseId);
    Task<int> GetWishlistCountAsync(int userId);
}
