using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using CourseMarketplaceBE.Application.DTOs;
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
    // FINANCIAL AGGREGATIONS — Chạy trên PostgreSQL, không kéo về RAM
    //
    // Công thức:
    //   GrossRevenue  = SUM(transactions.amount)  WHERE status = 'succeeded'
    //   TotalPaidOut   = SUM(payouts.payout_amount) WHERE is_paid = true
    //   PendingEscrow  = SUM(payouts.payout_amount) WHERE is_paid = false
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<decimal> GetGrossRevenueAsync(int? year = null, int? month = null)
    {
        var query = _context.Transactions
            .Where(t => t.TransactionsStatus == "succeeded" || t.TransactionsStatus == "refund_pending" || t.TransactionsStatus == "refunded");


        if (year.HasValue)
        {
            if (month.HasValue)
            {
                var startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddMonths(1);
                query = query.Where(t => t.TransactionCreatedAt >= startDate && t.TransactionCreatedAt < endDate);
            }
            else
            {
                var startDate = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddYears(1);
                query = query.Where(t => t.TransactionCreatedAt >= startDate && t.TransactionCreatedAt < endDate);
            }
        }

        var sum = await query.SumAsync(t => (decimal?)Math.Abs(t.Amount)) ?? 0;
        return sum;
    }

    public async Task<decimal> GetTotalRefundedAsync(int? year = null, int? month = null)
    {
        var query = _context.Transactions
            .Where(t => t.TransactionsStatus == "refunded");

        if (year.HasValue)
        {
            if (month.HasValue)
            {
                var startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddMonths(1);
                query = query.Where(t => t.TransactionCreatedAt >= startDate && t.TransactionCreatedAt < endDate);
            }
            else
            {
                var startDate = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddYears(1);
                query = query.Where(t => t.TransactionCreatedAt >= startDate && t.TransactionCreatedAt < endDate);
            }
        }

        var sum = await query.SumAsync(t => (decimal?)t.Amount) ?? 0;
        return Math.Abs(sum);
    }

    public async Task<int> GetSucceededTransactionCountAsync(int? year = null, int? month = null)
    {
        var query = _context.Transactions
            .Where(t => t.TransactionsStatus == "succeeded" || t.TransactionsStatus == "refund_pending");

        if (year.HasValue)
        {
            if (month.HasValue)
            {
                var startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddMonths(1);
                query = query.Where(t => t.TransactionCreatedAt >= startDate && t.TransactionCreatedAt < endDate);
            }
            else
            {
                var startDate = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddYears(1);
                query = query.Where(t => t.TransactionCreatedAt >= startDate && t.TransactionCreatedAt < endDate);
            }
        }

        return await query.CountAsync();
    }

    public async Task<decimal> GetTotalPaidOutAsync(int? year = null, int? month = null)
    {
        var query = _context.InstructorPayouts
            .Include(p => p.Transaction)
            .Where(p => p.IsPaid == true && p.PayoutStatus != "refunded" && (p.Transaction == null || p.Transaction.TransactionsStatus != "refunded"));

        if (year.HasValue)
        {
            if (month.HasValue)
            {
                var startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddMonths(1);
                query = query.Where(p => p.Transaction != null && p.Transaction.TransactionCreatedAt >= startDate && p.Transaction.TransactionCreatedAt < endDate);
            }
            else
            {
                var startDate = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddYears(1);
                query = query.Where(p => p.Transaction != null && p.Transaction.TransactionCreatedAt >= startDate && p.Transaction.TransactionCreatedAt < endDate);
            }
        }

        return await query.SumAsync(p => p.PayoutAmount);
    }

    public async Task<decimal> GetPendingEscrowAsync(int? year = null, int? month = null)
    {
        var refundLimitDate = DateTime.UtcNow.AddDays(-14);
        var query = _context.InstructorPayouts
            .Include(p => p.Transaction)
            .Where(p => p.IsPaid == false && p.PayoutStatus != "refunded" && p.Transaction != null && p.Transaction.TransactionsStatus != "refunded" && p.Transaction.TransactionCreatedAt >= refundLimitDate);

        if (year.HasValue)
        {
            if (month.HasValue)
            {
                var startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddMonths(1);
                query = query.Where(p => p.Transaction!.TransactionCreatedAt >= startDate && p.Transaction!.TransactionCreatedAt < endDate);
            }
            else
            {
                var startDate = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddYears(1);
                query = query.Where(p => p.Transaction!.TransactionCreatedAt >= startDate && p.Transaction!.TransactionCreatedAt < endDate);
            }
        }

        return await query.SumAsync(p => p.PayoutAmount);
    }

    public async Task<decimal> GetMaturedEscrowAsync(int? year = null, int? month = null)
    {
        var refundLimitDate = DateTime.UtcNow.AddDays(-14);
        var query = _context.InstructorPayouts
            .Include(p => p.Transaction)
            .Where(p => p.IsPaid == false && p.PayoutStatus != "refunded" && p.Transaction != null && p.Transaction.TransactionsStatus != "refunded" && p.Transaction.TransactionCreatedAt < refundLimitDate);

        if (year.HasValue)
        {
            if (month.HasValue)
            {
                var startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddMonths(1);
                query = query.Where(p => p.Transaction!.TransactionCreatedAt >= startDate && p.Transaction!.TransactionCreatedAt < endDate);
            }
            else
            {
                var startDate = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var endDate = startDate.AddYears(1);
                query = query.Where(p => p.Transaction!.TransactionCreatedAt >= startDate && p.Transaction!.TransactionCreatedAt < endDate);
            }
        }

        return await query.SumAsync(p => p.PayoutAmount);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PAYOUT HISTORY — JOIN instructor_payouts → transactions → order_items
    //                       → courses + instructors → users → accounts
    //
    // ★ Dùng .Select() projection để chỉ lấy đúng cột cần thiết,
    //   tránh kéo toàn bộ entity graph về RAM.
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<(List<PayoutDetailProjection> Items, int TotalCount)> GetPayoutDetailsAsync(int? year = null, int? month = null, int page = 1, int pageSize = 10)
    {
        var query = _context.InstructorPayouts
            .Include(p => p.Transaction)
                .ThenInclude(t => t!.OrderItem)
                    .ThenInclude(oi => oi!.Course)
            .Include(p => p.Instructor)
                .ThenInclude(i => i!.InstructorNavigation)
            .AsQueryable();

        if (year.HasValue && month.HasValue)
        {
            var startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);
            query = query.Where(p => p.Transaction != null && p.Transaction.TransactionCreatedAt >= startDate && p.Transaction.TransactionCreatedAt < endDate);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.Transaction!.TransactionCreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return (items, totalCount);
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

    public async Task<int> AddWithdrawalAsync(PlatformWithdrawal withdrawal)
    {
        _context.PlatformWithdrawals.Add(withdrawal);
        return await _context.SaveChangesAsync();

    }

    public async Task<(List<PlatformWithdrawal> Items, int TotalCount)> GetWithdrawalsAsync(int? year = null, int? month = null, int page = 1, int pageSize = 10)
    {
        var query = _context.PlatformWithdrawals
            .Include(w => w.Manager)
            .AsQueryable();

        if (year.HasValue && month.HasValue)
        {
            query = query.Where(w => w.CreatedAt.Year == year.Value && w.CreatedAt.Month == month.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // ── Refund ──────────────────────────────────────────────────────────────

    public async Task<(List<Transaction> Items, int TotalCount)> GetPendingRefundRequestsAsync(int page = 1, int pageSize = 10)
    {
        var query = _context.Transactions
            .Include(t => t.OrderItem)
                .ThenInclude(oi => oi!.Course)
            .Include(t => t.AccountFromNavigation)
                .ThenInclude(a => a!.User)
            .Include(t => t.TransactionExt)
            .Where(t => t.TransactionsStatus == "refund_pending");

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.TransactionExt != null ? t.TransactionExt.RefundRequestedAt : (DateTime?)null)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

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
            .Include(t => t.TransactionExt)
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
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

    public async Task<List<InstructorCourseRevenueProjection>> GetInstructorCourseRevenuesAsync(int year, int month)
    {
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var prevMonthStart = monthStart.AddMonths(-1);
        var prevMonthEnd = monthStart;

        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = yearStart.AddYears(1);

        var courses = await _context.Courses
            .Include(c => c.Instructor)
                .ThenInclude(i => i!.InstructorNavigation)
            .Where(c => !c.IsRemoved)
            .ToListAsync();

        var transactions = await _context.Transactions
            .Where(t => t.OrderItem != null && (t.TransactionsStatus == "succeeded" || t.TransactionsStatus == "refund_pending"))
            .Select(t => new
            {
                CourseId = t.OrderItem!.CourseId,
                Amount = t.Amount,
                CreatedAt = t.TransactionCreatedAt
            })
            .ToListAsync();

        decimal CalculateNetRevenue(decimal amount)
        {
            var fee = Math.Round(amount * 0.029m + 0.30m, 2);
            fee = Math.Min(fee, amount);
            return amount - fee;
        }

        var result = new List<InstructorCourseRevenueProjection>();

        foreach (var c in courses)
        {
            var courseTx = transactions.Where(t => t.CourseId == c.CourseId).ToList();

            var salesCount = courseTx.Count();

            var monthlyRevenue = courseTx
                .Where(t => t.CreatedAt >= monthStart && t.CreatedAt < monthEnd)
                .Sum(t => CalculateNetRevenue(t.Amount));

            var prevMonthRevenue = courseTx
                .Where(t => t.CreatedAt >= prevMonthStart && t.CreatedAt < prevMonthEnd)
                .Sum(t => CalculateNetRevenue(t.Amount));

            var yearlyRevenue = courseTx
                .Where(t => t.CreatedAt >= yearStart && t.CreatedAt < yearEnd)
                .Sum(t => CalculateNetRevenue(t.Amount));

            var lifetimeRevenue = courseTx
                .Sum(t => CalculateNetRevenue(t.Amount));

            result.Add(new InstructorCourseRevenueProjection
            {
                CourseId = c.CourseId,
                CourseTitle = c.Title,
                InstructorId = c.InstructorId ?? 0,
                InstructorName = c.Instructor?.InstructorNavigation?.FullName ?? "N/A",
                SalesCount = salesCount,
                MonthlyRevenue = monthlyRevenue,
                PreviousMonthRevenue = prevMonthRevenue,
                YearlyRevenue = yearlyRevenue,
                LifetimeRevenue = lifetimeRevenue
            });
        }

        return result;
    }

    public async Task<List<InstructorCourseRevenueProjection>> GetInstructorCourseRevenuesByInstructorAsync(int instructorId, int year, int month)
    {
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var prevMonthStart = monthStart.AddMonths(-1);
        var prevMonthEnd = monthStart;

        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = yearStart.AddYears(1);

        var courses = await _context.Courses
            .Include(c => c.Instructor)
                .ThenInclude(i => i!.InstructorNavigation)
            .Where(c => !c.IsRemoved && c.InstructorId == instructorId)
            .ToListAsync();

        var transactions = await _context.Transactions
            .Where(t => t.OrderItem != null && (t.TransactionsStatus == "succeeded" || t.TransactionsStatus == "refund_pending"))
            .Select(t => new
            {
                CourseId = t.OrderItem!.CourseId,
                Amount = t.Amount,
                CreatedAt = t.TransactionCreatedAt
            })
            .ToListAsync();

        decimal CalculateNetRevenue(decimal amount)
        {
            var fee = Math.Round(amount * 0.029m + 0.30m, 2);
            fee = Math.Min(fee, amount);
            return amount - fee;
        }

        var result = new List<InstructorCourseRevenueProjection>();

        foreach (var c in courses)
        {
            var courseTx = transactions.Where(t => t.CourseId == c.CourseId).ToList();

            var salesCount = courseTx.Count();

            var monthlyRevenue = courseTx
                .Where(t => t.CreatedAt >= monthStart && t.CreatedAt < monthEnd)
                .Sum(t => CalculateNetRevenue(t.Amount));

            var prevMonthRevenue = courseTx
                .Where(t => t.CreatedAt >= prevMonthStart && t.CreatedAt < prevMonthEnd)
                .Sum(t => CalculateNetRevenue(t.Amount));

            var yearlyRevenue = courseTx
                .Where(t => t.CreatedAt >= yearStart && t.CreatedAt < yearEnd)
                .Sum(t => CalculateNetRevenue(t.Amount));

            var lifetimeRevenue = courseTx
                .Sum(t => CalculateNetRevenue(t.Amount));

            result.Add(new InstructorCourseRevenueProjection
            {
                CourseId = c.CourseId,
                CourseTitle = c.Title,
                InstructorId = c.InstructorId ?? 0,
                InstructorName = c.Instructor?.InstructorNavigation?.FullName ?? "N/A",
                SalesCount = salesCount,
                MonthlyRevenue = monthlyRevenue,
                PreviousMonthRevenue = prevMonthRevenue,
                YearlyRevenue = yearlyRevenue,
                LifetimeRevenue = lifetimeRevenue
            });
        }

        return result;
    }

    public async Task<RefundEligibilityDto> GetRefundEligibilityMetricsAsync(int transactionId, int studentId, int courseId)
    {
        var account = await _context.Accounts
            .Where(a => a.AccountId == studentId)
            .Select(a => new { a.AccountFlagCount })
            .FirstOrDefaultAsync();
        var accountFlagCount = account?.AccountFlagCount ?? 0;

        var pastRefundedCount = await _context.Transactions
            .Where(t => t.AccountFrom == studentId 
                        && t.OrderItem != null 
                        && t.OrderItem.CourseId == courseId 
                        && t.TransactionsStatus == "refunded")
            .CountAsync();

        var refundRequestsLast14Days = await _context.Transactions
            .Where(t => t.AccountFrom == studentId 
                        && t.TransactionId != transactionId 
                        && t.TransactionExt != null 
                        && t.TransactionExt.RefundRequestedAt >= DateTime.UtcNow.AddDays(-14))
            .CountAsync();

        // Load active materials to RAM for safe jsonb duration calculations
        var activeMaterials = await _context.LearningMaterials
            .Where(m => m.Lesson != null 
                        && m.Lesson.CourseId == courseId 
                        && !m.Lesson.IsRemoved 
                        && m.LearningStatus != CourseMarketplaceBE.Domain.Constants.LearningStatus.Removed.ToValue())
            .Select(m => new {
                m.MaterialId,
                m.MaterialMetadata
            })
            .ToListAsync();

        var totalDurationSeconds = activeMaterials.Sum(m => m.MaterialMetadata?.Duration ?? 0);
        var courseTotalDurationHours = totalDurationSeconds / 3600.0;
        
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == studentId && e.CourseId == courseId);

        double studentProgressPercentage = 0.0;
        double completedVideoDurationHours = 0.0;

        if (enrollment != null)
        {
            var completedMaterialIds = await _context.MaterialCompletions
                .Where(c => c.EnrollmentId == enrollment.EnrollmentId)
                .Select(c => c.MaterialId)
                .ToListAsync();

            var completedActiveMaterials = activeMaterials
                .Where(m => completedMaterialIds.Contains(m.MaterialId))
                .ToList();

            studentProgressPercentage = activeMaterials.Count > 0
                ? (double)completedActiveMaterials.Count / activeMaterials.Count * 100.0
                : 0.0;

            var completedVideoDurationSeconds = completedActiveMaterials
                .Where(m => m.MaterialMetadata?.FileType == "video" || m.MaterialMetadata == null)
                .Sum(m => m.MaterialMetadata?.Duration ?? 0);

            completedVideoDurationHours = completedVideoDurationSeconds / 3600.0;
        }

        return new RefundEligibilityDto
        {
            AccountFlagCount = accountFlagCount,
            PastRefundedCountForCourse = pastRefundedCount,
            RefundRequestsLast14DaysCount = refundRequestsLast14Days,
            CourseTotalDurationHours = courseTotalDurationHours,
            StudentProgressPercentage = studentProgressPercentage,
            CompletedVideoDurationHours = completedVideoDurationHours
        };
    }

    public void RemoveInstructorPayout(InstructorPayout payout)
    {
        _context.InstructorPayouts.Remove(payout);
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}

