using System.Text.Json;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers;

/// <summary>
/// FE MVC Controller cho Module Quản lý Giao dịch (UC-114, UC-115).
/// Dùng ApiClient gọi BE API, tự động forward JWT Cookie.
/// </summary>
public class TransactionController : Controller
{
    private readonly ApiClient _api;
    private readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

    public TransactionController(ApiClient api) => _api = api;

    // ─── Wrapper cho BE ApiResponse<T> ────────────────────────────────────
    private class ApiResp<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /Transaction?page=1&pageSize=20
    // UC-115: Danh sách giao dịch
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult Index()
    {
        return Redirect("/AdminFinance");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /Transaction/History?page=1&pageSize=10
    // Lịch sử mua hàng cho Người dùng (Học viên)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> History(int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = "date_desc", string? status = null)
    {
        var vm = new TransactionPagedVM { Page = page, PageSize = pageSize };

        ViewBag.Keyword = keyword;
        ViewBag.SortBy = sortBy;
        ViewBag.Status = status;

        var query = $"transactions/my?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(keyword)) query += $"&keyword={Uri.EscapeDataString(keyword)}";
        if (!string.IsNullOrEmpty(sortBy)) query += $"&sortBy={Uri.EscapeDataString(sortBy)}";
        if (!string.IsNullOrEmpty(status)) query += $"&status={Uri.EscapeDataString(status)}";

        var resp = await _api.GetAsync(query);
        if (resp.IsSuccessStatusCode)
        {
            var json = await resp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<TransactionPagedVM>>(json, _jsonOpts);
            if (parsed?.Data != null)
                vm = parsed.Data;
        }
        else if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(vm);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /Transaction/Instructor?page=1&pageSize=20
    // Lịch sử giao dịch cho Giảng viên
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Instructor(int page = 1, int pageSize = 20, string? keyword = null, string? sortBy = "date_desc", string? status = null, string tab = "tx",
        int payoutPage = 1, int payoutPageSize = 20, string? payoutKeyword = null, string? payoutSortBy = "date_desc", string? payoutStatus = null,
        int? year = null, int? month = null)
    {
        // Guard: Chỉ giảng viên đã duyệt mới được xem Earnings
        var approvalStatus = Request.Cookies["InstructorApprovalStatus"];
        if (approvalStatus != "Approved") 
            return RedirectToAction("ApplicationStatus", "Instructor");

        var vm = new InstructorFinancePageVM();
        vm.Transactions.Page = page;
        vm.Transactions.PageSize = pageSize;
        vm.Payouts.Page = payoutPage;
        vm.Payouts.PageSize = payoutPageSize;
        
        // Default to current UTC month/year if not provided
        int selectedYear = year ?? DateTime.UtcNow.Year;
        int selectedMonth = month ?? DateTime.UtcNow.Month;

        ViewBag.Keyword = keyword;
        ViewBag.SortBy = sortBy;
        ViewBag.Status = status;

        ViewBag.PayoutKeyword = payoutKeyword;
        ViewBag.PayoutSortBy = payoutSortBy;
        ViewBag.PayoutStatus = payoutStatus;
        ViewBag.ActiveTab = tab;
        ViewBag.Year = selectedYear;
        ViewBag.Month = selectedMonth;

        var txQuery = $"transactions/instructor?page={page}&pageSize={pageSize}&year={selectedYear}&month={selectedMonth}";
        if (!string.IsNullOrEmpty(keyword)) txQuery += $"&keyword={Uri.EscapeDataString(keyword)}";
        if (!string.IsNullOrEmpty(sortBy)) txQuery += $"&sortBy={Uri.EscapeDataString(sortBy)}";
        if (!string.IsNullOrEmpty(status)) txQuery += $"&status={Uri.EscapeDataString(status)}";

        var payoutQuery = $"instructor/payouts?page={payoutPage}&pageSize={payoutPageSize}&year={selectedYear}&month={selectedMonth}";
        if (!string.IsNullOrEmpty(payoutKeyword)) payoutQuery += $"&keyword={Uri.EscapeDataString(payoutKeyword)}";
        if (!string.IsNullOrEmpty(payoutSortBy)) payoutQuery += $"&sortBy={Uri.EscapeDataString(payoutSortBy)}";
        if (!string.IsNullOrEmpty(payoutStatus)) payoutQuery += $"&status={Uri.EscapeDataString(payoutStatus)}";

        // Fetch Transactions
        var txTask = _api.GetAsync(txQuery);
        // Fetch Payouts
        var payoutTask = _api.GetAsync(payoutQuery);
        // Fetch Payout Days Config
        var payoutDaysTask = _api.GetAsync("instructor/payout-days");

        await Task.WhenAll(txTask, payoutTask, payoutDaysTask);

        var txResp = await txTask;
        if (txResp.IsSuccessStatusCode)
        {
            var json = await txResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<TransactionPagedVM>>(json, _jsonOpts);
            if (parsed?.Data != null)
                vm.Transactions = parsed.Data;
        }
        else if (txResp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return RedirectToAction("Login", "Account");
        }

        var payoutResp = await payoutTask;
        if (payoutResp.IsSuccessStatusCode)
        {
            var json = await payoutResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<InstructorPayoutPagedViewModel>>(json, _jsonOpts);
            if (parsed?.Data != null)
                vm.Payouts = parsed.Data;
        }

        string payoutDays = "15";
        var payoutDaysResp = await payoutDaysTask;
        if (payoutDaysResp.IsSuccessStatusCode)
        {
            var json = await payoutDaysResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<string>>(json, _jsonOpts);
            if (parsed?.Data != null)
                payoutDays = parsed.Data;
        }
        ViewBag.PayoutDays = payoutDays;

        return View(vm);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /Transaction/Detail/{id}
    // UC-114: Chi tiết giao dịch
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var resp = await _api.GetAsync($"transactions/{id}");

        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return RedirectToAction("Login", "Account");

        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            TempData["Error"] = $"Transaction #{id} not found.";
            return RedirectToAction(nameof(Index));
        }

        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = "Cannot load transaction details.";
            return RedirectToAction(nameof(Index));
        }

        var json = await resp.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<ApiResp<TransactionDetailVM>>(json, _jsonOpts);

        if (parsed?.Data == null)
        {
            TempData["Error"] = "Invalid transaction data.";
            return RedirectToAction(nameof(Index));
        }

        return View(parsed.Data);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /Transaction/InstructorDetail/{id}
    // Chi tiết giao dịch dành cho Giảng viên
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> InstructorDetail(int id)
    {
        var resp = await _api.GetAsync($"transactions/{id}");

        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return RedirectToAction("Login", "Account");

        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            TempData["Error"] = $"Transaction #{id} not found.";
            return RedirectToAction("Instructor");
        }

        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = "Cannot load transaction details.";
            return RedirectToAction("Instructor");
        }

