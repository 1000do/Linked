using CourseMarketplaceBE.Domain.Entities;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface IManagerRepository
    {
        Task<Manager?> GetManagerByIdAsync(int managerId);
        Task<bool> UpdateManagerProfileAsync(int managerId, string displayName, string? fullName, string? phoneNumber, string? avatarUrl, string? bio);
        Task<bool> UpdateAccountPasswordHashAsync(int accountId, string passwordHash);
    }
}
