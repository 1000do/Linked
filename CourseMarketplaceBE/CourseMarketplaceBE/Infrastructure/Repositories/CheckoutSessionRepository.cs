using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Infrastructure.Data;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class CheckoutSessionRepository : ICheckoutSessionRepository
{
    private readonly AppDbContext _context;

    public CheckoutSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateSessionAsync(CheckoutSession session)
    {
        _context.CheckoutSessions.Add(session);
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new CheckoutSessionException("Database operation failed due to a constraint violation or data issue while saving CheckoutSession.", ex);
        }
    }

    public async Task<CheckoutSession?> GetSessionByIdAsync(string sessionId)
    {
        return await _context.CheckoutSessions
            .FirstOrDefaultAsync(s => s.CheckoutSessionId == sessionId);
    }

    public async Task<int> UpdateSessionAsync(CheckoutSession session)
    {
        _context.CheckoutSessions.Update(session);
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new CheckoutSessionException("Database operation failed due to a constraint violation or data issue while updating CheckoutSession.", ex);
        }
    }
}