        var json = await resp.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<ApiResp<TransactionDetailVM>>(json, _jsonOpts);

        if (parsed?.Data == null)
        {
            TempData["Error"] = "Invalid transaction data.";
            return RedirectToAction("Instructor");
        }

        return View(parsed.Data);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /Transaction/HistoryDetail/{id}
    // Chi tiết giao dịch dành cho Người dùng (Học viên)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> HistoryDetail(int id)
    {
        var resp = await _api.GetAsync($"transactions/{id}");

        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return RedirectToAction("Login", "Account");

        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            TempData["Error"] = $"Transaction #{id} not found.";
            return RedirectToAction(nameof(History));
        }

        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = "Cannot load transaction details.";
            return RedirectToAction(nameof(History));
        }

        var json = await resp.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<ApiResp<TransactionDetailVM>>(json, _jsonOpts);

        if (parsed?.Data == null)
        {
            TempData["Error"] = "Invalid transaction data.";
            return RedirectToAction(nameof(History));
        }

        return View(parsed.Data);
    }

    [HttpPost]
    public async Task<IActionResult> RequestRefund(int transactionId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Json(new { success = false, message = "Refund reason cannot be empty." });
        }

        var response = await _api.PostJsonAsync($"transactions/{transactionId}/request-refund", new
        {
            Reason = reason
        });

        if (response.IsSuccessStatusCode)
        {
            return Json(new { success = true, message = "Refund request submitted successfully. Please wait for Admin approval." });
        }

        var errorBody = await response.Content.ReadAsStringAsync();
        var message = "System error when submitting the refund request.";
        try
        {
            using var doc = JsonDocument.Parse(errorBody);
            if (doc.RootElement.TryGetProperty("message", out var m))
                message = m.GetString() ?? message;
        }
        catch { }

        return Json(new { success = false, message = message });
    }
}
