using CourseMarketplaceBE.Application.Interfaces;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services
{
    public class AvatarFrameService : IAvatarFrameService
    {
        private readonly IAvatarFrameRepository _frameRepo;

        public AvatarFrameService(IAvatarFrameRepository frameRepo)
        {
            _frameRepo = frameRepo;
        }

        public async Task<bool> GrantFrameToUserAsync(string userId, int frameId)
        {
            if (!int.TryParse(userId, out var userIntId)) return false;

            if (await _frameRepo.UserHasFrameAsync(userIntId, frameId)) return true;

            var userFrame = new UserAvatarFrame
            {
                UserId = userIntId,
                FrameId = frameId,
                UnlockedAt = DateTime.Now,
                IsEquipped = false
            };

            return await _frameRepo.AddUserFrameAsync(userFrame);
        }

        public async Task<IEnumerable<dynamic>> GetUserFramesAsync(string userId)
        {
            if (!int.TryParse(userId, out var userIntId)) return Enumerable.Empty<dynamic>();

            var userFrames = await _frameRepo.GetUserFramesWithDetailsAsync(userIntId);
            return userFrames.Select(f => new {
                f.FrameId,
                f.Frame.Name,
                f.Frame.ImageUrl,
                f.IsEquipped,
                f.UnlockedAt
            });
        }

        public async Task<bool> SetActiveFrameAsync(string userId, int frameId)
        {
            if (!int.TryParse(userId, out var userIntId)) return false;
            return await _frameRepo.UpdateUserFramesEquippedStatusAsync(userIntId, frameId);
        }

        public async Task<IEnumerable<dynamic>> GetAllFramesAsync()
        {
            return await _frameRepo.GetAllActiveAsync();
        }

        public async Task<bool> CreateFrameAsync(dynamic frameDto)
        {
            var frame = new AvatarFrame
            {
                Name = frameDto.Name,
                ImageUrl = frameDto.ImageUrl,
                Description = frameDto.Description,
                RequirementType = frameDto.RequirementType,
                RequirementValue = frameDto.RequirementValue
            };
            return await _frameRepo.CreateAsync(frame);
        }
    }
}
