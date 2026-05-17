using System.Text.Json;
using CourseMarketplaceFE.Helpers;
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

    // ─── View Models (mirror BE DTOs) ─────────────────────────────────────

    public class TransactionListItemVM
    {
        public int TransactionId { get; set; }
        public string? StripeSessionId { get; set; }
        public DateTime? Date { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public string BuyerName { get; set; } = "";
        public string CourseTitle { get; set; } = "";
        public string InstructorName { get; set; } = "";
        public string? Currency { get; set; }
        public string? PayoutCurrency { get; set; }
    }

    public class TransactionPagedVM
    {
        public List<TransactionListItemVM> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class InstructorFinancePageVM
    {
        public TransactionPagedVM Transactions { get; set; } = new();
        public CourseMarketplaceFE.Controllers.InstructorController.InstructorPayoutPagedViewModel Payouts { get; set; } = new();
    }

    public class TransactionDetailVM
    {
        public int TransactionId { get; set; }
        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public DateTime? Date { get; set; }
        public string? Status { get; set; }
        public string? Currency { get; set; }
        public string BuyerName { get; set; } = "";
        public string BuyerEmail { get; set; } = "";
        public string CourseTitle { get; set; } = "";
        public string? CourseThumbnail { get; set; }
        public string InstructorName { get; set; } = "";
        public string InstructorEmail { get; set; } = "";
        public decimal GrossAmount { get; set; }
        
        // ★ Mới thêm: Thông tin Coupon
        public decimal OriginalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool CouponUsed { get; set; }
        public string? CouponCode { get; set; }
        public string? CouponType { get; set; }
        public decimal? CouponDiscountValue { get; set; }

        public decimal TransferRate { get; set; }
        public decimal InstructorPayout { get; set; }
        public decimal PlatformProfit { get; set; }
        public bool IsPaid { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /Transaction?page=1&pageSize=20
    // UC-115: Danh sách giao dịch
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? keyword = null, string? sortBy = "date_desc", string? status = null)
    {
        var vm = new TransactionPagedVM { Page = page, PageSize = pageSize };

        ViewBag.Keyword = keyword;
        ViewBag.SortBy = sortBy;
        ViewBag.Status = status;

        var query = $"transactions?page={page}&pageSize={pageSize}";
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
        int payoutPage = 1, int payoutPageSize = 20, string? payoutKeyword = null, string? payoutSortBy = "date_desc", string? payoutStatus = null)
    {
        // Guard: Chỉ giảng viên đã duyệt mới được xem Earnings
        var approvalStatus = Request.Cookies["InstructorApprovalStatus"];
        if (approvalStatus != "Approved") 
            return RedirectToAction("Dashboard", "Instructor");

        var vm = new InstructorFinancePageVM();
        vm.Transactions.Page = page;
        vm.Transactions.PageSize = pageSize;
        vm.Payouts.Page = payoutPage;
        vm.Payouts.PageSize = payoutPageSize;
        
        ViewBag.Keyword = keyword;
        ViewBag.SortBy = sortBy;
        ViewBag.Status = status;

        ViewBag.PayoutKeyword = payoutKeyword;
        ViewBag.PayoutSortBy = payoutSortBy;
        ViewBag.PayoutStatus = payoutStatus;
        ViewBag.ActiveTab = tab;

        var txQuery = $"transactions/instructor?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(keyword)) txQuery += $"&keyword={Uri.EscapeDataString(keyword)}";
        if (!string.IsNullOrEmpty(sortBy)) txQuery += $"&sortBy={Uri.EscapeDataString(sortBy)}";
        if (!string.IsNullOrEmpty(status)) txQuery += $"&status={Uri.EscapeDataString(status)}";

        var payoutQuery = $"instructor/payouts?page={payoutPage}&pageSize={payoutPageSize}";
        if (!string.IsNullOrEmpty(payoutKeyword)) payoutQuery += $"&keyword={Uri.EscapeDataString(payoutKeyword)}";
        if (!string.IsNullOrEmpty(payoutSortBy)) payoutQuery += $"&sortBy={Uri.EscapeDataString(payoutSortBy)}";
        if (!string.IsNullOrEmpty(payoutStatus)) payoutQuery += $"&status={Uri.EscapeDataString(payoutStatus)}";

        // Fetch Transactions
        var txTask = _api.GetAsync(txQuery);
        // Fetch Payouts
        var payoutTask = _api.GetAsync(payoutQuery);

        await Task.WhenAll(txTask, payoutTask);

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
            var parsed = JsonSerializer.Deserialize<ApiResp<CourseMarketplaceFE.Controllers.InstructorController.InstructorPayoutPagedViewModel>>(json, _jsonOpts);
            if (parsed?.Data != null)
                vm.Payouts = parsed.Data;
        }

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
            TempData["Error"] = $"Không tìm thấy giao dịch #{id}.";
            return RedirectToAction(nameof(Index));
        }

        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = "Không thể tải chi tiết giao dịch.";
            return RedirectToAction(nameof(Index));
        }

        var json = await resp.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<ApiResp<TransactionDetailVM>>(json, _jsonOpts);

        if (parsed?.Data == null)
        {
            TempData["Error"] = "Dữ liệu giao dịch không hợp lệ.";
            return RedirectToAction(nameof(Index));
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
            TempData["Error"] = $"Không tìm thấy giao dịch #{id}.";
            return RedirectToAction(nameof(History));
        }

        if (!resp.IsSuccessStatusCode)
        {
            TempData["Error"] = "Không thể tải chi tiết giao dịch.";
            return RedirectToAction(nameof(History));
        }

        var json = await resp.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<ApiResp<TransactionDetailVM>>(json, _jsonOpts);

        if (parsed?.Data == null)
        {
            TempData["Error"] = "Dữ liệu giao dịch không hợp lệ.";
            return RedirectToAction(nameof(History));
        }

        return View(parsed.Data);
    }
}
