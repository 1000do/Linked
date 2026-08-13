using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class GiftCheckoutSessionRepository : IGiftCheckoutSessionRepository
{
    private readonly AppDbContext _context;

    public GiftCheckoutSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GiftCheckoutSession?> GetByIdAsync(string id)
    {
        return await _context.GiftCheckoutSessions
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.GiftCheckoutSessionId == id);
    }

    public async Task AddAsync(GiftCheckoutSession session)
    {
        _context.GiftCheckoutSessions.Add(session);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
