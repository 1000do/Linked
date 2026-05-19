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
    private readonly IPaymentGatewayService _paymentGateway;
    private readonly IInstructorRepository _instructorRepo;
    private readonly INotificationService _notiService;
    private readonly ILogger<AdminFinanceService> _logger;

    // Key trong bảng system_configs
    private const string TransferRateKey = "TransferRate";
    private const decimal DefaultTransferRate = 70.00m;

    public AdminFinanceService(
        IAdminFinanceRepository repo,
        IPaymentGatewayService paymentGateway,
        IInstructorRepository instructorRepo,
        INotificationService notiService,
        ILogger<AdminFinanceService> logger)
    {
        _repo = repo;
        _paymentGateway = paymentGateway;
        _instructorRepo = instructorRepo;
        _notiService = notiService;
        _logger = logger;
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
                "The revenue share rate must be between 1% and 100%.");

        await _repo.UpsertConfigAsync(
            TransferRateKey,
            rate.ToString("F2"),
            $"Instructor share rate: {rate}%, Platform share rate: {100 - rate}%");

        // ── UC-XXX: THÔNG BÁO CHO GIẢNG VIÊN ─────────────────────────────────
        // Khi thay đổi tỷ lệ, cần báo cho các đối tác đã liên kết Stripe
        // ──────────────────────────────────────────────────────────────────────
        try
        {
            var instructors = await _instructorRepo.GetInstructorsWithStripeAsync();
            
            if (instructors.Any())
            {
                var title = "📢 Revenue Share Policy Update";
                var content = $"The system has updated the revenue share rate. From now on, you will receive {rate:F0}% of the revenue from each course sold.";

                foreach (var ins in instructors)
                {
                    // ReceiverId ở đây tương ứng với User ID (trong DB này InstructorId = UserId)
                    await _notiService.SendNotificationAsync(
                        ins.InstructorId, 
                        title, 
                        content, 
                        "/Instructor/Payouts" // Link đến trang thu nhập để họ kiểm tra
                    );
                }

                _logger.LogInformation("✅ Sent TransferRate change notification ({Rate}%) to {Count} instructors.", rate, instructors.Count);
            }
        }
        catch (Exception ex)
        {
            // Không throw lỗi ở đây để tránh làm gián đoạn việc lưu config
            _logger.LogError(ex, "❌ Error sending TransferRate update notification.");
        }
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
            PayoutDate = p.PayoutDate,
            PayoutStatus = p.PayoutStatus ?? "pending",
            StripeTransferId = p.StripeTransferId,
            StripePayoutId = p.StripePayoutId,
            PaidToBankAt = p.PaidToBankAt
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
            throw new InvalidOperationException("This payment was not found.");

        if (payout.IsPaid)
            throw new InvalidOperationException("This payment has already been marked as Paid.");

        payout.IsPaid = true;
        // Có thể update PayoutDate = DateTime.Now nếu muốn ghi nhận ngày trả thực tế
        payout.PayoutDate = DateTime.Now;

        await _repo.SaveChangesAsync();
    }

    public async Task<string> PerformStripeTransferAsync(int payoutId)
    {
        var payout = await _repo.GetPayoutByIdAsync(payoutId);
        if (payout == null)
            throw new InvalidOperationException("Payment not found.");

        if (payout.IsPaid)
            throw new InvalidOperationException("This payment was previously paid.");

        if (payout.Instructor == null || string.IsNullOrEmpty(payout.Instructor.StripeAccountId))
            throw new InvalidOperationException("This instructor has not set up a Stripe Connect account.");

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
            try 
            {
                var requestOptions = new RequestOptions { StripeAccount = payout.Instructor.StripeAccountId };
                var chargeService = new ChargeService();
                var destinationCharge = await chargeService.GetAsync(transfer.DestinationPaymentId, null, requestOptions);
                if (destinationCharge != null)
                    payout.PayoutAmount = (decimal)destinationCharge.Amount / 100m;
            }
            catch { /* Nếu không có quyền xem charge, bỏ qua và giữ nguyên số tiền gốc */ }

            // ★ Cập nhật trạng thái: transferred (đã vào ví Stripe của GV, chờ về ngân hàng)
            payout.IsPaid = true;
            payout.PayoutStatus = "transferred";
            // ★ QUAN TRỌNG: Lưu DestinationPaymentId (py_xxx) thay vì TransferId (tr_xxx)
            // Vì tài khoản Connected Account chỉ nhìn thấy mã py_xxx trong lịch sử số dư của họ.
            payout.StripeTransferId = transfer.DestinationPaymentId; 
            payout.PayoutDate = DateTime.UtcNow;
            await _repo.SaveChangesAsync();

            return transfer.Id;
        }
        catch (StripeException ex)
        {
            // ★ Đánh dấu thất bại vào DB để Admin biết
            payout.PayoutStatus = "failed";
            await _repo.SaveChangesAsync();
            throw new InvalidOperationException($"Stripe Transfer error: {ex.Message}");
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
                result.Errors.Add($"Payout #{p.PayoutId} (Instructor: {p.InstructorName}): {ex.Message}");
            }
        }

        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PLATFORM WITHDRAWAL — Rút tiền lợi nhuận Sàn về ngân hàng
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<PlatformBalanceResponse> GetPlatformBalanceAsync()
    {
        // ★ Gọi Stripe Balance API để lấy số dư thực tế
        var balanceService = new BalanceService();
        var balance = await balanceService.GetAsync();

        // Lấy số dư USD (hoặc loại tiền đầu tiên nếu chỉ có 1)
        var available = balance.Available?.FirstOrDefault(b => b.Currency == "usd");
        var pending = balance.Pending?.FirstOrDefault(b => b.Currency == "usd");

        decimal availableAmount = available != null ? (decimal)available.Amount / 100m : 0m;
        decimal pendingAmount = pending != null ? (decimal)pending.Amount / 100m : 0m;

        // Lấy payout schedule hiện tại
        var accountService = new AccountService();
        // Platform account không có account ID, lấy info qua Retrieve
        string scheduleInterval = "manual";
        string? scheduleAnchor = null;

        try
        {
            var account = await accountService.GetSelfAsync();
            if (account?.Settings?.Payouts?.Schedule != null)
            {
                scheduleInterval = account.Settings.Payouts.Schedule.Interval ?? "manual";
                scheduleAnchor = account.Settings.Payouts.Schedule.WeeklyAnchor 
                    ?? account.Settings.Payouts.Schedule.MonthlyAnchor.ToString();
            }
        }
        catch { /* Bỏ qua nếu không có quyền đọc schedule */ }

        return new PlatformBalanceResponse
        {
            Available = availableAmount,
            Incoming = pendingAmount,
            Total = availableAmount + pendingAmount,
            Currency = "usd",
            PayoutScheduleInterval = scheduleInterval,
            PayoutScheduleAnchor = scheduleAnchor
        };
    }

    public async Task<WithdrawResponse> CreateWithdrawalAsync(WithdrawRequest request, int managerId)
    {
        // 1. Kiểm tra số dư
        var balanceResp = await GetPlatformBalanceAsync();
        decimal amountToWithdraw = (request.Amount.HasValue && request.Amount.Value > 0)
            ? request.Amount.Value
            : balanceResp.Available;

        if (amountToWithdraw <= 0)
            throw new InvalidOperationException("The withdrawal amount must be greater than 0.");

        if (amountToWithdraw > balanceResp.Available)
            throw new InvalidOperationException(
                $"Insufficient balance. Available: ${balanceResp.Available:F2}, Requested: ${amountToWithdraw:F2}");

        // 2. Tạo Payout trên Stripe (chuyển từ Stripe → ngân hàng Admin)
        var payoutService = new PayoutService();
        Payout stripePayout;
        try
        {
            stripePayout = await payoutService.CreateAsync(new PayoutCreateOptions
            {
                Amount = (long)(amountToWithdraw * 100), // Chuyển sang cents
                Currency = "usd",
                Description = request.Description ?? $"Platform withdrawal by Manager #{managerId}",
                Metadata = new Dictionary<string, string>
                {
                    { "manager_id", managerId.ToString() },
                    { "source", "web_dashboard" }
                }
            });
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException($"Stripe Payout error: {ex.Message}");
        }

        // 3. Lưu vào DB
        var withdrawal = new Domain.Entities.PlatformWithdrawal
        {
            ManagerId = managerId,
            Amount = amountToWithdraw,
            Currency = "usd",
            StripePayoutId = stripePayout.Id,
            Status = stripePayout.Status ?? "pending",
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddWithdrawalAsync(withdrawal);

        return new WithdrawResponse
        {
            WithdrawalId = withdrawal.WithdrawalId,
            StripePayoutId = stripePayout.Id,
            Amount = amountToWithdraw,
            Status = stripePayout.Status ?? "pending",
            CreatedAt = withdrawal.CreatedAt
        };
    }

    public async Task<List<WithdrawalHistoryItem>> GetWithdrawalHistoryAsync()
    {
        var withdrawals = await _repo.GetWithdrawalsAsync();

        bool hasChanges = false;
        var payoutService = new PayoutService();

        foreach (var w in withdrawals)
        {
            if (w.Status == "pending" || w.Status == "in_transit")
            {
                try
                {
                    var stripePayout = await payoutService.GetAsync(w.StripePayoutId);
                    if (stripePayout.Status != w.Status)
                    {
                        w.Status = stripePayout.Status;
                        if (stripePayout.Status == "paid")
                        {
                            w.ArrivedAt = stripePayout.ArrivalDate;
                        }
                        hasChanges = true;
                    }
                }
                catch
                {
                    // Ignore error during sync, keep old status
                }
            }
        }

        if (hasChanges)
        {
            await _repo.SaveChangesAsync();
        }

        return withdrawals.Select(w => new WithdrawalHistoryItem
        {
            WithdrawalId = w.WithdrawalId,
            ManagerName = w.Manager?.DisplayName ?? "Admin",
            Amount = w.Amount,
            Currency = w.Currency,
            StripePayoutId = w.StripePayoutId,
            Status = w.Status,
            Description = w.Description,
            CreatedAt = w.CreatedAt,
            ArrivedAt = w.ArrivedAt
        }).ToList();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // REFUND — Hoàn tiền toàn bộ cho 1 giao dịch
    //
    // ★ Business Flow:
    //   1. Validate: Giao dịch phải tồn tại, status = succeeded, chưa bị refund.
    //   2. Nếu đã Transfer cho GV → Reverse Transfer (lấy lại tiền từ GV).
    //   3. Tạo Stripe Refund → trả tiền về thẻ/ví khách hàng.
    //   4. Update DB: Transaction.status → refunded, InstructorPayout.status → refunded.
    //   5. Thu hồi Enrollment (xóa quyền truy cập khóa học).
    //
    // ★ Edge Cases:
    //   - GV đã rút tiền khỏi Stripe → Reverse Transfer FAIL → throw lỗi.
    //   - Giao dịch chưa có PaymentIntentId → không thể refund.
    //   - Giao dịch đã refund trước đó → từ chối.
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<RefundResultResponse> RefundTransactionAsync(int transactionId, string? reason = null)
    {
        // ── 1. LOAD & VALIDATE ──────────────────────────────────────────
        var txn = await _repo.GetTransactionWithFullGraphAsync(transactionId);
        if (txn == null)
            throw new InvalidOperationException($"Transaction #{transactionId} not found.");

        if (txn.TransactionsStatus == "refunded")
            throw new InvalidOperationException("This transaction was previously refunded.");

        if (txn.TransactionsStatus != "succeeded")
            throw new InvalidOperationException(
                $"Only successful transactions can be refunded. Current status: {txn.TransactionsStatus}.");

        if (string.IsNullOrEmpty(txn.StripePaymentintentId))
            throw new InvalidOperationException(
                "This transaction does not have a PaymentIntent ID — cannot refund via Stripe.");

        _logger.LogInformation(
            "🔄 REFUND START | TxnId={TxnId} | Amount={Amount} {Currency} | PI={PI}",
            transactionId, txn.Amount, txn.Currency, txn.StripePaymentintentId);

        var result = new RefundResultResponse { RefundedAmount = txn.Amount };
        string? reversalId = null;

        // ── 2. REVERSE STRIPE TRANSFER (nếu đã chuyển cho GV) ───────────
        var payout = txn.InstructorPayouts.FirstOrDefault();
        if (payout != null && payout.IsPaid && !string.IsNullOrEmpty(payout.StripeTransferId))
        {
            _logger.LogInformation(
                "🔄 Reversing transfer for GV | PayoutId={PId} | DestPaymentId={DPI}",
                payout.PayoutId, payout.StripeTransferId);

            // Resolve py_xxx → tr_xxx (Stripe Transfer Reversal yêu cầu transfer ID gốc)
            var stripeTransferId = await _repo.GetStripeTransferIdByDestinationPaymentAsync(payout.StripeTransferId);
            if (string.IsNullOrEmpty(stripeTransferId))
                throw new InvalidOperationException(
                    $"Cannot find original Stripe Transfer ID for DestPaymentId={payout.StripeTransferId}. " +
                    "Please handle manually on the Stripe Dashboard.");

            try
            {
                reversalId = await _paymentGateway.ReverseTransferAsync(stripeTransferId);
                _logger.LogInformation("✅ Transfer reversed | ReversalId={RId}", reversalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Transfer reversal FAILED");
                throw new InvalidOperationException(
                    $"Cannot claw back money from the instructor: {ex.Message}. " +
                    "The instructor may have already withdrawn all funds from Stripe.");
            }
        }

        // ── 3. CREATE STRIPE REFUND ─────────────────────────────────────
        string refundId;
        try
        {
            refundId = await _paymentGateway.RefundAsync(txn.StripePaymentintentId, reason);
            _logger.LogInformation("✅ Stripe Refund created | RefundId={RfId}", refundId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Stripe Refund FAILED");
            throw new InvalidOperationException($"Stripe refund error: {ex.Message}");
        }

        // ── 4. UPDATE DB STATUS ─────────────────────────────────────────
        txn.TransactionsStatus = "refunded";

        if (payout != null)
        {
            payout.PayoutStatus = "refunded";
            payout.IsPaid = false;
        }

        // ── 5. REVOKE ENROLLMENT ────────────────────────────────────────
        bool enrollmentRevoked = false;
        var buyerUserId = txn.AccountFromNavigation?.User?.UserId;
        var courseId = txn.OrderItem?.CourseId;

        if (buyerUserId.HasValue && courseId.HasValue)
        {
            var enrollment = await _repo.GetActiveEnrollmentAsync(buyerUserId.Value, courseId.Value);
            if (enrollment != null)
            {
                enrollment.EnrollmentStatus = "revoked";
                enrollmentRevoked = true;
                _logger.LogInformation(
                    "🚫 Enrollment revoked | UserId={UID} | CourseId={CID}",
                    buyerUserId.Value, courseId.Value);
            }
        }

        // ── 6. COMMIT ───────────────────────────────────────────────────
        await _repo.SaveChangesAsync();

        _logger.LogInformation(
            "✅ REFUND COMPLETE | TxnId={TxnId} | RefundId={RfId} | ReversalId={RvId} | EnrollRevoked={ER}",
            transactionId, refundId, reversalId, enrollmentRevoked);

        result.StripeRefundId = refundId;
        result.StripeReversalId = reversalId;
        result.EnrollmentRevoked = enrollmentRevoked;
        return result;
    }
}
