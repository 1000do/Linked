using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IUserRepository
{
    Task<bool> IsEmailExistsAsync(string email);
    Task<Account?> GetAccountByEmailAsync(string email);
    Task UpdateLastLoginAsync(int accountId);
    Task<bool> RegisterUserAsync(Account account, User user);
}   