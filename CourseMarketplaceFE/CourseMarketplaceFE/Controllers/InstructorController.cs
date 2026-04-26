using System.Text.Json;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers;

/// <summary>
/// Frontend MVC Controller xử lý luồng Giảng viên:
/// GET/POST /Instructor/Apply       → Form nộp đơn
/// GET      /Instructor/Dashboard   → Dashboard giảng viên
/// POST     /Instructor/SetupPayout → Gọi API setup Stripe → redirect
/// GET      /Instructor/StripeReturn → Stripe redirect về → verify → hiển thị kết quả
/// </summary>
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
                // Đã có đơn (Pending/Approved/...) -> chuyển sang Dashboard xem trạng thái
                return RedirectToAction("Dashboard");
            }
        }
        catch { }

        // Kiểm tra email đã xác thực chưa
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
                }
                else
                {
                    ViewBag.IsEmailVerified = false;
                }
            }
            else
            {
                ViewBag.IsEmailVerified = false;
            }
        }
        catch
        {
            ViewBag.IsEmailVerified = false;
        }

        return View(new InstructorApplyViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(InstructorApplyViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            // Gửi multipart/form-data (vì có file upload)
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.ProfessionalTitle ?? ""), "ProfessionalTitle");
            content.Add(new StringContent(model.ExpertiseCategories ?? ""), "ExpertiseCategories");
            content.Add(new StringContent(model.LinkedinUrl ?? ""), "LinkedinUrl");

            if (model.DocumentFile != null)
            {
                var fileContent = new StreamContent(model.DocumentFile.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.DocumentFile.ContentType);
                content.Add(fileContent, "DocumentFile", model.DocumentFile.FileName);
            }

            var response = await _api.PostFormDataAsync("instructor/apply", content);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đơn đăng ký đã được gửi thành công! Vui lòng chờ Admin duyệt.";
                return RedirectToAction("Dashboard");
            }

            // Parse lỗi
            var errorMsg = "Có lỗi xảy ra.";
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
            ModelState.AddModelError("", errorMsg);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Lỗi: {ex.Message}");
        }

        return View(model);
    }

    // ═══════════════════════════════════════════════════════════════════
    // 2. DASHBOARD GIẢNG VIÊN
    // ═══════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> Dashboard()
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
                        FullName = dataEl.TryGetProperty("fullName", out var fn) ? fn.GetString() : null
                    };
                    return View(model);
                }
            }

            // Chưa đăng ký → redirect về form Apply
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return RedirectToAction("Apply");
        }
        catch { }

        return RedirectToAction("Apply");
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

            TempData["ErrorMessage"] = "Không thể thiết lập Stripe. Vui lòng thử lại.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
        }

        return RedirectToAction("Dashboard");
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

                var message = root.TryGetProperty("message", out var m) ? m.GetString() : "Đang xử lý...";
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
            TempData["ErrorMessage"] = $"Lỗi xác thực Stripe: {ex.Message}";
        }

        return RedirectToAction("Dashboard");
    }

    // ═══════════════════════════════════════════════════════════════════
    // 5. STRIPE LINK HẾT HẠN
    // ═══════════════════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult OnboardingRefresh()
    {
        ViewBag.Title = "Link đã hết hạn";
        ViewBag.Message = "Link thiết lập Stripe đã hết hạn. Vui lòng vào Dashboard để tạo lại.";
        ViewBag.IsSuccess = false;
        return View("OnboardingResult");
    }

    [HttpGet]
    [Route("Instructor/Notifications")]
    public async Task<IActionResult> Notifications()
    {
        var response = await _api.GetAsync("notification");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            // Đọc thẳng thành List giống hệt cách làm của bên User
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var list = JsonSerializer.Deserialize<List<NotificationViewModel>>(json, options);

            return View(list ?? new List<NotificationViewModel>());
        }
        return View(new List<NotificationViewModel>());
    }
    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        // SỬA: Đưa id vào chuỗi URL để khớp với [HttpPut("mark-as-read/{id}")] bên BE
        var response = await _api.PutAsync($"notification/mark-as-read/{id}", null);

        if (response.IsSuccessStatusCode) return Ok();
        return BadRequest();
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        // SỬA: Đưa id vào chuỗi URL để khớp với [HttpDelete("{id}")] bên BE
        var response = await _api.DeleteAsync($"notification/{id}");

        if (response.IsSuccessStatusCode) return Ok();
        return BadRequest();
    }
}
