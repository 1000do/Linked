using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IUserRepository
{
    Task<bool> IsEmailExistsAsync(string email);
    Task<Account?> GetAccountByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int userId);
    Task UpdateLastLoginAsync(int accountId);
    Task<bool> RegisterUserAsync(Account account, User user);
    Task<bool> UpdateUserProfileAsync(int userId, string fullName, string? bio, DateOnly? dob, string? avatarUrl, string? phoneNumber);
    Task SaveRefreshTokenAsync(int accountId, string refreshToken, DateTime expiry);
    Task<Account?> GetAccountByRefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(int accountId);
}
