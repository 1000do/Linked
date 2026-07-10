using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class LockoutRepository : ILockoutRepository
{
    private readonly AppDbContext _context;

    public LockoutRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Lockout?> GetActiveLockoutAsync(int accountId, string lockoutType)
    {
        return await _context.Lockouts
            .Where(l => l.AccountId == accountId 
                     && l.LockoutType == lockoutType 
                     && l.LockoutEnd > DateTime.Now)
            .OrderByDescending(l => l.LockoutEnd)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Lockout lockout)
    {
        await _context.Lockouts.AddAsync(lockout);
        await Task.CompletedTask;
    }

    public async Task RemoveAccountLockoutsAsync(int accountId)
    {
        var activeLockouts = await _context.Lockouts
            .Where(l => l.AccountId == accountId && l.LockoutType == "account")
            .ToListAsync();
        _context.Lockouts.RemoveRange(activeLockouts);
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}
