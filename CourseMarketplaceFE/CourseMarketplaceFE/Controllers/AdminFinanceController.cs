using System.Text.Json;
using CourseMarketplaceFE.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers;

/// <summary>
/// FE MVC Controller cho Dashboard Tài chính Admin.
/// Dùng ApiClient gọi BE API, tự động forward JWT Cookie.
/// </summary>
public class AdminFinanceController : Controller
{
    private readonly ApiClient _api;
    private readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

    public AdminFinanceController(ApiClient api) => _api = api;

    // ─── Wrapper models cho BE ApiResponse<T> ────────────────────────────
    private class ApiResp<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    // ─── DTOs (mirror BE DTOs) ───────────────────────────────────────────

    public class FinancialSummaryVM
    {
        public decimal GrossRevenue { get; set; }
        public decimal TotalPaidOut { get; set; }
        public decimal PendingEscrow { get; set; }
        public decimal MaturedEscrow { get; set; }
        public decimal PlatformNetProfit { get; set; }
        public decimal CurrentTransferRate { get; set; }
        public int TotalTransactions { get; set; }
    }

    public class PayoutDetailVM
    {
        public int PayoutId { get; set; }
        public int TransactionId { get; set; }
        public string InstructorName { get; set; } = "";
        public string InstructorEmail { get; set; } = "";
        public string CourseTitle { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public decimal InstructorReceived { get; set; }
        public decimal PlatformReceived { get; set; }
        public decimal TransferRate { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime PayoutDate { get; set; }

        // ★ Webhook tracking fields
        /// <summary>pending | transferred | in_transit | paid | failed</summary>
        public string? PayoutStatus { get; set; }
        public string? StripeTransferId { get; set; }
        public string? StripePayoutId { get; set; }
        public DateTime? PaidToBankAt { get; set; }
    }

    public class FinanceDashboardVM
    {
        public FinancialSummaryVM Summary { get; set; } = new();
        public List<PayoutDetailVM> Payouts { get; set; } = new();
    }

    public class PlatformBalanceVM
    {
        public decimal Available { get; set; }
        public decimal Incoming { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; } = "usd";
        public string PayoutScheduleInterval { get; set; } = "manual";
        public string? PayoutScheduleAnchor { get; set; }
    }

    public class WithdrawalHistoryVM
    {
        public int WithdrawalId { get; set; }
        public string? ManagerName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string? StripePayoutId { get; set; }
        public string Status { get; set; } = "pending";
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ArrivedAt { get; set; }
    }

    public class WithdrawPageVM
    {
        public PlatformBalanceVM Balance { get; set; } = new();
        public List<WithdrawalHistoryVM> History { get; set; } = new();
    }

    public class PayoutPagedVM
    {
        public List<PayoutDetailVM> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class WithdrawalPagedVM
    {
        public List<WithdrawalHistoryVM> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class RefundRequestVM
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? RefundReason { get; set; }
        public DateTime? RefundRequestedAt { get; set; }
        public DateTime? TransactionCreatedAt { get; set; }

        public OrderItemDto? OrderItem { get; set; }
        public AccountDto? AccountFromNavigation { get; set; }

        public string CourseTitle => OrderItem?.Course?.Title ?? "N/A";
        public string BuyerName => AccountFromNavigation?.User?.FullName ?? "N/A";
    }

    public class OrderItemDto
    {
        public CourseDto? Course { get; set; }
    }

    public class CourseDto
    {
        public string Title { get; set; } = "";
    }

    public class AccountDto
    {
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public string FullName { get; set; } = "";
    }

    public class InstructorCourseRevenueResponse
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
        public decimal LifetimeRevenue { get; set; }
    }

    public class AdminFinanceUnifiedVM
    {
        public FinanceDashboardVM Dashboard { get; set; } = new();
        public WithdrawPageVM Withdraw { get; set; } = new();
        public CourseMarketplaceFE.Controllers.TransactionController.TransactionPagedVM Transactions { get; set; } = new();
        public PayoutPagedVM Payouts { get; set; } = new();
        public WithdrawalPagedVM Withdrawals { get; set; } = new();
        public List<RefundRequestVM> PendingRefunds { get; set; } = new();
        public string PayoutDays { get; set; } = "15";
        public List<InstructorCourseRevenueResponse> CourseRevenues { get; set; } = new();
        public Dictionary<int, FinancialSummaryVM> YearlySummaries { get; set; } = new();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /AdminFinance
    // Dashboard chính — gọi 2 API song song: summary + payouts
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = "date_desc", string? status = null, string tab = "tx", int? year = null, int? month = null, int payoutPage = 1, int withdrawPage = 1)
    {
        var vm = new AdminFinanceUnifiedVM();
        vm.Transactions.Page = page;
        vm.Transactions.PageSize = pageSize;

        // Default to current UTC month/year if not provided
        int selectedYear = year ?? DateTime.UtcNow.Year;
        int selectedMonth = month ?? DateTime.UtcNow.Month;

        ViewBag.Keyword = keyword;
        ViewBag.SortBy = sortBy;
        ViewBag.Status = status;
        ViewBag.ActiveTab = tab;
        ViewBag.Year = selectedYear;
        ViewBag.Month = selectedMonth;
        ViewBag.PayoutPage = payoutPage;
        ViewBag.WithdrawPage = withdrawPage;

        var txQuery = $"transactions?page={page}&pageSize={pageSize}&year={selectedYear}&month={selectedMonth}";
        if (!string.IsNullOrEmpty(keyword)) txQuery += $"&keyword={Uri.EscapeDataString(keyword)}";
        if (!string.IsNullOrEmpty(sortBy)) txQuery += $"&sortBy={Uri.EscapeDataString(sortBy)}";
        if (!string.IsNullOrEmpty(status)) txQuery += $"&status={Uri.EscapeDataString(status)}";

        // Gọi song song 7 API để giảm latency
        var summaryTask = _api.GetAsync($"admin/finance/summary?year={selectedYear}&month={selectedMonth}");
        var payoutsTask = _api.GetAsync($"admin/finance/payouts?year={selectedYear}&month={selectedMonth}");
        var balanceTask = _api.GetAsync("admin/finance/balance");
        var historyTask = _api.GetAsync($"admin/finance/withdrawals?year={selectedYear}&month={selectedMonth}");
        var txTask = _api.GetAsync(txQuery);
        var refundTask = _api.GetAsync("admin/finance/refunds/pending");
        var payoutDaysTask = _api.GetAsync("admin/finance/payout-days");

        await Task.WhenAll(summaryTask, payoutsTask, balanceTask, historyTask, txTask, refundTask, payoutDaysTask);

        // Parse payout days
        var payoutDaysResp = await payoutDaysTask;
        if (payoutDaysResp.IsSuccessStatusCode)
        {
            var json = await payoutDaysResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<string>>(json, _jsonOpts);
            if (parsed?.Data != null)
                vm.PayoutDays = parsed.Data;
        }

        // Parse summary
        var summaryResp = await summaryTask;
        if (summaryResp.IsSuccessStatusCode)
        {
            var json = await summaryResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<FinancialSummaryVM>>(json, _jsonOpts);
            if (parsed?.Data != null)
                vm.Dashboard.Summary = parsed.Data;
        }

        // Parse payouts
        var payoutsResp = await payoutsTask;
        if (payoutsResp.IsSuccessStatusCode)
        {
            var json = await payoutsResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<List<PayoutDetailVM>>>(json, _jsonOpts);
            if (parsed?.Data != null)
                vm.Dashboard.Payouts = parsed.Data;
        }

        // Parse balance
        var balanceResp = await balanceTask;
        if (balanceResp.IsSuccessStatusCode)
        {
            var json = await balanceResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<PlatformBalanceVM>>(json, _jsonOpts);
            if (parsed?.Data != null)
                vm.Withdraw.Balance = parsed.Data;
        }

        // Parse history
        var historyResp = await historyTask;
        if (historyResp.IsSuccessStatusCode)
        {
            var json = await historyResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<List<WithdrawalHistoryVM>>>(json, _jsonOpts);
            if (parsed?.Data != null)
                vm.Withdraw.History = parsed.Data;
        }

        // Parse transactions
        var txResp = await txTask;
        if (txResp.IsSuccessStatusCode)
        {
            var json = await txResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<CourseMarketplaceFE.Controllers.TransactionController.TransactionPagedVM>>(json, _jsonOpts);
            if (parsed?.Data != null)
                vm.Transactions = parsed.Data;
        }

        // Parse pending refunds
        var refundResp = await refundTask;
        if (refundResp.IsSuccessStatusCode)
        {
            var json = await refundResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<List<RefundRequestVM>>>(json, _jsonOpts);
            if (parsed?.Data != null)
                vm.PendingRefunds = parsed.Data;
        }

        // Slice & Paginate Payouts
        int payoutPageSize = 10;
        var allPayouts = vm.Dashboard.Payouts;
        vm.Payouts.TotalCount = allPayouts.Count;
        vm.Payouts.Page = payoutPage;
        vm.Payouts.PageSize = payoutPageSize;
        vm.Payouts.TotalPages = (int)Math.Ceiling((double)allPayouts.Count / payoutPageSize);
        vm.Payouts.Items = allPayouts.Skip((payoutPage - 1) * payoutPageSize).Take(payoutPageSize).ToList();

        // Slice & Paginate Withdrawals
        int withdrawPageSize = 10;
        var allWithdrawals = vm.Withdraw.History;
        vm.Withdrawals.TotalCount = allWithdrawals.Count;
        vm.Withdrawals.Page = withdrawPage;
        vm.Withdrawals.PageSize = withdrawPageSize;
        vm.Withdrawals.TotalPages = (int)Math.Ceiling((double)allWithdrawals.Count / withdrawPageSize);
        vm.Withdrawals.Items = allWithdrawals.Skip((withdrawPage - 1) * withdrawPageSize).Take(withdrawPageSize).ToList();

        return View(vm);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /AdminFinance/SetTransferRate
    // Cập nhật tỷ lệ chia sẻ doanh thu
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetTransferRate(decimal rate)
    {
        var response = await _api.PostJsonAsync("admin/finance/transfer-rate", new { Rate = rate });

        if (response.IsSuccessStatusCode)
        {
            TempData["FinanceSuccess"] = $"✅ Updated: Instructor gets {rate}%, Platform gets {100 - rate}%.";
        }
        else
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(errorBody);
                if (doc.RootElement.TryGetProperty("message", out var m))
                    errorBody = m.GetString() ?? errorBody;
            }
            catch { }
            TempData["FinanceError"] = $"❌ Error: {errorBody}";
        }

        return RedirectToAction(nameof(Index));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /AdminFinance/SetPayoutDays
    // Cập nhật ngày thanh toán tự động định kỳ
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPayoutDays(string payoutDays)
    {
        var response = await _api.PostJsonAsync("admin/finance/payout-days", new { PayoutDays = payoutDays });

        if (response.IsSuccessStatusCode)
        {
            TempData["FinanceSuccess"] = $"✅ Updated automated payout days to: {payoutDays}.";
        }
        else
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(errorBody);
                if (doc.RootElement.TryGetProperty("message", out var m))
                    errorBody = m.GetString() ?? errorBody;
            }
            catch { }
            TempData["FinanceError"] = $"❌ Error: {errorBody}";
        }

