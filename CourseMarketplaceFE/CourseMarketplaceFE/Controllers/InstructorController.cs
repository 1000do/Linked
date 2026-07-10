using System.Text.Json;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CourseMarketplaceFE.Controllers;

/// <summary>
/// Frontend MVC Controller xử lý luồng Giảng viên:
/// GET/POST /Instructor/Apply       → Form nộp đơn
/// GET      /Instructor/ApplicationStatus   → Trạng thái đăng ký giảng viên
/// POST     /Instructor/SetupPayout → Gọi API setup Stripe → redirect
/// GET      /Instructor/StripeReturn → Stripe redirect về → verify → hiển thị kết quả
/// </summary>
[Authorize(Roles = "user,instructor")]
public class InstructorController : Controller
{
    private readonly ApiClient _api;

    public InstructorController(ApiClient api)
    {
        _api = api;
    }

    // ═══════════════════════════════════════════════════════════════════
    // 1. FORM NỘP ĐƠN — GET: hiển thị form, POST: gửi form
    // ═══════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Apply()
    {
        // Kiểm tra đăng nhập
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
            return Redirect("/Account/Login");

        try
        {
            // Kiểm tra xem đã có đơn đăng ký chưa
            var response = await _api.GetAsync("instructor/dashboard");
            if (response.IsSuccessStatusCode)
            {
                var dashJson = await response.Content.ReadAsStringAsync();
                using var dashDoc = JsonDocument.Parse(dashJson);
                if (dashDoc.RootElement.TryGetProperty("data", out var dashData) &&
                    dashData.TryGetProperty("approvalStatus", out var statusEl))
                {
                    var approvalStatus = statusEl.GetString();

                    // Nếu đơn bị từ chối → hiển form điền sẵn thông tin cũ để nộp lại
                    if (approvalStatus == "Rejected")
                    {
                        var model = new InstructorApplyViewModel { IsResubmit = true };

                        // Gọi API lấy thông tin đơn cũ
                        var infoResponse = await _api.GetAsync("instructor/apply-info");
                        if (infoResponse.IsSuccessStatusCode)
                        {
                            var infoJson = await infoResponse.Content.ReadAsStringAsync();
                            using var infoDoc = JsonDocument.Parse(infoJson);
                            if (infoDoc.RootElement.TryGetProperty("data", out var info))
                            {
                                model.ProfessionalTitle = info.TryGetProperty("professionalTitle", out var pt) ? pt.GetString() ?? "" : "";
                                model.ExpertiseCategories = info.TryGetProperty("expertiseCategories", out var ec) ? ec.GetString() ?? "" : "";
                                model.LinkedinUrl = info.TryGetProperty("linkedinUrl", out var li) ? li.GetString() : null;
                                model.ExistingDocumentUrl = info.TryGetProperty("documentUrl", out var doc) ? doc.GetString() : null;
                                model.RejectionReason = info.TryGetProperty("rejectionReason", out var rr) ? rr.GetString() : null;
                                if (!string.IsNullOrEmpty(model.ExistingDocumentUrl))
                                {
                                    model.ExistingDocumentUrls = model.ExistingDocumentUrl.Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList();
                                }
                            }
                        }

                        // Kiểm tra email đã xác thực
                        await LoadEmailVerifiedAsync();
                        model.AvailableCountries = await LoadStripeCountriesAsync();
                        model.AvailableCategories = await LoadCategoriesAsync();
                        return View(model);
                    }
                }

                // Pending hoặc Approved → redirect ApplicationStatus xem trạng thái
                return RedirectToAction("ApplicationStatus");
            }
        }
        catch { }

        // Kiểm tra email đã xác thực chưa
        await LoadEmailVerifiedAsync();

        var applyModel = new InstructorApplyViewModel();
        applyModel.AvailableCountries = await LoadStripeCountriesAsync();
        applyModel.AvailableCategories = await LoadCategoriesAsync();
        return View(applyModel);
    }

