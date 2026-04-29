using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using CourseMarketplaceBE.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

/// <summary>
/// SOLID — SRP: Repository CHỈ làm nhiệm vụ truy vấn DB.
/// SOLID — DIP: Implement IAdminFinanceRepository, Service không biết EF Core.
///
/// ★ Performance: Dùng .SumAsync() / .CountAsync() để tính toán trên DB (PostgreSQL),
///   KHÔNG kéo toàn bộ list về RAM rồi .Sum() trong C#.
/// </summary>
public class AdminFinanceRepository : IAdminFinanceRepository
{
    private readonly AppDbContext _context;

    public AdminFinanceRepository(AppDbContext context) => _context = context;

    // ═══════════════════════════════════════════════════════════════════════
    // SYSTEM CONFIG — Upsert pattern
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<string?> GetConfigValueAsync(string configKey)
    {
        return await _context.SystemConfigs
            .Where(c => c.ConfigKey == configKey)
            .Select(c => c.ConfigValue)
            .FirstOrDefaultAsync();
    }

    public async Task UpsertConfigAsync(string configKey, string configValue, string? description = null)
    {
        var existing = await _context.SystemConfigs
            .FirstOrDefaultAsync(c => c.ConfigKey == configKey);

        if (existing != null)
        {
            existing.ConfigValue = configValue;
            existing.Description = description ?? existing.Description;
            existing.UpdatedAt = DateTime.Now;
        }
        else
        {
            _context.SystemConfigs.Add(new SystemConfig
            {
                ConfigKey = configKey,
                ConfigValue = configValue,
                Description = description ?? $"Auto-created config: {configKey}",
                UpdatedAt = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // FINANCIAL AGGREGATIONS — Chạy trên PostgreSQL, không kéo về RAM
    //
    // Công thức:
    //   GrossRevenue  = SUM(transactions.amount)  WHERE status = 'succeeded'
    //   TotalPaidOut   = SUM(payouts.payout_amount) WHERE is_paid = true
    //   PendingEscrow  = SUM(payouts.payout_amount) WHERE is_paid = false
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<decimal> GetGrossRevenueAsync()
    {
        // ★ .SumAsync() → SQL: SELECT COALESCE(SUM(amount), 0) FROM transactions WHERE status = 'succeeded'
        return await _context.Transactions
            .Where(t => t.TransactionsStatus == "succeeded")
            .SumAsync(t => t.Amount);
    }

    public async Task<int> GetSucceededTransactionCountAsync()
    {
        return await _context.Transactions
            .Where(t => t.TransactionsStatus == "succeeded")
            .CountAsync();
    }

    public async Task<decimal> GetTotalPaidOutAsync()
    {
        // ★ SQL: SELECT COALESCE(SUM(payout_amount), 0) FROM instructor_payouts WHERE is_paid = true
        return await _context.InstructorPayouts
            .Where(p => p.IsPaid == true)
            .SumAsync(p => p.PayoutAmount);
    }

    public async Task<decimal> GetPendingEscrowAsync()
    {
        // ★ SQL: SELECT COALESCE(SUM(payout_amount), 0) FROM instructor_payouts WHERE is_paid = false
        return await _context.InstructorPayouts
            .Where(p => p.IsPaid == false)
            .SumAsync(p => p.PayoutAmount);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PAYOUT HISTORY — JOIN instructor_payouts → transactions → order_items
    //                       → courses + instructors → users → accounts
    //
    // ★ Dùng .Select() projection để chỉ lấy đúng cột cần thiết,
    //   tránh kéo toàn bộ entity graph về RAM.
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<List<PayoutDetailProjection>> GetPayoutDetailsAsync()
    {
        return await _context.InstructorPayouts
            .Include(p => p.Transaction)
                .ThenInclude(t => t!.OrderItem)
                    .ThenInclude(oi => oi!.Course)
            .Include(p => p.Instructor)
                .ThenInclude(i => i!.InstructorNavigation)
            .OrderByDescending(p => p.Transaction!.TransactionCreatedAt)
            .Select(p => new PayoutDetailProjection
            {
                PayoutId = p.PayoutId,
                TransactionId = p.TransactionId ?? 0,
                InstructorName = p.Instructor != null && p.Instructor.InstructorNavigation != null
                    ? p.Instructor.InstructorNavigation.FullName ?? "N/A"
                    : "N/A",
                InstructorEmail = "", // Sẽ lấy ở bước dưới nếu cần
                CourseTitle = p.Transaction != null && p.Transaction.OrderItem != null && p.Transaction.OrderItem.Course != null
                    ? p.Transaction.OrderItem.Course.Title ?? "N/A"
                    : "N/A",
                TotalAmount = p.Transaction != null ? p.Transaction.Amount : 0,
                InstructorReceived = p.PayoutAmount,
                TransferRate = p.Transaction != null ? p.Transaction.TransferRate : 0,
                IsPaid = p.IsPaid,
                TransactionDate = p.Transaction != null ? p.Transaction.TransactionCreatedAt : null,
                PayoutDate = p.PayoutDate
            })
            .ToListAsync();
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
