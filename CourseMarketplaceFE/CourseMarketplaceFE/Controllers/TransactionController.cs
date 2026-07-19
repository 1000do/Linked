using System.Text.Json;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Authorization;
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
    // UC-39: View Transaction List, UC-40: Search — User và Instructor xem lịch sử giao dịch của mình
    [HttpGet]
    [Authorize(Roles = "user,instructor")]
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
    // UC-67: View Instructor TX, UC-69: Sort, UC-70: Filter, UC-74: View Revenue — Chỉ Instructor
    [HttpGet]
    [Authorize(Roles = "instructor")]
    public async Task<IActionResult> Instructor(int page = 1, int pageSize = 20, string? keyword = null, string? sortBy = "date_desc", string? status = null, string tab = "tx",
        int payoutPage = 1, int payoutPageSize = 20, string? payoutKeyword = null, string? payoutSortBy = "date_desc", string? payoutStatus = null,
        int? year = null, int? month = null, string? courseSortBy = "sales_desc")
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
        ViewBag.CourseSortBy = courseSortBy;

        var txQuery = $"transactions/instructor?page={page}&pageSize={pageSize}&year={selectedYear}&month={selectedMonth}";
        if (!string.IsNullOrEmpty(keyword)) txQuery += $"&keyword={Uri.EscapeDataString(keyword)}";
        if (!string.IsNullOrEmpty(sortBy)) txQuery += $"&sortBy={Uri.EscapeDataString(sortBy)}";
        if (!string.IsNullOrEmpty(status)) txQuery += $"&status={Uri.EscapeDataString(status)}";

        var payoutQuery = $"instructor/payouts?page={payoutPage}&pageSize={payoutPageSize}&year={selectedYear}&month={selectedMonth}";
        if (!string.IsNullOrEmpty(payoutKeyword)) payoutQuery += $"&keyword={Uri.EscapeDataString(payoutKeyword)}";
        if (!string.IsNullOrEmpty(payoutSortBy)) payoutQuery += $"&sortBy={Uri.EscapeDataString(payoutSortBy)}";
        if (!string.IsNullOrEmpty(payoutStatus)) payoutQuery += $"&status={Uri.EscapeDataString(payoutStatus)}";

        var allTxQuery = $"transactions/instructor?page=1&pageSize=100000&year={selectedYear}&month={selectedMonth}";
        var allPayoutQuery = $"instructor/payouts?page=1&pageSize=100000&year={selectedYear}&month={selectedMonth}";

        // Fetch Transactions
        var txTask = _api.GetAsync(txQuery);
        // Fetch Payouts
        var payoutTask = _api.GetAsync(payoutQuery);
        // Fetch Payout Days Config
        var payoutDaysTask = _api.GetAsync("instructor/payout-days");
        // Fetch ALL Transactions (for chart)
        var allTxTask = _api.GetAsync(allTxQuery);
        // Fetch ALL Payouts (for overview metrics)
        var allPayoutTask = _api.GetAsync(allPayoutQuery);
        // Fetch Course Revenues
        var courseRevenuesTask = _api.GetAsync($"transactions/instructor/course-revenues?year={selectedYear}&month={selectedMonth}");

        await Task.WhenAll(txTask, payoutTask, payoutDaysTask, allTxTask, allPayoutTask, courseRevenuesTask);

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

        List<TransactionListItemVM> allTransactionsList = new();
        var allTxResp = await allTxTask;
        if (allTxResp.IsSuccessStatusCode)
        {
            var json = await allTxResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<TransactionPagedVM>>(json, _jsonOpts);
            if (parsed?.Data?.Items != null)
                allTransactionsList = parsed.Data.Items;
        }
        ViewBag.AllTransactions = allTransactionsList;

        List<InstructorPayoutViewModel> allPayoutsList = new();
        var allPayoutResp = await allPayoutTask;
        if (allPayoutResp.IsSuccessStatusCode)
        {
            var json = await allPayoutResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<InstructorPayoutPagedViewModel>>(json, _jsonOpts);
            if (parsed?.Data?.Items != null)
                allPayoutsList = parsed.Data.Items;
        }
        ViewBag.AllPayouts = allPayoutsList;

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

        var courseRevenuesResp = await courseRevenuesTask;
        if (courseRevenuesResp.IsSuccessStatusCode)
        {
            var json = await courseRevenuesResp.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<List<InstructorCourseRevenueResponse>>>(json, _jsonOpts);
            if (parsed?.Data != null)
            {
                var courseRevenues = parsed.Data;
                courseRevenues = courseSortBy switch
                {
                    "sales_desc" => courseRevenues.OrderByDescending(r => r.SalesCount).ToList(),
                    "sales_asc" => courseRevenues.OrderBy(r => r.SalesCount).ToList(),
                    "lifetime_desc" => courseRevenues.OrderByDescending(r => r.LifetimeRevenue).ToList(),
                    "lifetime_asc" => courseRevenues.OrderBy(r => r.LifetimeRevenue).ToList(),
                    "monthly_desc" => courseRevenues.OrderByDescending(r => r.MonthlyRevenue).ToList(),
                    "monthly_asc" => courseRevenues.OrderBy(r => r.MonthlyRevenue).ToList(),
                    "yearly_desc" => courseRevenues.OrderByDescending(r => r.YearlyRevenue).ToList(),
                    "yearly_asc" => courseRevenues.OrderBy(r => r.YearlyRevenue).ToList(),
                    _ => courseRevenues.OrderByDescending(r => r.SalesCount).ToList()
                };
                vm.CourseRevenues = courseRevenues;
            }
        }

        return View(vm);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /Transaction/Detail/{id}
    // UC-114: Chi tiết giao dịch
    // ═══════════════════════════════════════════════════════════════════════
    // UC-114: View Transaction Detail — Admin
    [HttpGet]
    [Authorize(Roles = "admin")]
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
    // UC-73: View Instructor's Transaction Detail — Chỉ Instructor
    [HttpGet]
    [Authorize(Roles = "instructor")]
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
    // UC-42: View Transaction Detail (User View) — User và Instructor
    [HttpGet]
    [Authorize(Roles = "user,instructor")]
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

    // UC-41: Request Refund — User và Instructor
    [HttpPost]
    [Authorize(Roles = "user,instructor")]
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
            var json = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<ApiResp<RefundResultDto>>(json, _jsonOpts);
            if (parsed?.Success == true && parsed.Data != null)
            {
                var result = parsed.Data;
                if (result.IsAutoRejected)
                {
                    return Json(new { success = true, autoRejected = true, message = $"Your refund request has been rejected due to {result.RejectReason}." });
                }
                else
                {
                    return Json(new { success = true, autoRejected = false, message = "Your refund request has been successfully received and is currently under review." });
                }
            }
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
