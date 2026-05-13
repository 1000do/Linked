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
                PayoutDate = p.PayoutDate,
                PayoutStatus = p.PayoutStatus,
                StripeTransferId = p.StripeTransferId,
                StripePayoutId = p.StripePayoutId,
                PaidToBankAt = p.PaidToBankAt
            })
            .ToListAsync();
    }

    public async Task<InstructorPayout?> GetPayoutByIdAsync(int payoutId)
    {
        return await _context.InstructorPayouts
            .Include(p => p.Instructor)
            .Include(p => p.Transaction)
            .FirstOrDefaultAsync(p => p.PayoutId == payoutId);
    }

    /// <summary>
    /// Tìm TẤT CẢ payout theo StripePayoutId (po_xxx) — dùng trong webhook payout.paid / payout.failed.
    /// </summary>
    public async Task<List<InstructorPayout>> GetPayoutsByStripePayoutIdAsync(string stripePayoutId)
    {
        return await _context.InstructorPayouts
            .Where(p => p.StripePayoutId == stripePayoutId)
            .ToListAsync();
    }

    /// <summary>
    /// Tìm tất cả payout đang ở trạng thái 'transferred' của một Connected Account.
    /// Dùng trong webhook payout.created để lưu stripe_payout_id cho toàn bộ các khoản giao dịch.
    /// </summary>
    public async Task<List<InstructorPayout>> GetTransferredPayoutsByAccountAsync(string stripeAccountId)
    {
        return await _context.InstructorPayouts
            .Include(p => p.Instructor)
            .Where(p => p.PayoutStatus == "transferred"
                     && p.Instructor != null
                     && p.Instructor.StripeAccountId == stripeAccountId)
            .ToListAsync();
    }

    public async Task<InstructorPayout?> GetPayoutByTransferIdAsync(string transferId)
    {
        return await _context.InstructorPayouts
            .FirstOrDefaultAsync(p => p.StripeTransferId == transferId);
    }

    // ── Platform Withdrawals ──────────────────────────────────────────────

    public async Task AddWithdrawalAsync(PlatformWithdrawal withdrawal)
    {
        _context.PlatformWithdrawals.Add(withdrawal);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PlatformWithdrawal>> GetWithdrawalsAsync()
    {
        return await _context.PlatformWithdrawals
            .Include(w => w.Manager)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    // ── Refund ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Lấy Transaction entity đầy đủ kèm toàn bộ entity graph cần cho refund:
    ///   Transaction → InstructorPayouts → Instructor
    ///   Transaction → OrderItem → Course → Instructor
    ///   Transaction → AccountFrom → User (buyer)
    /// </summary>
    public async Task<Transaction?> GetTransactionWithFullGraphAsync(int transactionId)
    {
        return await _context.Transactions
            .Include(t => t.InstructorPayouts)
                .ThenInclude(p => p.Instructor)
            .Include(t => t.OrderItem)
                .ThenInclude(oi => oi!.Course)
                    .ThenInclude(c => c!.Instructor)
            .Include(t => t.AccountFromNavigation)
                .ThenInclude(a => a!.User)
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
    }

    /// <summary>
    /// Tìm Enrollment active của user cho khóa học cụ thể.
    /// Chỉ trả về enrollment có status "active" (không tìm enrollment đã bị revoke trước đó).
    /// </summary>
    public async Task<Enrollment?> GetActiveEnrollmentAsync(int userId, int courseId)
    {
        return await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId
                                   && e.CourseId == courseId
                                   && e.EnrollmentStatus == "active");
    }

    /// <summary>
    /// Resolve DestinationPaymentId (py_xxx) → Transfer ID (tr_xxx).
    /// 
    /// ★ Context: Khi tạo Transfer, Stripe trả về Transfer.Id (tr_xxx) và 
    ///   Transfer.DestinationPaymentId (py_xxx). Hệ thống lưu py_xxx vào DB.
    ///   Nhưng Transfer Reversal API yêu cầu tr_xxx.
    ///   → Gọi Stripe Charge.Get trên Connected Account để tìm transfer gốc.
    /// </summary>
    public async Task<string?> GetStripeTransferIdByDestinationPaymentAsync(string destinationPaymentId)
    {
        // Tìm payout có StripeTransferId = py_xxx để lấy thông tin instructor
        var payout = await _context.InstructorPayouts
            .Include(p => p.Instructor)
            .FirstOrDefaultAsync(p => p.StripeTransferId == destinationPaymentId);

        if (payout?.Instructor?.StripeAccountId == null)
            return null;

        // Gọi Stripe API để lấy Charge trên Connected Account → tìm source_transfer (tr_xxx)
        try
        {
            var chargeService = new Stripe.ChargeService();
            var charge = await chargeService.GetAsync(
                destinationPaymentId,
                requestOptions: new Stripe.RequestOptions { StripeAccount = payout.Instructor.StripeAccountId });
            return charge?.SourceTransferId; // tr_xxx
        }
        catch
        {
            return null;
        }
    }

    public async Task<Transaction?> GetTransactionByPaymentIntentIdAsync(string paymentIntentId)
    {
        return await _context.Transactions
            .Include(t => t.InstructorPayouts)
            .Include(t => t.OrderItem)
            .Include(t => t.AccountFromNavigation)
                .ThenInclude(a => a!.User)
            .FirstOrDefaultAsync(t => t.StripePaymentintentId == paymentIntentId);
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
