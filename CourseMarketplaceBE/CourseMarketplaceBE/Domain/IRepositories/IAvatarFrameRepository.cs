using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface IAvatarFrameRepository
    {
        Task<AvatarFrame?> GetByIdAsync(int id);
        Task<IEnumerable<AvatarFrame>> GetAllActiveAsync();
        Task<bool> CreateAsync(AvatarFrame frame);
        
        Task<bool> UserHasFrameAsync(int userId, int frameId);
        Task<bool> AddUserFrameAsync(UserAvatarFrame userFrame);
        Task<IEnumerable<UserAvatarFrame>> GetUserFramesWithDetailsAsync(int userId);
        Task<bool> UpdateUserFramesEquippedStatusAsync(int userId, int frameId);
    }
}