    /// <summary>Load ViewBag.IsEmailVerified từ API Profile.</summary>
    private async Task LoadEmailVerifiedAsync()
    {
        try
        {
            var profileResponse = await _api.GetAsync("Profile");
            if (profileResponse.IsSuccessStatusCode)
            {
                var json = await profileResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("isVerified", out var isVerifiedProp))
                {
                    ViewBag.IsEmailVerified = isVerifiedProp.GetBoolean();
                    return;
                }
            }
        }
        catch { }
        ViewBag.IsEmailVerified = false;
    }

    /// <summary>Load danh sách quốc gia Stripe hỗ trợ.</summary>
    private async Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> LoadStripeCountriesAsync()
    {
        var items = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
        try
        {
            var response = await _api.GetAsync("instructor/stripe-countries");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var data))
                {
                    foreach (var country in data.EnumerateArray())
                    {
                        var code = country.GetProperty("code").GetString();
                        var name = country.GetProperty("name").GetString();
                        if (code != null && name != null)
                        {
                            items.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                            {
                                Value = code,
                                Text = $"{name} ({code})"
                            });
                        }
                    }
                }
            }
        }
        catch { }

        // Fallback default
        if (items.Count == 0)
        {
            items.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "SG", Text = "Singapore (SG)" });
            items.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "US", Text = "United States (US)" });
        }

        // Ưu tiên SG nếu có
        var sg = items.FirstOrDefault(x => x.Value == "SG");
        if (sg != null) sg.Selected = true;

        return items;
    }

    /// <summary>Load danh sách lĩnh vực chuyên môn (Categories).</summary>
    private async Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> LoadCategoriesAsync()
    {
        var items = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
        try
        {
            var response = await _api.GetAsync("public/courses/categories");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var data))
                {
                    foreach (var cat in data.EnumerateArray())
                    {
                        // Kiểm tra cả camelCase và snake_case để chắc chắn
                        string? name = null;
                        if (cat.TryGetProperty("categoriesName", out var prop1)) name = prop1.GetString();
                        else if (cat.TryGetProperty("categories_name", out var prop2)) name = prop2.GetString();
                        else if (cat.TryGetProperty("CategoriesName", out var prop3)) name = prop3.GetString();

                        if (name != null)
                        {
                            items.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                            {
                                Value = name,
                                Text = name
                            });
                        }
                    }
                }
            }
            else
            {
                // Log lỗi nếu cần
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"API Category Error: {error}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadCategoriesAsync Exception: {ex.Message}");
        }
        return items;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(InstructorApplyViewModel model)
    {
        // Validate file count:
        int newFileCount = model.DocumentFiles?.Count(f => f != null && f.Length > 0) ?? 0;
        if (model.DocumentFile != null && model.DocumentFile.Length > 0)
        {
            newFileCount += 1;
        }

        int totalFileCount = newFileCount;
        if (model.IsResubmit)
        {
            totalFileCount += model.RetainedDocumentUrls?.Count ?? 0;
        }

        if (totalFileCount == 0)
        {
            ModelState.AddModelError("DocumentFiles", "Please upload at least 1 document/certificate file.");
        }
        else if (totalFileCount > 3)
        {
            ModelState.AddModelError("DocumentFiles", "You can upload a maximum of 3 document/certificate files.");
        }

        if (!ModelState.IsValid)
        {
            await LoadEmailVerifiedAsync();
            model.AvailableCountries = await LoadStripeCountriesAsync();
            model.AvailableCategories = await LoadCategoriesAsync();
            // Re-populate ExistingDocumentUrls with all original files on validation failure
            if (model.IsResubmit && !string.IsNullOrEmpty(model.ExistingDocumentUrl))
            {
                model.ExistingDocumentUrls = model.ExistingDocumentUrl.Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList();
            }
            return View(model);
        }

        try
        {
            // Gửi multipart/form-data (vì có file upload)
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.ProfessionalTitle ?? ""), "ProfessionalTitle");
            content.Add(new StringContent(model.ExpertiseCategories ?? ""), "ExpertiseCategories");
            content.Add(new StringContent(model.LinkedinUrl ?? ""), "LinkedinUrl");
            content.Add(new StringContent(model.YoutubeUrl ?? ""), "YoutubeUrl");
            content.Add(new StringContent(model.FacebookUrl ?? ""), "FacebookUrl");
            content.Add(new StringContent(model.StripeCountry ?? "SG"), "StripeCountry");

            if (model.RetainedDocumentUrls != null && model.RetainedDocumentUrls.Count > 0)
            {
                foreach (var url in model.RetainedDocumentUrls)
                {
                    content.Add(new StringContent(url), "RetainedDocumentUrls");
                }
            }

            if (model.DocumentFiles != null && model.DocumentFiles.Count > 0)
            {
                foreach (var file in model.DocumentFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        var fileContent = new StreamContent(file.OpenReadStream());
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                        content.Add(fileContent, "DocumentFiles", file.FileName);
                    }
                }
            }
            else if (model.DocumentFile != null && model.DocumentFile.Length > 0)
            {
                var fileContent = new StreamContent(model.DocumentFile.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.DocumentFile.ContentType);
                content.Add(fileContent, "DocumentFiles", model.DocumentFile.FileName);
            }

            var response = await _api.PostFormDataAsync("instructor/apply", content);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Your application has been submitted successfully! Please wait for admin approval.";
                return RedirectToAction("ApplicationStatus");
            }

            // Parse lỗi
            var errorMsg = "An error occurred.";
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("message", out var m))
                        errorMsg = m.GetString() ?? errorMsg;
                }
                catch { }
            }
            ViewBag.ApiError = errorMsg;
        }
        catch (Exception ex)
        {
            ViewBag.ApiError = $"Error: {ex.Message}";
        }

        if (model.IsResubmit && !string.IsNullOrEmpty(model.ExistingDocumentUrl))
        {
            model.ExistingDocumentUrls = model.ExistingDocumentUrl.Split(';').Where(x => !string.IsNullOrEmpty(x)).ToList();
        }
        model.AvailableCountries = await LoadStripeCountriesAsync();
        model.AvailableCategories = await LoadCategoriesAsync();
        return View(model);
    }

    // ═══════════════════════════════════════════════════════════════════
    // 2. TRẠNG THÁI ĐĂNG KÝ GIẢNG VIÊN
    // ═══════════════════════════════════════════════════════════════════
    [HttpGet]
    [Route("Instructor/ApplicationStatus")]
    public async Task<IActionResult> ApplicationStatus()
    {
        try
        {
            var response = await _api.GetAsync("instructor/dashboard");
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(json))
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var dataEl))
                {
                    var model = new InstructorDashboardViewModel
                    {
                        InstructorId = dataEl.GetProperty("instructorId").GetInt32(),
                        ProfessionalTitle = dataEl.TryGetProperty("professionalTitle", out var pt) ? pt.GetString() : null,
                        ExpertiseCategories = dataEl.TryGetProperty("expertiseCategories", out var ec) ? ec.GetString() : null,
                        ApprovalStatus = dataEl.TryGetProperty("approvalStatus", out var aps) ? aps.GetString() : null,
                        StripeOnboardingStatus = dataEl.TryGetProperty("stripeOnboardingStatus", out var sos) ? sos.GetString() : null,
                        PayoutsEnabled = dataEl.TryGetProperty("payoutsEnabled", out var pe) && pe.GetBoolean(),
                        ChargesEnabled = dataEl.TryGetProperty("chargesEnabled", out var ce) && ce.GetBoolean(),
                        FullName = dataEl.TryGetProperty("fullName", out var fn) ? fn.GetString() : null,
                        RejectionReason = dataEl.TryGetProperty("rejectionReason", out var rr) ? rr.GetString() : null
                    };

                    // ★ Lưu trạng thái duyệt vào Cookie để các phần khác kiểm tra nhanh
                    var statusCookieOpts = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7), Path = "/" };
                    Response.Cookies.Append("InstructorApprovalStatus", model.ApprovalStatus ?? "None", statusCookieOpts);

                    // ★ Set cookie UserRole = instructor ngay khi Approved (không chờ Stripe)
                    if (model.ApprovalStatus == "Approved")
                    {
                        if (Request.Cookies["UserRole"] != "instructor")
                        {
                            await _api.TryRefreshTokenAsync();
                        }
                        Response.Cookies.Append("UserRole", "instructor", statusCookieOpts);
                    }

                    return View(model);
                }
            }

            // Chưa đăng ký -> Xóa cookie trạng thái
            Response.Cookies.Delete("InstructorApprovalStatus", new CookieOptions { Path = "/" });

            // Chưa đăng ký → redirect về form Apply
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return RedirectToAction("Apply");
        }
        catch { }

        return RedirectToAction("Apply");
    }

    [HttpGet]
    [Route("Instructor/Dashboard")]
    public IActionResult DashboardRedirect()
    {
        return RedirectToAction("ApplicationStatus");
    }

    // ═══════════════════════════════════════════════════════════════════
    // 3. SETUP PAYOUT — Gọi API tạo Stripe → redirect sang Stripe
    // ═══════════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetupPayout()
    {
        try
        {
            var response = await _api.PostAsync("instructor/setup-payout");
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(json))
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("url", out var urlEl))
                {
                    var url = urlEl.GetString();
                    if (!string.IsNullOrEmpty(url))
                        return Redirect(url); // → Stripe Onboarding page
                }
            }

            TempData["ErrorMessage"] = "Could not set up Stripe. Please try again.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
        }

        return RedirectToAction("ApplicationStatus");
    }

    // ═══════════════════════════════════════════════════════════════════
    // 4. STRIPE RETURN — Stripe redirect về → gọi API verify
    // ═══════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> StripeReturn(int instructorId)
    {
        try
        {
            var response = await _api.GetAsync($"instructor/stripe-return?instructorId={instructorId}");
            var json = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(json))
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var message = root.TryGetProperty("message", out var m) ? m.GetString() : "Processing...";
                var stripeStatus = root.TryGetProperty("stripeStatus", out var ss) ? ss.GetString() : "Pending";

                if (response.IsSuccessStatusCode)
                {
                    if (stripeStatus == "Active")
                    {
                        // Hoàn tất 100% -> set cookie role
                        var cookieOpts = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(7), Path = "/" };
                        Response.Cookies.Append("UserRole", "instructor", cookieOpts);
                        TempData["SuccessMessage"] = message;
                    }
                    else
                    {
                        // Bấm Back hoặc chưa hoàn tất
                        TempData["ErrorMessage"] = message;
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Stripe authentication error: {ex.Message}";
        }

        return RedirectToAction("ApplicationStatus");
    }

    // ═══════════════════════════════════════════════════════════════════
    // 5. STRIPE LINK HẾT HẠN
    // ═══════════════════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult OnboardingRefresh()
    {
        ViewBag.Title = "Link Expired";
        ViewBag.Message = "The Stripe setup link has expired. Please go to the Application Status page to generate a new one.";
        ViewBag.IsSuccess = false;
        return View("OnboardingResult");
    }

    [HttpGet]
    [Route("Instructor/Notifications")]
    public async Task<IActionResult> Notifications()
    {
        // Guard
        var approvalStatus = Request.Cookies["InstructorApprovalStatus"];
        if (approvalStatus != "Approved") 
            return RedirectToAction("ApplicationStatus", "Instructor");

        var response = await _api.GetAsync("notification");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            // Đọc thẳng thành List giống hệt cách làm của bên User
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiResp = JsonSerializer.Deserialize<ApiResponseWrapper<PagedResult<NotificationViewModel>>>(json, options);

            return View(apiResp?.Data?.Items ?? new List<NotificationViewModel>());
        }
        return View(new List<NotificationViewModel>());
    }

    private class ApiResponseWrapper<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
    [HttpPost]
    [IgnoreAntiforgeryToken] // Diệt lỗi 400 khi gọi AJAX
    public async Task<IActionResult> MarkAsRead(int id)
    {
        // Gọi trực tiếp đến Backend API từ Server
        var response = await _api.PutAsync($"notification/mark-as-read/{id}", null!);
        if (response.IsSuccessStatusCode) return Ok();
        return BadRequest();
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var response = await _api.PutAsync("notification/mark-all-as-read", null!);
        if (response.IsSuccessStatusCode) return Ok();
        return BadRequest();
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var response = await _api.DeleteAsync($"notification/{id}");
        if (response.IsSuccessStatusCode) return Ok();
        return BadRequest();
    }

    // ═══════════════════════════════════════════════════════════════════
    // 6. LỊCH SỬ THANH TOÁN (PAYOUTS)
    // ═══════════════════════════════════════════════════════════════════


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AccessStripeDashboard()
    {
        try
        {
            var response = await _api.PostAsync("instructor/stripe-login-link");
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(json))
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("url", out var urlEl))
                {
                    var url = urlEl.GetString();
                    if (!string.IsNullOrEmpty(url))
                        return Redirect(url); // Redirect to Stripe Express Dashboard
                }
            }

            TempData["ErrorMessage"] = "Could not access Stripe Dashboard. Please try again.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
        }

        return RedirectToAction("ApplicationStatus");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetStripe()
    {
        try
        {
            var response = await _api.PostAsync("instructor/reset-stripe");
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Stripe account disconnected successfully! You can now link a new Stripe account.";
            }
            else
            {
                var errorMsg = "Could not disconnect Stripe account.";
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("message", out var m))
                            errorMsg = m.GetString() ?? errorMsg;
                    }
                    catch { }
                }
                TempData["ErrorMessage"] = errorMsg;
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
        }

        return RedirectToAction("ApplicationStatus");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncPayouts()
    {
        try
        {
            var response = await _api.PostAsync("instructor/sync-payouts");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Payout data synced from Stripe successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not sync data. Please try again later.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Sync error: {ex.Message}";
        }

        return RedirectToAction("Instructor", "Transaction");
    }

    // ═══════════════════════════════════════════════════════════════════
    // 7. PUBLIC PROFILE — GET: /Instructor/Profile/{id}
    // ═══════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Profile(int id)
    {
        try
        {
            var response = await _api.GetAsync($"instructor/profile/{id}");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index", "Home");

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<InstructorPublicProfileViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null || result.Data == null)
                return RedirectToAction("Index", "Home");

            return View(result.Data);
        }
        catch
        {
            return RedirectToAction("Index", "Home");
        }
    }

    // ═══════════════════════════════════════════════════════════════════

}

