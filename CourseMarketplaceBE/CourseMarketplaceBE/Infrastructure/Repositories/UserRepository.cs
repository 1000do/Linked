using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
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
        await _context.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.Email == email);

    public async Task UpdateLastLoginAsync(int accountId)
    {
        var a = await _context.Accounts.FindAsync(accountId);
        if (a != null) { a.AccountLastLoginAt = DateTime.Now; await _context.SaveChangesAsync(); }
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
}