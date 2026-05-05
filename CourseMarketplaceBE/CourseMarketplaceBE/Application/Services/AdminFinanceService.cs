using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.IRepositories;
using Stripe;

namespace CourseMarketplaceBE.Application.Services;

/// <summary>
/// SOLID — SRP: Class CHỈ điều phối nghiệp vụ Admin Finance.
///   - Không biết EF Core (dùng IAdminFinanceRepository — DIP).
///   - Không xử lý HTTP (Controller lo — SRP).
///
/// ★ Business Logic:
///   TransferRate = % giảng viên nhận (lưu trong system_configs)
///   PlatformFee  = 100 - TransferRate (% sàn ăn)
///
///   Ví dụ: TransferRate = 70
///     → Khách trả $100 → Giảng viên nhận $70, Sàn ăn $30
///     → PlatformNetProfit = SUM(amount) - SUM(payout_amount)
/// </summary>
public class AdminFinanceService : IAdminFinanceService
{
    private readonly IAdminFinanceRepository _repo;

    // Key trong bảng system_configs
    private const string TransferRateKey = "TransferRate";
    private const decimal DefaultTransferRate = 70.00m;

    public AdminFinanceService(IAdminFinanceRepository repo)
    {
        _repo = repo;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // UC-120: CẬP NHẬT TỶ LỆ CHIA SẺ DOANH THU
    //
    // Validate: 1 ≤ rate ≤ 100
    // Upsert vào system_configs.TransferRate
    // ★ Lưu ý: Chỉ ảnh hưởng giao dịch MỚI.
    //   Giao dịch cũ đã lưu transfer_rate trong bảng transactions.
    // ═══════════════════════════════════════════════════════════════════════
    public async Task SetTransferRateAsync(decimal rate)
    {
        if (rate < 1 || rate > 100)
            throw new InvalidOperationException(
                "Tỷ lệ chia sẻ phải từ 1% đến 100%.");

        await _repo.UpsertConfigAsync(
            TransferRateKey,
            rate.ToString("F2"),
            $"Tỷ lệ giảng viên nhận: {rate}%, Sàn nhận: {100 - rate}%");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // UC-112: TỔNG QUAN TÀI CHÍNH HỆ THỐNG
    //
    // Công thức (tất cả tính trên DB bằng SUM):
    //   GrossRevenue      = SUM(transactions.amount)  WHERE succeeded
    //   TotalPaidOut       = SUM(payouts.payout_amount) WHERE is_paid = true
    //   PendingEscrow      = SUM(payouts.payout_amount) WHERE is_paid = false
    //   PlatformNetProfit  = GrossRevenue - TotalPaidOut - PendingEscrow
    //                      = Tiền sàn THỰC SỰ giữ được
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<FinancialSummaryResponse> GetFinancialSummaryAsync()
    {
        var grossRevenue = await _repo.GetGrossRevenueAsync();
        var totalPaidOut = await _repo.GetTotalPaidOutAsync();
        var pendingEscrow = await _repo.GetPendingEscrowAsync();
        var totalTransactions = await _repo.GetSucceededTransactionCountAsync();
        var currentRate = await GetCurrentTransferRateAsync();

        return new FinancialSummaryResponse
        {
            GrossRevenue = grossRevenue,
            TotalPaidOut = totalPaidOut,
            PendingEscrow = pendingEscrow,
            // ★ Net Profit = Tổng thu - Tổng đã trả GV - Tổng đang giữ hộ GV
            PlatformNetProfit = grossRevenue - totalPaidOut - pendingEscrow,
            CurrentTransferRate = currentRate,
            TotalTransactions = totalTransactions
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // DANH SÁCH CHIA TIỀN GIẢNG VIÊN
    //
    // JOIN: instructor_payouts → transactions → order_items → courses
    //       instructor_payouts → instructors → users
    //
    // PlatformReceived = TotalAmount - InstructorReceived
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<List<PayoutDetailResponse>> GetInstructorPayoutsAsync()
    {
        var projections = await _repo.GetPayoutDetailsAsync();

        return projections.Select(p => new PayoutDetailResponse
        {
            PayoutId = p.PayoutId,
            TransactionId = p.TransactionId,
            InstructorName = p.InstructorName,
            InstructorEmail = p.InstructorEmail,
            CourseTitle = p.CourseTitle,
            TotalAmount = p.TotalAmount,
            InstructorReceived = p.InstructorReceived,
            // ★ Sàn nhận = Tổng tiền khách trả - Giảng viên nhận
            PlatformReceived = p.TotalAmount - p.InstructorReceived,
            TransferRate = p.TransferRate,
            IsPaid = p.IsPaid,
            TransactionDate = p.TransactionDate,
            PayoutDate = p.PayoutDate
        }).ToList();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // LẤY TỶ LỆ CHIA SẺ HIỆN TẠI
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<decimal> GetCurrentTransferRateAsync()
    {
        var rateStr = await _repo.GetConfigValueAsync(TransferRateKey);
        return decimal.TryParse(rateStr, out var rate) ? rate : DefaultTransferRate;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // MARK PAYOUT AS PAID (Manual payout confirmation)
    // ═══════════════════════════════════════════════════════════════════════
    public async Task MarkPayoutAsPaidAsync(int payoutId)
    {
        var payout = await _repo.GetPayoutByIdAsync(payoutId);
        if (payout == null)
            throw new InvalidOperationException("Không tìm thấy khoản thanh toán này.");

        if (payout.IsPaid)
            throw new InvalidOperationException("Khoản thanh toán này đã được đánh dấu là Đã trả.");

        payout.IsPaid = true;
        // Có thể update PayoutDate = DateTime.Now nếu muốn ghi nhận ngày trả thực tế
        payout.PayoutDate = DateTime.Now;

        await _repo.SaveChangesAsync();
    }

    public async Task<string> PerformStripeTransferAsync(int payoutId)
    {
        var payout = await _repo.GetPayoutByIdAsync(payoutId);
        if (payout == null)
            throw new InvalidOperationException("Không tìm thấy khoản thanh toán.");

        if (payout.IsPaid)
            throw new InvalidOperationException("Khoản thanh toán này đã được trả trước đó.");

        if (payout.Instructor == null || string.IsNullOrEmpty(payout.Instructor.StripeAccountId))
            throw new InvalidOperationException("Giảng viên này chưa thiết lập tài khoản Stripe Connect.");

        try
        {
            var transferService = new TransferService();
            var transfer = await transferService.CreateAsync(new TransferCreateOptions
            {
                Amount = (long)(payout.PayoutAmount * 100), 
                Currency = payout.Transaction?.Currency?.ToLower() ?? "usd",
                Destination = payout.Instructor.StripeAccountId,
                Description = $"Payout for PayoutId #{payoutId}",
                Metadata = new Dictionary<string, string> { { "payout_id", payoutId.ToString() } }
            });

            // ★ Lấy thông tin số tiền thực tế giảng viên nhận được (sau khi Stripe chuyển đổi ngoại tệ)
            // Lưu ý: Cần quyền truy cập vào Connected Account để đọc Charge
            try 
            {
                var requestOptions = new RequestOptions { StripeAccount = payout.Instructor.StripeAccountId };
                var chargeService = new ChargeService();
                var destinationCharge = await chargeService.GetAsync(transfer.DestinationPaymentId, null, requestOptions);
                
                if (destinationCharge != null)
                {
                    // Cập nhật lại số tiền và có thể ghi chú loại tiền vào DB
                    // Vì DB hiện tại chỉ có 1 cột PayoutAmount (decimal), ta sẽ lưu số tiền đã quy đổi.
                    payout.PayoutAmount = (decimal)destinationCharge.Amount / 100m;
                    // Nếu có cột Currency trong Payout thì tốt, nếu không ta lưu vào một field tạm hoặc ghi log.
                }
            }
            catch { /* Thu thập lỗi nếu không có quyền xem charge, nhưng vẫn tiếp tục update trạng thái */ }

            // Update DB
            payout.IsPaid = true;
            payout.PayoutDate = DateTime.Now;
            await _repo.SaveChangesAsync();

            return transfer.Id;
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException($"Lỗi Stripe Transfer: {ex.Message}");
        }
    }

    public async Task<BulkPayoutResult> BulkPayAllViaStripeAsync()
    {
        var pendingPayouts = await _repo.GetPayoutDetailsAsync();
        var toProcess = pendingPayouts.Where(p => !p.IsPaid).ToList();
        
        var result = new BulkPayoutResult { TotalProcessed = toProcess.Count };

        foreach (var p in toProcess)
        {
            try
            {
                await PerformStripeTransferAsync(p.PayoutId);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailCount++;
                result.Errors.Add($"Payout #{p.PayoutId} (GV: {p.InstructorName}): {ex.Message}");
            }
        }

        return result;
    }
}