        return RedirectToAction(nameof(Index));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /AdminFinance/MarkAsPaid/{payoutId}
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsPaid(int payoutId)
    {
        var response = await _api.PostAsync($"admin/finance/payouts/{payoutId}/mark-paid");

        if (response.IsSuccessStatusCode)
        {
            TempData["FinanceSuccess"] = "✅ Marked as paid successfully.";
        }
        else
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            TempData["FinanceError"] = $"❌ Error: {errorBody}";
        }

        return RedirectToAction(nameof(Index));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /AdminFinance/PayViaStripe/{payoutId}
    // Chuyển tiền thật qua Stripe Connect API
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayViaStripe(int payoutId)
    {
        var response = await _api.PostAsync($"admin/finance/payouts/{payoutId}/stripe-transfer");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var transferId = doc.RootElement.GetProperty("data").GetString();
            TempData["FinanceSuccess"] = $"✅ Money transferred successfully via Stripe! (TX ID: {transferId})";
        }
        else
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            // Parse lỗi nếu là JSON ApiResponse
            try
            {
                using var doc = JsonDocument.Parse(errorBody);
                if (doc.RootElement.TryGetProperty("message", out var m))
                    errorBody = m.GetString();
            }
            catch { }
            
            TempData["FinanceError"] = $"❌ Stripe Error: {errorBody}";
        }

