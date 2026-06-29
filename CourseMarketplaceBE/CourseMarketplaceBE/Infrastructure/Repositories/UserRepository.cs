 using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            .Include(u => u.Instructor)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<bool> UpdateUserProfileAsync(
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
      string? email = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _context.Users
                .Include(u => u.UserNavigation) 
                .Include(u => u.Instructor)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return false;

            user.FullName = fullName;
            user.Bio = bio;
            user.DateOfBirth = dob;

            var account = user.UserNavigation;

            if (account != null)
            {
                if (!string.IsNullOrWhiteSpace(avatarUrl))
                {
                    account.AvatarUrl = avatarUrl;
                }
                account.PhoneNumber = phoneNumber;
                account.AccountUpdatedAt = DateTime.Now;

                if (!string.IsNullOrWhiteSpace(email) && !email.Equals(account.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var isEmailTaken = await _context.Accounts.AnyAsync(a => a.AccountId != account.AccountId && a.Email.ToLower() == email.ToLower());
                    if (isEmailTaken)
                    {
                        throw new InvalidOperationException("Email is already taken by another account.");
                    }
                    account.Email = email.ToLower();
                    account.IsVerified = false;
                }
            }

            // Update instructor fields if they exist
            if (user.Instructor != null)
            {
                user.Instructor.ProfessionalTitle = professionalTitle;
                user.Instructor.ExpertiseCategories = expertiseCategories;
                user.Instructor.LinkedinUrl = linkedinUrl;
                user.Instructor.YoutubeUrl = youtubeUrl;
                user.Instructor.FacebookUrl = facebookUrl;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
    public async Task<bool> IsEmailExistsAsync(string email) => await _context.Accounts.AnyAsync(a => a.Email.ToLower() == email.ToLower());

    public async Task<bool> IsUsernameExistsAsync(string username) =>
        await _context.Accounts.AnyAsync(a => a.Username != null && a.Username.ToLower() == username.ToLower());

    public async Task<Account?> GetAccountByEmailAsync(string email) =>
        await _context.Accounts
            .Include(a => a.User)
            .Include(a => a.Manager)
            .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());

    public async Task<Account?> GetAccountByEmailOrUsernameAsync(string emailOrUsername) =>
        await _context.Accounts
            .Include(a => a.User)
            .Include(a => a.Manager)
            .FirstOrDefaultAsync(a => (a.Email != null && a.Email.ToLower() == emailOrUsername.ToLower()) 
                                   || (a.Username != null && a.Username.ToLower() == emailOrUsername.ToLower()));
            
    public async Task<Account?> GetAccountByIdAsync(int accountId) =>
        await _context.Accounts
            .Include(a => a.User)
                .ThenInclude(u => u!.Instructor)
            .Include(a => a.Manager)
            .FirstOrDefaultAsync(a => a.AccountId == accountId);

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
            a.RefreshTokenExpiryTime = Unspec(expiry);
            await _context.SaveChangesAsync();
        }
    }

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
        var manager = await _context.Managers.FirstOrDefaultAsync(m => m.ManagerId == accountId);
        if (manager != null) return manager.Role; 

        var isInstructor = await _context.Instructors.AnyAsync(i =>
            i.InstructorId == accountId
            && i.ApprovalStatus == "Approved"
            && i.StripeOnboardingStatus == "Active");
        if (isInstructor) return "instructor";

        return "user";
    }


    public async Task<bool> UpdateEmailVerifiedAsync(string email)
    {
        var acc = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        if (acc == null) return false;
        acc.IsVerified = true;
        int numRowsAffected = await _context.SaveChangesAsync();
        return numRowsAffected > 0;
    }

    public async Task<bool> UpdateAccountAsync(Account account)
    {
        try
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int?> GetStaffAccountIdAsync()
    {
        var staff = await _context.Managers.FirstOrDefaultAsync(m => m.Role == "staff");
        if (staff != null) return staff.ManagerId;
        var admin = await _context.Managers.FirstOrDefaultAsync(m => m.Role == "admin");
        return admin?.ManagerId;
    }

    public async Task<int?> GetAdminIdAsync()
    {
        var admin = await _context.Managers.FirstOrDefaultAsync(m => m.Role == "admin");
        return admin?.ManagerId;
    }

    public async Task<List<int>> GetAllManagerIdsAsync()
    {
        return await _context.Managers.Select(m => m.ManagerId).ToListAsync();
    }

    public async Task<int> GetTotalStudentsCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<List<int>> GetAllUserIdsAsync()
    {
        return await _context.Users
            .Select(u => u.UserId)
            .ToListAsync();
    }

    public async Task<List<int>> GetUserIdsForStaffSenderAsync()
    {
        return await _context.Users
            .Select(u => u.UserId)
            .ToListAsync();
    }

    public async Task<List<int>> GetUserIdsForAdminSenderAsync()
    {
        return await GetUserIdsForStaffSenderAsync();
        // var staffIds = await _context.Managers
        //     .Where(m => m.Role == "staff")
        //     .Select(m => m.ManagerId)
        //     .ToListAsync();

        // var userIds = await _context.Users
        //     .Select(u => u.UserId)
        //     .ToListAsync();

        // return staffIds.Concat(userIds).Distinct().ToList();
    }

    public async Task<int?> GetUserIdByEmailAsync(string email)
    {
        return await _context.Accounts
            .Where(a => a.Email == email)
            .Select(a => (int?)a.AccountId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<string>> SearchEmailsByQueryAsync(string query, int senderId, string senderRole, int take = 5)
    {
        var dbQuery = _context.Accounts
            .Where(a => a.Email != null && a.Email.ToLower().Contains(query.ToLower()) && a.AccountId != senderId);

        if (senderRole == "staff")
        {
            dbQuery = dbQuery.Where(a => !_context.Managers.Any(m => m.ManagerId == a.AccountId));
        }
        else if (senderRole == "admin")
        {
            dbQuery = dbQuery.Where(a => !_context.Managers.Any(m => m.ManagerId == a.AccountId && m.Role == "admin"));
        }

        return await dbQuery
            .Select(a => a.Email!)
            .Take(take)
            .ToListAsync();
    }

    public async Task<string?> GetUserEmailAsync(int userId)
    {
        return await _context.Accounts
            .Where(a => a.AccountId == userId)
            .Select(a => a.Email)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetInstructorStripeAccountIdAsync(int instructorId)
    {
        return await _context.Instructors
            .Where(i => i.InstructorId == instructorId)
            .Select(i => i.StripeAccountId)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetInstructorStripeCountryAsync(int instructorId)
    {
        return await _context.Instructors
            .Where(i => i.InstructorId == instructorId)
            .Select(i => i.StripeCountry)
            .FirstOrDefaultAsync();
    }

    public async Task<(List<Account> Items, int TotalCount)> GetAccountsPagedAsync(string? keyword, string? role, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.Accounts
            .Include(a => a.Manager)
            .Include(a => a.User)
                .ThenInclude(u => u!.Instructor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(role) && role.ToLower() != "all")
        {
            var roleLower = role.ToLower();
            if (roleLower == "staff")
            {
                query = query.Where(a => a.Manager != null && a.Manager.Role == "staff");
            }
            else if (roleLower == "user")
            {
                query = query.Where(a => a.Manager == null);
            }
            else if (roleLower == "instructor")
            {
                query = query.Where(a => a.User != null && a.User.Instructor != null);
            }
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.ToLower();
            query = query.Where(a =>
                a.Email.ToLower().Contains(kw) ||
                (a.PhoneNumber != null && a.PhoneNumber.Contains(kw)) ||
                (a.Manager != null && a.Manager.DisplayName.ToLower().Contains(kw)) ||
                (a.User != null && a.User.FullName.ToLower().Contains(kw))
            );
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AccountCreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> RegisterManagerAsync(Account account, Manager manager)
    {
        using var t = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            manager.ManagerId = account.AccountId;
            _context.Managers.Add(manager);
            await _context.SaveChangesAsync();
            await t.CommitAsync();
            return true;
        }
        catch
        {
            await t.RollbackAsync();
            return false;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
