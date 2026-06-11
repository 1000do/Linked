using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class GiftRepository : IGiftRepository
{
    private readonly AppDbContext _context;

    public GiftRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Gift?> GetByIdAsync(int giftId)
    {
        return await _context.Gifts
            .Include(g => g.OrderItem)
                .ThenInclude(oi => oi.Course)
            .Include(g => g.Sender)
            .Include(g => g.ClaimedByUser)
            .FirstOrDefaultAsync(g => g.GiftId == giftId);
    }

    public async Task<Gift?> GetByTokenAsync(string token)
    {
        return await _context.Gifts
            .Include(g => g.OrderItem)
                .ThenInclude(oi => oi.Course)
            .Include(g => g.Sender)
            .Include(g => g.ClaimedByUser)
            .FirstOrDefaultAsync(g => g.RedemptionToken == token);
    }

    public async Task<Gift?> GetByOrderItemIdAsync(int orderItemId)
    {
        return await _context.Gifts
            .Include(g => g.OrderItem)
            .FirstOrDefaultAsync(g => g.OrderItemId == orderItemId);
    }

    public async Task AddAsync(Gift gift)
    {
        await _context.Gifts.AddAsync(gift);
    }

    public void Update(Gift gift)
    {
        _context.Gifts.Update(gift);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
