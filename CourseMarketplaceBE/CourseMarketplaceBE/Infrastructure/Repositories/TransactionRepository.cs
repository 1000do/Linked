using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

/// <summary>
/// SOLID — SRP: Chỉ truy vấn dữ liệu giao dịch.
/// SOLID — DIP: Implement ITransactionRepository — Service không biết EF Core.
///
/// ★ Performance:
///   - Dùng .Select() projection: chỉ lấy đúng cột cần thiết, không kéo toàn entity graph
///   - Dùng .Skip().Take() cho phân trang trên DB (LIMIT/OFFSET trong PostgreSQL)
///   - TotalCount dùng .CountAsync() riêng biệt trước khi lấy data
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context) => _context = context;

    // ═══════════════════════════════════════════════════════════════════════
    // UC-115: DANH SÁCH GIAO DỊCH (CÓ PHÂN TRANG)
    //
    // SQL tương đương:
    //   SELECT t.*, u_buyer.full_name, c.title, u_inst.full_name
    //   FROM transactions t
    //   LEFT JOIN order_items oi ON t.order_item_id = oi.id
    //   LEFT JOIN courses c ON oi.course_id = c.course_id
    //   LEFT JOIN instructors i ON c.instructor_id = i.instructor_id
    //   LEFT JOIN users u_inst ON i.instructor_id = u_inst.user_id
    //   LEFT JOIN accounts a_buyer ON t.account_from = a_buyer.account_id
    //   LEFT JOIN users u_buyer ON a_buyer.user_id = u_buyer.user_id
    //   ORDER BY t.transaction_created_at DESC
    //   LIMIT @pageSize OFFSET @skip
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<(List<TransactionListDto> Items, int TotalCount)> GetTransactionsAsync(int page, int pageSize, string? keyword = null, string? sortBy = "date_desc", string? status = null, int? year = null, int? month = null)
    {
        var baseQuery = _context.Transactions.AsQueryable();

        if (year.HasValue && month.HasValue)
        {
            var startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);
            baseQuery = baseQuery.Where(t => t.TransactionCreatedAt >= startDate && t.TransactionCreatedAt < endDate);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var keywordLower = keyword.ToLower();
            baseQuery = baseQuery.Where(t =>
                (t.OrderItem != null && t.OrderItem.Course != null && t.OrderItem.Course.Title != null && t.OrderItem.Course.Title.ToLower().Contains(keywordLower)) ||
                (t.AccountFromNavigation != null && t.AccountFromNavigation.User != null && t.AccountFromNavigation.User.FullName != null && t.AccountFromNavigation.User.FullName.ToLower().Contains(keywordLower)) ||
                (t.OrderItem != null && t.OrderItem.Course != null && t.OrderItem.Course.Instructor != null && t.OrderItem.Course.Instructor.InstructorNavigation != null && t.OrderItem.Course.Instructor.InstructorNavigation.FullName != null && t.OrderItem.Course.Instructor.InstructorNavigation.FullName.ToLower().Contains(keywordLower))
            );
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusLower = status.ToLower();
            baseQuery = baseQuery.Where(t => t.TransactionsStatus != null && t.TransactionsStatus.ToLower() == statusLower);
        }
        else
        {
            baseQuery = baseQuery.Where(t => t.TransactionsStatus != "pending");
        }

        switch (sortBy?.ToLower())
        {
            case "date_asc":
                baseQuery = baseQuery.OrderBy(t => t.TransactionCreatedAt);
                break;
            case "amount_desc":
                baseQuery = baseQuery.OrderByDescending(t => t.Amount);
                break;
            case "amount_asc":
                baseQuery = baseQuery.OrderBy(t => t.Amount);
                break;
            default: // date_desc
                baseQuery = baseQuery.OrderByDescending(t => t.TransactionCreatedAt);
                break;
        }

        var query = baseQuery
            .Select(t => new TransactionListDto
            {
                TransactionId  = t.TransactionId,
                StripeSessionId = t.StripeSessionId,
                Date           = t.TransactionCreatedAt,
                Amount         = t.Amount,
                Currency       = t.Currency ?? "USD",
                Status         = t.TransactionsStatus,

                // ★ Tên người mua: account_from → account → user
                BuyerName = t.AccountFromNavigation != null && t.AccountFromNavigation.User != null
                    ? t.AccountFromNavigation.User.FullName ?? "N/A"
                    : "N/A",

                // ★ Tên khóa học: order_item → course
                CourseTitle = t.OrderItem != null && t.OrderItem.Course != null
                    ? t.OrderItem.Course.Title ?? "N/A"
                    : "N/A",

                // ★ Tên giảng viên: order_item → course → instructor → user
                InstructorName = t.OrderItem != null
                    && t.OrderItem.Course != null
                    && t.OrderItem.Course.Instructor != null
                    && t.OrderItem.Course.Instructor.InstructorNavigation != null
                    ? t.OrderItem.Course.Instructor.InstructorNavigation.FullName ?? "N/A"
                    : "N/A",

                RefundReason = t.RefundReason,
                RefundAdminNote = t.RefundAdminNote,
                RefundRequestedAt = t.RefundRequestedAt
            });

        // ★ Đếm tổng trước (COUNT chạy trong DB)
        var totalCount = await query.CountAsync();

        // ★ Lấy page (LIMIT/OFFSET trong PostgreSQL)
        var skip = (page - 1) * pageSize;
        var items = await query.Skip(skip).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // UC-114: CHI TIẾT 1 GIAO DỊCH
    //
    // Dùng FirstOrDefaultAsync với Include để lấy đủ thông tin biên lai.
    // Không dùng eager loading toàn bộ graph — chỉ dùng .Select() projection.
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<TransactionDetailDto?> GetTransactionDetailAsync(int transactionId)
    {
        return await _context.Transactions
            .Where(t => t.TransactionId == transactionId)
            .Select(t => new TransactionDetailDto
            {
                // ── Giao dịch ────────────────────────────────────────────
                TransactionId         = t.TransactionId,
                StripeSessionId       = t.StripeSessionId,
                StripePaymentIntentId = t.StripePaymentintentId,
                Date                  = t.TransactionCreatedAt,
                Status                = t.TransactionsStatus,
                Currency              = t.Currency ?? "AUD",

                // ── Người mua ─────────────────────────────────────────────
                BuyerName = t.AccountFromNavigation != null && t.AccountFromNavigation.User != null
                    ? t.AccountFromNavigation.User.FullName ?? "N/A" : "N/A",
                BuyerEmail = t.AccountFromNavigation != null && t.AccountFromNavigation.Email != null
                    ? t.AccountFromNavigation.Email : "N/A",

                // ── Khóa học ──────────────────────────────────────────────
                CourseTitle = t.OrderItem != null && t.OrderItem.Course != null
                    ? t.OrderItem.Course.Title ?? "N/A" : "N/A",
                CourseThumbnail = t.OrderItem != null && t.OrderItem.Course != null
                    ? t.OrderItem.Course.CourseThumbnailUrl : null,

                // ── Giảng viên ────────────────────────────────────────────
                InstructorName = t.OrderItem != null
                    && t.OrderItem.Course != null
                    && t.OrderItem.Course.Instructor != null
                    && t.OrderItem.Course.Instructor.InstructorNavigation != null
                    ? t.OrderItem.Course.Instructor.InstructorNavigation.FullName ?? "N/A" : "N/A",
                InstructorEmail = t.OrderItem != null
                    && t.OrderItem.Course != null
                    && t.OrderItem.Course.Instructor != null
                    && t.OrderItem.Course.Instructor.InstructorNavigation != null
                    && t.OrderItem.Course.Instructor.InstructorNavigation.UserNavigation != null
                    ? t.OrderItem.Course.Instructor.InstructorNavigation.UserNavigation.Email ?? "N/A" : "N/A",

                // ── Phân bổ tài chính ★ ───────────────────────────────────
                // ★ Ưu tiên đọc từ snapshot order_items (dữ liệu bất biến tại thời điểm mua)
                //   Fallback về courses.price nếu chưa có dữ liệu snapshot (giao dịch cũ)
                GrossAmount      = t.Amount,
                OriginalPrice    = t.OrderItem != null && t.OrderItem.OriginalPrice.HasValue
                    ? t.OrderItem.OriginalPrice.Value
                    : (t.OrderItem != null && t.OrderItem.Course != null ? t.OrderItem.Course.Price : t.Amount),
                DiscountAmount   = t.OrderItem != null && t.OrderItem.DiscountAmount > 0
                    ? t.OrderItem.DiscountAmount
                    : (t.OrderItem != null && t.OrderItem.Course != null 
                        ? Math.Max(t.OrderItem.Course.Price - t.Amount, 0) : 0m),
                CouponUsed       = t.OrderItem != null && t.OrderItem.CouponUsed == true,
                CouponCode       = t.OrderItem != null && t.OrderItem.CouponCode != null
                    ? t.OrderItem.CouponCode
                    : (t.OrderItem != null && t.OrderItem.CouponUsed == true
                       && t.OrderItem.Course != null && t.OrderItem.Course.Coupon != null
                        ? t.OrderItem.Course.Coupon.CouponCode : null),
                CouponType       = t.OrderItem != null && t.OrderItem.CouponType != null
                    ? t.OrderItem.CouponType
                    : (t.OrderItem != null && t.OrderItem.CouponUsed == true
                       && t.OrderItem.Course != null && t.OrderItem.Course.Coupon != null
                        ? t.OrderItem.Course.Coupon.CouponType : null),
                CouponDiscountValue = t.OrderItem != null && t.OrderItem.CouponUsed == true
                                      && t.OrderItem.Course != null && t.OrderItem.Course.Coupon != null
                    ? t.OrderItem.Course.Coupon.DiscountValue : null,
                TransferRate     = t.TransferRate,
                // Payout = amount lấy từ instructor_payouts (là số thực tế đã tính)
                InstructorPayout = t.InstructorPayouts.Any()
                    ? t.InstructorPayouts.First().PayoutAmount
                    : Math.Round(t.Amount * (t.TransferRate / 100m), 2),
                PlatformProfit   = t.Amount - (t.InstructorPayouts.Any()
                    ? t.InstructorPayouts.First().PayoutAmount
                    : Math.Round(t.Amount * (t.TransferRate / 100m), 2)),
                IsPaid = t.InstructorPayouts.Any() && t.InstructorPayouts.First().IsPaid,

                RefundReason = t.RefundReason,
                RefundAdminNote = t.RefundAdminNote,
                RefundRequestedAt = t.RefundRequestedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(List<TransactionListDto> Items, int TotalCount)> GetInstructorTransactionsAsync(int instructorId, int page, int pageSize, string? keyword = null, string? sortBy = "date_desc", string? status = null, int? year = null, int? month = null)
    {
        var baseQuery = _context.Transactions
            .Where(t => t.InstructorPayouts.Any(p => p.InstructorId == instructorId));

        if (year.HasValue && month.HasValue)
        {
            var startDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);
            baseQuery = baseQuery.Where(t => t.TransactionCreatedAt >= startDate && t.TransactionCreatedAt < endDate);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var keywordLower = keyword.ToLower();
            baseQuery = baseQuery.Where(t =>
                (t.OrderItem != null && t.OrderItem.Course != null && t.OrderItem.Course.Title != null && t.OrderItem.Course.Title.ToLower().Contains(keywordLower)) ||
                (t.AccountFromNavigation != null && t.AccountFromNavigation.User != null && t.AccountFromNavigation.User.FullName != null && t.AccountFromNavigation.User.FullName.ToLower().Contains(keywordLower))
            );
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusLower = status.ToLower();
            baseQuery = baseQuery.Where(t => t.TransactionsStatus != null && t.TransactionsStatus.ToLower() == statusLower);
        }

        switch (sortBy?.ToLower())
        {
            case "date_asc":
                baseQuery = baseQuery.OrderBy(t => t.TransactionCreatedAt);
                break;
            case "amount_desc":
                baseQuery = baseQuery.OrderByDescending(t => t.Amount);
                break;
            case "amount_asc":
                baseQuery = baseQuery.OrderBy(t => t.Amount);
                break;
            default: // date_desc
                baseQuery = baseQuery.OrderByDescending(t => t.TransactionCreatedAt);
                break;
        }

        var query = baseQuery
            .Select(t => new TransactionListDto
            {
                TransactionId  = t.TransactionId,
                StripeSessionId = t.StripeSessionId,
                Date           = t.TransactionCreatedAt,
                Amount         = t.InstructorPayouts.FirstOrDefault(p => p.InstructorId == instructorId)!.PayoutAmount,
                Currency       = t.Currency ?? "USD",
                PayoutCurrency = t.InstructorPayouts.Any(p => p.InstructorId == instructorId && p.IsPaid)
                    ? (t.OrderItem!.Course!.Instructor!.StripeCountry == "GB" ? "GBP" :
                       t.OrderItem!.Course!.Instructor!.StripeCountry == "VN" ? "VND" :
                       t.OrderItem!.Course!.Instructor!.StripeCountry == "AU" ? "AUD" :
                       t.OrderItem!.Course!.Instructor!.StripeCountry == "CA" ? "CAD" :
                       t.OrderItem!.Course!.Instructor!.StripeCountry == "SG" ? "SGD" : "USD")
                    : (t.Currency ?? "USD"),
                Status         = t.TransactionsStatus,

                BuyerName = t.AccountFromNavigation != null && t.AccountFromNavigation.User != null
                    ? t.AccountFromNavigation.User.FullName ?? "N/A"
                    : "N/A",

                CourseTitle = t.OrderItem != null && t.OrderItem.Course != null
                    ? t.OrderItem.Course.Title ?? "N/A"
                    : "N/A",

                InstructorName = "Me",

                RefundReason = t.RefundReason,
                RefundAdminNote = t.RefundAdminNote,
                RefundRequestedAt = t.RefundRequestedAt
            });

        var totalCount = await query.CountAsync();
        var skip = (page - 1) * pageSize;
        var items = await query.Skip(skip).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<TransactionListDto> Items, int TotalCount)> GetUserTransactionsAsync(int userId, int page, int pageSize, string? keyword = null, string? sortBy = "date_desc", string? status = null)
    {
        var baseQuery = _context.Transactions
            .Where(t => t.AccountFrom == userId);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var keywordLower = keyword.ToLower();
            baseQuery = baseQuery.Where(t =>
                (t.OrderItem != null && t.OrderItem.Course != null && t.OrderItem.Course.Title != null && t.OrderItem.Course.Title.ToLower().Contains(keywordLower))
            );
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusLower = status.ToLower();
            baseQuery = baseQuery.Where(t => t.TransactionsStatus != null && t.TransactionsStatus.ToLower() == statusLower);
        }
        else
        {
            baseQuery = baseQuery.Where(t => t.TransactionsStatus == "succeeded" || t.TransactionsStatus == "refund_pending" || t.TransactionsStatus == "refunded");
        }

        switch (sortBy?.ToLower())
        {
            case "date_asc":
                baseQuery = baseQuery.OrderBy(t => t.TransactionCreatedAt);
                break;
            case "amount_desc":
                baseQuery = baseQuery.OrderByDescending(t => t.Amount);
                break;
            case "amount_asc":
                baseQuery = baseQuery.OrderBy(t => t.Amount);
                break;
            default: // date_desc
                baseQuery = baseQuery.OrderByDescending(t => t.TransactionCreatedAt);
                break;
        }

        var query = baseQuery
            .Select(t => new TransactionListDto
            {
                TransactionId   = t.TransactionId,
                StripeSessionId = t.StripeSessionId,
                Date            = t.TransactionCreatedAt,
                Amount          = t.Amount,
                Currency        = t.Currency ?? "USD",
                Status          = t.TransactionsStatus,
                BuyerName       = "Me",
                CourseTitle     = t.OrderItem != null && t.OrderItem.Course != null ? t.OrderItem.Course.Title ?? "N/A" : "N/A",
                InstructorName  = t.OrderItem != null && t.OrderItem.Course != null && t.OrderItem.Course.Instructor != null && t.OrderItem.Course.Instructor.InstructorNavigation != null ? t.OrderItem.Course.Instructor.InstructorNavigation.FullName ?? "N/A" : "N/A",

                RefundReason = t.RefundReason,
                RefundAdminNote = t.RefundAdminNote,
                RefundRequestedAt = t.RefundRequestedAt
            });

        var totalCount = await query.CountAsync();
        var skip = (page - 1) * pageSize;
        var items = await query.Skip(skip).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }
}
