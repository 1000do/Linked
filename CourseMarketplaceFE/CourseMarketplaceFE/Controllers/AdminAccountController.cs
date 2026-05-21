using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers;

public class AdminAccountController : Controller
{
    private readonly ApiClient _apiClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AdminAccountController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // GET /AdminAccount
    public async Task<IActionResult> Index(string? keyword, string? role, int page = 1)
    {
        role ??= "all";
        if (page < 1) page = 1;

        var url = $"/api/admin/accounts?keyword={Uri.EscapeDataString(keyword ?? "")}&role={role}&page={page}&pageSize=10";
        var response = await _apiClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var dataEl = root.GetProperty("data");
            var items = JsonSerializer.Deserialize<List<AdminAccountListItem>>(dataEl.GetRawText(), _jsonOptions) ?? new();

            var model = new AdminAccountListViewModel
            {
                Items = items,
                TotalCount = root.GetProperty("totalCount").GetInt32(),
                Page = root.GetProperty("page").GetInt32(),
                PageSize = root.GetProperty("pageSize").GetInt32(),
                Keyword = keyword,
                Role = role
            };
            return View(model);
        }

        // Return empty layout if API failed or unauthorized
        return View(new AdminAccountListViewModel { Role = role, Page = page, PageSize = 10 });
    }

    // GET /AdminAccount/Detail/{id}
    public async Task<IActionResult> Detail(int id)
    {
        var detailUrl = $"/api/admin/accounts/{id}";
        var transUrl = $"/api/admin/accounts/{id}/transactions";

        var detailResponse = await _apiClient.GetAsync(detailUrl);
        var transResponse = await _apiClient.GetAsync(transUrl);

        if (detailResponse.IsSuccessStatusCode && transResponse.IsSuccessStatusCode)
        {
            var detailContent = await detailResponse.Content.ReadAsStringAsync();
            var transContent = await transResponse.Content.ReadAsStringAsync();

            using var detailDoc = JsonDocument.Parse(detailContent);
            using var transDoc = JsonDocument.Parse(transContent);

            var detailDataEl = detailDoc.RootElement.GetProperty("data");
            var transDataEl = transDoc.RootElement.GetProperty("data");

            var account = JsonSerializer.Deserialize<AdminAccountDetailViewModel>(detailDataEl.GetRawText(), _jsonOptions);
            var transactions = JsonSerializer.Deserialize<AccountTransactionSummaryViewModel>(transDataEl.GetRawText(), _jsonOptions);

            if (account != null && transactions != null)
            {
                var model = new AdminAccountDetailPageViewModel
                {
                    Account = account,
                    Transactions = transactions
                };
                return View(model);
            }
        }

        TempData["ErrorMessage"] = "Failed to load account details or unauthorized.";
        return RedirectToAction("Index");
    }

    // POST /AdminAccount/CreateStaff
    [HttpPost]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffFERequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.DisplayName))
        {
            return Json(new { success = false, message = "All required fields must be completed." });
        }

        var response = await _apiClient.PostJsonAsync("/api/admin/accounts/staff", model);
        if (response.IsSuccessStatusCode)
        {
            return Json(new { success = true, message = "Staff account created successfully!" });
        }

        var content = await response.Content.ReadAsStringAsync();
        string errMsg = "Failed to create staff account.";
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("message", out var msgEl))
            {
                errMsg = msgEl.GetString() ?? errMsg;
            }
        }
        catch { }

        return Json(new { success = false, message = errMsg });
    }

    // POST /AdminAccount/EditStaff/{id}
    [HttpPost]
    public async Task<IActionResult> EditStaff(int id, [FromBody] UpdateStaffFERequest model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.DisplayName))
        {
            return Json(new { success = false, message = "Display name is required." });
        }

        var response = await _apiClient.PostJsonAsync($"/api/admin/accounts/staff/{id}", model);
        // Wait! Backend PUT endpoint is at "/api/admin/accounts/staff/{id}".
        // Let's use PUT via ApiClient or see if ApiClient supports PUT with body.
        // Wait! Let's check ApiClient's method signatures:
        // - PatchJsonAsync<T>(string url, T body)
        // - PutAsync(string url, HttpContent content) -> Wait, PutAsync doesn't serialize JSON directly, it takes HttpContent!
        // Wait, does backend support PUT, and can we call it via PutAsync using JsonContent or do we have PutJsonAsync in ApiClient?
        // Let's check: ApiClient has:
        // public Task<HttpResponseMessage> PutAsync(string url, HttpContent content)
        // So we can do: PutAsync(url, JsonContent.Create(model)) or we can define PutJsonAsync in ApiClient if it's missing, but actually we can just use PutAsync and wrap the body in JsonContent! Let's see:
        // Yes: `JsonContent.Create(model)` is standard in .NET. Let's do that!
        // Wait, is there a PutJsonAsync in ApiClient? No, there is only PutAsync taking HttpContent.
        // So let's write `JsonContent.Create(model)` which creates a `System.Net.Http.Json.JsonContent` representing the JSON payload.
        // Wait, let's check if the namespace System.Net.Http.Json is available. Yes, it is standard in .NET 6/7/8.
        // Let's do that. Or we can just use: `new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json")` as it is fully standard and doesn't rely on System.Net.Http.Json! That is extremely safe.
        // Let's write the EditStaff call:
        var jsonContent = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
        var putResponse = await _apiClient.PutAsync($"/api/admin/accounts/staff/{id}", jsonContent);

        if (putResponse.IsSuccessStatusCode)
        {
            return Json(new { success = true, message = "Staff account updated successfully!" });
        }

        var content = await putResponse.Content.ReadAsStringAsync();
        string errMsg = "Failed to update staff account.";
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("message", out var msgEl))
            {
                errMsg = msgEl.GetString() ?? errMsg;
            }
        }
        catch { }

        return Json(new { success = false, message = errMsg });
    }

    // POST /AdminAccount/ToggleBan/{id}
    [HttpPost]
    public async Task<IActionResult> ToggleBan(int id)
    {
        var response = await _apiClient.PostAsync($"/api/admin/accounts/{id}/toggle-ban");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            var currentStatus = root.GetProperty("currentStatus").GetString();

            return Json(new { success = true, currentStatus = currentStatus, message = $"Account status changed to {currentStatus} successfully." });
        }

        var errContent = await response.Content.ReadAsStringAsync();
        string errMsg = "Failed to change account status.";
        try
        {
            using var doc = JsonDocument.Parse(errContent);
            if (doc.RootElement.TryGetProperty("message", out var msgEl))
            {
                errMsg = msgEl.GetString() ?? errMsg;
            }
        }
        catch { }

        return Json(new { success = false, message = errMsg });
    }
}
