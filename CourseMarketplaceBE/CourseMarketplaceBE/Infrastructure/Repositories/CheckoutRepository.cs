using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

/// <summary>
/// SOLID — DIP: Implement ICheckoutRepository ở Infrastructure layer.
/// Application layer không bao giờ thấy AppDbContext — chỉ biết interface.
/// </summary>
public class CheckoutRepository : ICheckoutRepository
{
    private readonly AppDbContext _context;

    public CheckoutRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Đọc dữ liệu ─────────────────────────────────────────────────────────

    public async Task<List<CartItem>> GetCartItemsWithCourseAndInstructorAsync(int userId)
        => await _context.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Course)
                .ThenInclude(course => course!.Instructor)
            .OrderBy(c => c.AddedDate)
            .ToListAsync();


    // ── Tìm transaction theo Stripe Session ID ───────────────────────────────

    public async Task<List<Transaction>> GetTransactionsBySessionIdAsync(string stripeSessionId)
        => await _context.Transactions
            .Where(t => t.StripeSessionId == stripeSessionId)
            .Include(t => t.OrderItem)
                .ThenInclude(oi => oi!.Course)
                    .ThenInclude(c => c!.Instructor)
            .Include(t => t.OrderItem)
                .ThenInclude(oi => oi!.Order)
            .Include(t => t.InstructorPayouts)
            .ToListAsync();

    public async Task<List<Transaction>> GetTransactionsByOrderIdAsync(int orderId)
        => await _context.Transactions
            .Where(t => t.OrderItem != null && t.OrderItem.OrderId == orderId)
            .Include(t => t.OrderItem)
                .ThenInclude(oi => oi!.Order)
            .ToListAsync();

    public async Task<OrderInfo?> GetOrderWithDetailsAsync(int orderId)
        => await _context.OrderInfos
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Course)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

    public async Task<List<OrderInfo>> GetPendingOrdersByUserAsync(int userId)
        => await _context.OrderInfos
            .Where(o => o.UserId == userId && o.OrderStatus == TransactionStatus.Pending.ToValue())
            .Include(o => o.OrderItems)
            .ToListAsync();



    // ── Ghi dữ liệu ─────────────────────────────────────────────────────────

    public async Task AddOrderAsync(OrderInfo order)
        => await _context.OrderInfos.AddAsync(order);

    public async Task AddOrderItemAsync(OrderItem item)
        => await _context.OrderItems.AddAsync(item);

    public async Task AddTransactionAsync(Transaction transaction)
        => await _context.Transactions.AddAsync(transaction);



    public async Task AddInstructorPayoutAsync(InstructorPayout payout)
        => await _context.InstructorPayouts.AddAsync(payout);

    public async Task ClearCartAsync(int userId)
    {
        var items = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();
        _context.CartItems.RemoveRange(items);
    }

    public async Task IncrementCouponUsageAsync(int couponId)
    {
        var coupon = await _context.Coupons.FindAsync(couponId);
        if (coupon != null)
        {
            coupon.UsedCount = (coupon.UsedCount ?? 0) + 1;
        }
    }

    public Task DeleteOrderAsync(OrderInfo order)
    {
        // Xóa OrderItems trước do foreign key (nếu DB không cấu hình cascade delete)
        if (order.OrderItems != null && order.OrderItems.Any())
        {
            _context.OrderItems.RemoveRange(order.OrderItems);
        }
        _context.OrderInfos.Remove(order);
        return Task.CompletedTask;
    }

    public Task DeleteTransactionsAsync(IEnumerable<Transaction> transactions)
    {
        _context.Transactions.RemoveRange(transactions);
        return Task.CompletedTask;
    }

    // ── Unit of Work ─────────────────────────────────────────────────────────

    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await _context.Database.BeginTransactionAsync();

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
