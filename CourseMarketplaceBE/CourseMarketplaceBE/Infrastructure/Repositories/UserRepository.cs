using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.UserNavigation)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<bool> UpdateUserProfileAsync(
      int userId,
      string fullName,
      string? bio,
      DateOnly? dob,
      string? avatarUrl,
      string? phoneNumber)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 🔥 LẤY USER + ACCOUNT CÙNG LÚC (FIX BUG SAI ID)
            var user = await _context.Users
                .Include(u => u.UserNavigation) // navigation sang Account
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return false;

            // ✅ UPDATE USER
            user.FullName = fullName;
            user.Bio = bio;
            user.DateOfBirth = dob;

            // 🔥 LẤY ACCOUNT ĐÚNG CÁCH
            var account = user.UserNavigation;

            if (account != null)
            {
                // ✅ Avatar: chỉ update khi upload thành công
                if (!string.IsNullOrWhiteSpace(avatarUrl))
                {
                    account.AvatarUrl = avatarUrl;
                    Console.WriteLine("Avatar updated: " + avatarUrl);
                }
                else
                {
                    Console.WriteLine("Avatar NULL -> không update");
                }

                // ✅ Phone luôn update
                account.PhoneNumber = phoneNumber;
                account.AccountUpdatedAt = DateTime.Now;
            }
            else
            {
                Console.WriteLine("Account NULL (mapping lỗi)");
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DB Update Error: {ex.Message}");
            await transaction.RollbackAsync();
            return false;
        }
    }
    public async Task<bool> IsEmailExistsAsync(string email) => await _context.Accounts.AnyAsync(a => a.Email == email);

    public async Task<Account?> GetAccountByEmailAsync(string email) =>
        await _context.Accounts
            .Include(a => a.User)
            .Include(a => a.Manager)
            .FirstOrDefaultAsync(a => a.Email == email);

    public async Task UpdateLastLoginAsync(int accountId)
    {
        var a = await _context.Accounts.FindAsync(accountId);
        if (a != null) { a.AccountLastLoginAt = Unspec(DateTime.UtcNow); await _context.SaveChangesAsync(); }
    }

    public async Task<bool> RegisterUserAsync(Account a, User u)
    {
        using var t = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Accounts.Add(a);
            await _context.SaveChangesAsync();
            u.UserId = a.AccountId;
            _context.Users.Add(u);
            await _context.SaveChangesAsync();
            await t.CommitAsync();
            return true;
        }
        catch { await t.RollbackAsync(); return false; }
    }

    public async Task SaveRefreshTokenAsync(int accountId, string refreshToken, DateTime expiry)
    {
        var a = await _context.Accounts.FindAsync(accountId);
        if (a != null)
        {
            a.RefreshToken = refreshToken;
            // Npgsql yêu cầu Kind=Unspecified cho cột timestamp without time zone
            a.RefreshTokenExpiryTime = Unspec(expiry);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>Strip DateTimeKind về Unspecified để tương thích PostgreSQL timestamp without time zone.</summary>
    private static DateTime Unspec(DateTime dt) => DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);

    public async Task<Account?> GetAccountByRefreshTokenAsync(string refreshToken) =>
        await _context.Accounts
            .Include(a => a.User)
            .Include(a => a.Manager)
            .FirstOrDefaultAsync(a => a.RefreshToken == refreshToken);

    public async Task RevokeRefreshTokenAsync(int accountId)
    {
        var a = await _context.Accounts.FindAsync(accountId);
        if (a != null)
        {
            a.RefreshToken = null;
            a.RefreshTokenExpiryTime = null;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GetRoleByAccountIdAsync(int accountId)
    {
        // Kiểm tra bảng managers — nếu tồn tại → manager, không thì → user
        var isManager = await _context.Managers.AnyAsync(m => m.ManagerId == accountId);
        return isManager ? "manager" : "user";
    }


    public async Task<bool> UpdateEmailVerifiedAsync(string email)
    {
        var acc = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);

        if (acc == null) return false;

        acc.IsVerified = true;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAccountAsync(Account account)
    {
        try
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateAccount Error: {ex.Message}");
            return false;
        }
    }
}