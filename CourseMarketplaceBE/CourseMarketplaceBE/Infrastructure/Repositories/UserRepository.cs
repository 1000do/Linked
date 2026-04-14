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

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await _context.Accounts.AnyAsync(a => a.Email == email);
    }

    public async Task<bool> RegisterUserAsync(Account account, User user)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            user.UserId = account.AccountId; // Đồng bộ PK/FK
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine("DB Error: " + ex.InnerException?.Message ?? ex.Message);
            return false;
        }
    }

    public async Task<Account?> GetAccountByEmailAsync(string email)
    {
        return await _context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task UpdateLastLoginAsync(int accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account != null)
        {
            account.AccountLastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}