        return RedirectToAction(nameof(Index));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // TRANG RÚT TIỀN SÀN
    // GET /AdminFinance/Withdrawals — Hiển thị số dư + lịch sử rút
    // POST /AdminFinance/Withdraw — Tạo lệnh rút
    // ═══════════════════════════════════════════════════════════════════════



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(decimal? amount, string? description)
    {
        var response = await _api.PostJsonAsync("admin/finance/withdraw", new
        {
            Amount = amount,
            Description = description
        });

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            TempData["WithdrawSuccess"] = "✅ Withdrawal request created successfully! Funds will arrive in your bank in 1-3 days.";
        }
        else
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(errorBody);
                if (doc.RootElement.TryGetProperty("message", out var m))
                    errorBody = m.GetString();
            }
            catch { }

            TempData["WithdrawError"] = $"❌ Error: {errorBody}";
        }

        return RedirectToAction(nameof(Index));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /AdminFinance/Refund/{transactionId}
    // Hoàn tiền cho 1 giao dịch qua Stripe
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(int transactionId, string? reason)
    {
        var response = await _api.PostJsonAsync($"admin/finance/transactions/{transactionId}/refund", new
        {
            Reason = reason
        });

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");

            var refundId = data.GetProperty("stripeRefundId").GetString();
            var amount = data.GetProperty("refundedAmount").GetDecimal();
            var enrollRevoked = data.GetProperty("enrollmentRevoked").GetBoolean();

            TempData["FinanceSuccess"] = $"✅ Refund successful! " +
                $"Stripe Refund: {refundId} | " +
                $"Amount: ${amount:F2} | " +
                $"Access revoked: {(enrollRevoked ? "Yes" : "N/A")}";
        }
        else
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(errorBody);
                if (doc.RootElement.TryGetProperty("message", out var m))
                    errorBody = m.GetString();
            }
            catch { }

            TempData["FinanceError"] = $"❌ Refund Error: {errorBody}";
        }

        return RedirectToAction("Detail", "Transaction", new { id = transactionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncAllPayouts()
    {
        var response = await _api.PostAsync("admin/finance/payouts/sync-all");

        if (response.IsSuccessStatusCode)
        {
            TempData["FinanceSuccess"] = "✅ Successfully synced all Stripe Connect payouts with their bank arrival statuses!";
        }
        else
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(errorBody);
                if (doc.RootElement.TryGetProperty("message", out var m))
                    errorBody = m.GetString();
            }
            catch { }

            TempData["FinanceError"] = $"❌ Sync Error: {errorBody}";
        }

        return RedirectToAction(nameof(Index), new { tab = "po" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveRefundDecision(int transactionId, string? adminNote)
    {
        var response = await _api.PostJsonAsync($"admin/finance/refunds/{transactionId}/approve", new
        {
            AdminNote = adminNote ?? "Approved by Admin."
        });

        if (response.IsSuccessStatusCode)
        {
            return Json(new { success = true, message = "Refund approved successfully via Stripe!" });
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        var message = "Error approving refund request.";
        try
        {
            using var doc = JsonDocument.Parse(errorBody);
            if (doc.RootElement.TryGetProperty("message", out var m))
                message = m.GetString() ?? message;
        }
        catch { }

        return Json(new { success = false, message = message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectRefundDecision(int transactionId, string? adminNote)
    {
        var response = await _api.PostJsonAsync($"admin/finance/refunds/{transactionId}/reject", new
        {
            AdminNote = adminNote ?? "Rejected by Admin."
        });

        if (response.IsSuccessStatusCode)
        {
            return Json(new { success = true, message = "Refund request successfully rejected." });
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        var message = "Error rejecting refund request.";
        try
        {
            using var doc = JsonDocument.Parse(errorBody);
            if (doc.RootElement.TryGetProperty("message", out var m))
                message = m.GetString() ?? message;
        }
        catch { }

        return Json(new { success = false, message = message });
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /AdminFinance/InstructorRevenues
    // Dedicated page for Instructor Course Revenues with KPIs, search, period filter, sorting
    // ═══════════════════════════════════════════════════════════════════════
    public class InstructorCourseRevenuesPageVM
    {
        public List<InstructorCourseRevenueResponse> Revenues { get; set; } = new();
        public int TotalActiveCourses { get; set; }
        public string TopSellingCourseTitle { get; set; } = "N/A";
        public int TopSellingCourseSales { get; set; }
        public string HighestEarningCourseTitle { get; set; } = "N/A";
        public decimal HighestEarningCourseRevenue { get; set; }

        public Dictionary<int, decimal[]> YearlyMonthlyRevenue { get; set; } = new();
        public decimal TotalGrossRevenue { get; set; }
        public int TotalCoursesSold { get; set; }
        public decimal PlatformNetProfit { get; set; }
        public int TotalNewLearners { get; set; }
    }

    [HttpGet]
    public async Task<IActionResult> InstructorRevenues(int? year = null, int? month = null, string? keyword = null, string? sortBy = "sales_desc")
    {
        int selectedYear = year ?? DateTime.UtcNow.Year;
        int selectedMonth = month ?? DateTime.UtcNow.Month;

        ViewBag.Year = selectedYear;
        ViewBag.Month = selectedMonth;
        ViewBag.Keyword = keyword;
        ViewBag.SortBy = sortBy;

        var vm = new InstructorCourseRevenuesPageVM();

        var revTask = _api.GetAsync($"admin/finance/instructor-courses-revenue?year={selectedYear}&month={selectedMonth}");
        var summaryTask = _api.GetAsync($"admin/finance/summary?year={selectedYear}&month={selectedMonth}");
        var txTask = _api.GetAsync($"transactions?page=1&pageSize=1000&year={selectedYear}&month={selectedMonth}");
        var payoutsTask = _api.GetAsync("admin/finance/payouts");

        await Task.WhenAll(revTask, summaryTask, txTask, payoutsTask);

        var revResp = await revTask;
        if (revResp.IsSuccessStatusCode)
        {
            var json = await revResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<List<InstructorCourseRevenueResponse>>>(json, _jsonOpts);
            if (parsed?.Data != null)
            {
                var allRevenues = parsed.Data;

                // 1. Calculate KPIs before filtering
                vm.TotalActiveCourses = allRevenues.Count;
                if (allRevenues.Any())
                {
                    var topSelling = allRevenues.OrderByDescending(r => r.SalesCount).First();
                    vm.TopSellingCourseTitle = topSelling.CourseTitle;
                    vm.TopSellingCourseSales = topSelling.SalesCount;

                    var highestEarning = allRevenues.OrderByDescending(r => r.LifetimeRevenue).First();
                    vm.HighestEarningCourseTitle = highestEarning.CourseTitle;
                    vm.HighestEarningCourseRevenue = highestEarning.LifetimeRevenue;
                }

                // 2. Perform search/filter
                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.ToLower();
                    allRevenues = allRevenues.Where(r => 
                        r.CourseTitle.ToLower().Contains(keyword) || 
                        r.InstructorName.ToLower().Contains(keyword)
                    ).ToList();
                }

                // 3. Perform sorting
                allRevenues = sortBy switch
                {
                    "sales_desc" => allRevenues.OrderByDescending(r => r.SalesCount).ToList(),
                    "sales_asc" => allRevenues.OrderBy(r => r.SalesCount).ToList(),
                    "lifetime_desc" => allRevenues.OrderByDescending(r => r.LifetimeRevenue).ToList(),
                    "lifetime_asc" => allRevenues.OrderBy(r => r.LifetimeRevenue).ToList(),
                    "monthly_desc" => allRevenues.OrderByDescending(r => r.MonthlyRevenue).ToList(),
                    "monthly_asc" => allRevenues.OrderBy(r => r.MonthlyRevenue).ToList(),
                    "yearly_desc" => allRevenues.OrderByDescending(r => r.YearlyRevenue).ToList(),
                    "yearly_asc" => allRevenues.OrderBy(r => r.YearlyRevenue).ToList(),
                    _ => allRevenues.OrderByDescending(r => r.SalesCount).ToList()
                };

                vm.Revenues = allRevenues;
            }
        }
        else if (revResp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return RedirectToAction("Login", "Account");
        }

        var summaryResp = await summaryTask;
        if (summaryResp.IsSuccessStatusCode)
        {
            var json = await summaryResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<FinancialSummaryVM>>(json, _jsonOpts);
            if (parsed?.Data != null)
            {
                vm.TotalGrossRevenue = parsed.Data.GrossRevenue;
                vm.TotalCoursesSold = parsed.Data.TotalTransactions;
                vm.PlatformNetProfit = parsed.Data.PlatformNetProfit;
            }
        }

        var txResp = await txTask;
        if (txResp.IsSuccessStatusCode)
        {
            var json = await txResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<CourseMarketplaceFE.Controllers.TransactionController.TransactionPagedVM>>(json, _jsonOpts);
            if (parsed?.Data?.Items != null)
            {
                vm.TotalNewLearners = parsed.Data.Items.Select(x => x.BuyerName).Distinct().Count();
            }
        }

        var payoutsResp = await payoutsTask;
        if (payoutsResp.IsSuccessStatusCode)
        {
            var json = await payoutsResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<List<PayoutDetailVM>>>(json, _jsonOpts);
            if (parsed?.Data != null)
            {
                var allPayouts = parsed.Data;
                var yearlyMonthlyData = new Dictionary<int, decimal[]>();
                int[] targetYears = { selectedYear, selectedYear - 1, selectedYear - 2 };

                foreach (var y in targetYears)
                {
                    var monthlyArray = new decimal[12];
                    var yearPayouts = allPayouts.Where(p => p.TransactionDate.HasValue && p.TransactionDate.Value.Year == y && p.PayoutStatus != "refunded").ToList();
                    
                    for (int m = 1; m <= 12; m++)
                    {
                        monthlyArray[m - 1] = yearPayouts.Where(p => p.TransactionDate.Value.Month == m).Sum(p => p.TotalAmount);
                    }
                    
                    yearlyMonthlyData[y] = monthlyArray;
                }
                vm.YearlyMonthlyRevenue = yearlyMonthlyData;
            }
        }

        return View(vm);
    }
}
