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
    }

    public class TransactionPagedVM
    {
        public List<TransactionListItemVM> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
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
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
    {
        var vm = new TransactionPagedVM { Page = page, PageSize = pageSize };

        var resp = await _api.GetAsync($"transactions?page={page}&pageSize={pageSize}");
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
    public async Task<IActionResult> Instructor(int page = 1, int pageSize = 20)
    {
        var vm = new TransactionPagedVM { Page = page, PageSize = pageSize };

        var resp = await _api.GetAsync($"transactions/instructor?page={page}&pageSize={pageSize}");
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
}
