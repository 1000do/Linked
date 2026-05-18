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

    public class AdminFinanceUnifiedVM
    {
        public FinanceDashboardVM Dashboard { get; set; } = new();
        public WithdrawPageVM Withdraw { get; set; } = new();
        public CourseMarketplaceFE.Controllers.TransactionController.TransactionPagedVM Transactions { get; set; } = new();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /AdminFinance
    // Dashboard chính — gọi 2 API song song: summary + payouts
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = "date_desc", string? status = null, string tab = "tx")
    {
        var vm = new AdminFinanceUnifiedVM();
        vm.Transactions.Page = page;
        vm.Transactions.PageSize = pageSize;

        ViewBag.Keyword = keyword;
        ViewBag.SortBy = sortBy;
        ViewBag.Status = status;
        ViewBag.ActiveTab = tab;

        var txQuery = $"transactions?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(keyword)) txQuery += $"&keyword={Uri.EscapeDataString(keyword)}";
        if (!string.IsNullOrEmpty(sortBy)) txQuery += $"&sortBy={Uri.EscapeDataString(sortBy)}";
        if (!string.IsNullOrEmpty(status)) txQuery += $"&status={Uri.EscapeDataString(status)}";

        // Gọi song song 5 API để giảm latency
        var summaryTask = _api.GetAsync("admin/finance/summary");
        var payoutsTask = _api.GetAsync("admin/finance/payouts");
        var balanceTask = _api.GetAsync("admin/finance/balance");
        var historyTask = _api.GetAsync("admin/finance/withdrawals");
        var txTask = _api.GetAsync(txQuery);

        await Task.WhenAll(summaryTask, payoutsTask, balanceTask, historyTask, txTask);

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
}
