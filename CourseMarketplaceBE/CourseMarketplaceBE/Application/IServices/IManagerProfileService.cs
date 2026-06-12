using CourseMarketplaceBE.Application.DTOs;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IManagerProfileService
    {
        Task<ManagerProfileResponse?> GetProfileAsync(int managerId);
        Task<bool> UpdateProfileAsync(int managerId, UpdateManagerProfileRequest request);
        Task<(bool Success, string Message)> ChangePasswordAsync(int managerId, string currentPassword, string newPassword);
    }
}
