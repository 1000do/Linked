using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface ILockoutRepository
{
    Task<Lockout?> GetActiveLockoutAsync(int accountId, string lockoutType);
    Task AddAsync(Lockout lockout);
}
