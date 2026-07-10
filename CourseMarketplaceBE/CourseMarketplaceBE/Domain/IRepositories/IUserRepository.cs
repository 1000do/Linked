using CourseMarketplaceBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IUserRepository
{
    Task<bool> IsEmailExistsAsync(string email);
    Task<bool> IsUsernameExistsAsync(string username);
    Task<Account?> GetAccountByEmailAsync(string email);
    Task<Account?> GetAccountByEmailOrUsernameAsync(string emailOrUsername);
    Task<Account?> GetAccountByIdAsync(int accountId);
    Task<User?> GetUserByIdAsync(int userId);
    Task UpdateLastLoginAsync(int accountId);
    Task<bool> RegisterUserAsync(Account account, User user);
    Task<bool> UpdateUserProfileAsync(
        int userId, 
        string fullName, 
        string? bio, 
        DateOnly? dob, 
        string? avatarUrl, 
        string? phoneNumber,
        string? professionalTitle = null,
        string? expertiseCategories = null,
        string? linkedinUrl = null,
        string? youtubeUrl = null,
        string? facebookUrl = null,
        string? email = null);
    Task SaveRefreshTokenAsync(int accountId, string refreshToken, DateTime expiry);
    Task<Account?> GetAccountByRefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(int accountId);
    /// <summary>Trả về "manager" nếu account_id có trong bảng managers, ngược lại "user"</summary>
    Task<string> GetRoleByAccountIdAsync(int accountId);
    Task<bool> UpdateEmailVerifiedAsync(string email);
    Task<bool> UpdateAccountAsync(Account account);
    Task<int?> GetStaffAccountIdAsync();
    Task<int?> GetAdminIdAsync();
    Task<List<int>> GetAllManagerIdsAsync();
    Task<int> GetTotalStudentsCountAsync();

    // Migrated from INotificationRepository
    Task<List<int>> GetAllUserIdsAsync();
    Task<List<int>> GetUserIdsForStaffSenderAsync();
    Task<List<int>> GetUserIdsForAdminSenderAsync();
    Task<int?> GetUserIdByEmailAsync(string email);
    Task<List<string>> SearchEmailsByQueryAsync(string query, int senderId, string senderRole, int take = 5);

    // Migrated from ICheckoutRepository
    Task<string?> GetUserEmailAsync(int userId);
    Task<string?> GetInstructorStripeAccountIdAsync(int instructorId);
    Task<string?> GetInstructorStripeCountryAsync(int instructorId);
    Task<(List<Account> Items, int TotalCount)> GetAccountsPagedAsync(string? keyword, string? role, int page, int pageSize);
    Task<bool> RegisterManagerAsync(Account account, Manager manager);
    Task<int> SaveChangesAsync();
}
