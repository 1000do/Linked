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
    }

    public class FinanceDashboardVM
    {
        public FinancialSummaryVM Summary { get; set; } = new();
        public List<PayoutDetailVM> Payouts { get; set; } = new();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /AdminFinance
    // Dashboard chính — gọi 2 API song song: summary + payouts
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var vm = new FinanceDashboardVM();

        // Gọi song song 2 API để giảm latency
        var summaryTask = _api.GetAsync("admin/finance/summary");
        var payoutsTask = _api.GetAsync("admin/finance/payouts");

        await Task.WhenAll(summaryTask, payoutsTask);

        // Parse summary
        var summaryResp = await summaryTask;
        if (summaryResp.IsSuccessStatusCode)
        {
            var json = await summaryResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<FinancialSummaryVM>>(json, _jsonOpts);
            if (parsed?.Data != null)
                vm.Summary = parsed.Data;
        }

        // Parse payouts
        var payoutsResp = await payoutsTask;
        if (payoutsResp.IsSuccessStatusCode)
        {
            var json = await payoutsResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<List<PayoutDetailVM>>>(json, _jsonOpts);
            if (parsed?.Data != null)
                vm.Payouts = parsed.Data;
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
            TempData["FinanceSuccess"] = $"✅ Đã cập nhật: Giảng viên nhận {rate}%, Sàn nhận {100 - rate}%.";
        }
        else
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            TempData["FinanceError"] = $"❌ Lỗi: {errorBody}";
        }

        return RedirectToAction(nameof(Index));
    }
}
