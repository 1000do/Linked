using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IAvatarFrameService
    {
        Task<bool> GrantFrameToUserAsync(string userId, int frameId);
        Task<IEnumerable<dynamic>> GetUserFramesAsync(string userId);
        Task<bool> SetActiveFrameAsync(string userId, int frameId);
        Task<IEnumerable<dynamic>> GetAllFramesAsync();
        Task<bool> CreateFrameAsync(dynamic frameDto);
    }
}
