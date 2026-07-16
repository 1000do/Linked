using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CourseMarketplaceFE.Controllers;

public class GiftController : Controller
{
    private readonly ApiClient _api;
    private readonly IConfiguration _config;

    public GiftController(ApiClient api, IConfiguration config)
    {
        _api = api;
        _config = config;
    }

    // UC-17/18: Gift Courses & Pay for Gift — User và Instructor mới được tặng khóa học
    [HttpGet]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> Setup(int courseId)
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
        {
            return Redirect($"/Account/Login?returnUrl=/Gift/Setup?courseId={courseId}");
        }

        // Lấy thông tin khóa học để hiển thị trên giao diện Setup
        var response = await _api.GetAsync($"gift/course/{courseId}");
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Course not found.";
            return Redirect("/");
        }

        var json = await response.Content.ReadAsStringAsync();
        PublicCourseViewModel? course = null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("data", out var dataEl))
            {
                course = JsonSerializer.Deserialize<PublicCourseViewModel>(dataEl.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch (Exception)
        {
            TempData["Error"] = "Error loading course details.";
            return Redirect("/");
        }

        if (course == null)
        {
            TempData["Error"] = "Course data is empty.";
            return Redirect("/");
        }

        ViewBag.Course = course;
        return View(new GiftSetupViewModel { CourseId = courseId });
    }

    // UC-17/18: Post Gift Setup form — User và Instructor
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> Setup(GiftSetupViewModel model)
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
        {
            return Redirect($"/Account/Login?returnUrl=/Gift/Setup?courseId={model.CourseId}");
        }

        if (ModelState.IsValid)
        {
            // Check recipient enrollment on the backend
            var checkResponse = await _api.GetAsync($"gift/check-recipient?email={Uri.EscapeDataString(model.RecipientEmail)}&courseId={model.CourseId}");
            if (checkResponse.IsSuccessStatusCode)
            {
                var json = await checkResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var dataProp) && dataProp.GetBoolean())
                {
                    ModelState.AddModelError("RecipientEmail", "The recipient has already owned/joined this course.");
                }
            }
        }

        if (!ModelState.IsValid)
        {
            // Load lại thông tin khóa học nếu form invalid
            var courseResponse = await _api.GetAsync($"gift/course/{model.CourseId}");
            if (courseResponse.IsSuccessStatusCode)
            {
                var json = await courseResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var dataEl))
                {
                    ViewBag.Course = JsonSerializer.Deserialize<PublicCourseViewModel>(dataEl.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            return View(model);
        }

        return RedirectToAction("Checkout", new
        {
            courseId = model.CourseId,
            recipientEmail = model.RecipientEmail,
            recipientName = model.RecipientName,
            giftMessage = model.GiftMessage,
            cardTheme = model.CardTheme
        });
    }

    // UC-17: Check Recipient AJAX — User và Instructor
    [HttpGet]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> CheckRecipient(string email, int courseId)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Json(new { enrolled = false });
        }

        var response = await _api.GetAsync($"gift/check-recipient?email={Uri.EscapeDataString(email)}&courseId={courseId}");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("data", out var dataProp))
            {
                return Json(new { enrolled = dataProp.GetBoolean() });
            }
        }
        return Json(new { enrolled = false });
    }

    // UC-18: Pay for Gift (Review page trước khi thanh toán) — User và Instructor
    [HttpGet]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> Checkout(
        int courseId,
        string recipientEmail,
        string? recipientName,
        string? giftMessage,
        string cardTheme)
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
        {
            return Redirect($"/Account/Login?returnUrl=/Gift/Setup?courseId={courseId}");
        }

        // Lấy thông tin khóa học
        var courseResponse = await _api.GetAsync($"gift/course/{courseId}");
        if (!courseResponse.IsSuccessStatusCode)
        {
            TempData["Error"] = "Course not found.";
            return Redirect("/");
        }

        var json = await courseResponse.Content.ReadAsStringAsync();
        PublicCourseViewModel? course = null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("data", out var dataEl))
            {
                course = JsonSerializer.Deserialize<PublicCourseViewModel>(dataEl.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch { }

        if (course == null)
        {
            TempData["Error"] = "Course data is empty.";
            return Redirect("/");
        }

        string clientSecret = "";
        string paymentIntentId = "";

        // Lấy thông tin email người dùng
        string userEmail = "";
        try
        {
            var profileResponse = await _api.GetAsync("Profile");
            if (profileResponse.IsSuccessStatusCode)
            {
                var profileJson = await profileResponse.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(profileJson);
                if (doc.RootElement.TryGetProperty("data", out var dataEl))
                {
                    userEmail = dataEl.TryGetProperty("email", out var emailProp) ? emailProp.GetString() ?? "" : "";
                }
            }
        }
        catch { }

        var publishableKey = _config["Stripe:PublishableKey"] ?? "";

        var cartModel = new CartViewModel
        {
            Items = new List<CartItemViewModel>
            {
                new CartItemViewModel
                {
                    CourseId = courseId,
                    Title = course.Title,
                    ThumbnailUrl = course.CourseThumbnailUrl,
                    Price = course.Price,
                    InstructorName = course.InstructorName ?? "Instructor"
                }
            },
            SubTotal = course.Price,
            Total = course.Price
        };

        var model = new CheckoutViewModel
        {
            PublishableKey = publishableKey,
            ClientSecret = clientSecret,
            PaymentIntentId = paymentIntentId,
            Email = userEmail,
            Cart = cartModel
        };

        ViewBag.RecipientName = recipientName;
        ViewBag.RecipientEmail = recipientEmail;
        ViewBag.GiftMessage = giftMessage;
        ViewBag.CardTheme = cardTheme;

        return View(model);
    }

    // UC-18: Tạo Gift Payment Intent (AJAX) — User và Instructor
    [HttpPost]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> CreateGiftPaymentIntentAjax(
        int courseId,
        string recipientEmail,
        string? recipientName,
        string? giftMessage,
        string cardTheme)
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
            return Unauthorized(new { message = "Unauthorized" });

        var scheme = Request.Headers.ContainsKey("X-Forwarded-Proto") 
            ? Request.Headers["X-Forwarded-Proto"].ToString() 
            : Request.Scheme;
        var host = Request.Headers.ContainsKey("X-Forwarded-Host") 
            ? Request.Headers["X-Forwarded-Host"].ToString() 
            : Request.Host.ToString();
        var baseUrl = $"{scheme}://{host}";
        var response = await _api.PostJsonAsync("checkout/gift-intent", new
        {
            courseId = courseId,
            recipientEmail = recipientEmail,
            recipientName = recipientName,
            giftMessage = giftMessage,
            cardTheme = cardTheme,
            successUrl = $"{baseUrl}/Gift/CheckoutSuccess?session_id={{CHECKOUT_SESSION_ID}}",
            cancelUrl = $"{baseUrl}/Gift/CheckoutCancel"
        });

        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return BadRequest(json);
        }

        return Content(json, "application/json");
    }

    // ─── 2. MÀN HÌNH NHẬN QUÀ TẶNG (UC-17) ──────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Claim([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View("Error", new ErrorViewModel { RequestId = "Missing redemption token." });
        }

        // Gọi Backend để kiểm tra token quà tặng
        var response = await _api.GetAsync($"gift/validate/{token}");
        if (!response.IsSuccessStatusCode)
        {
            var errJson = await response.Content.ReadAsStringAsync();
            string errorMsg = "This gift is invalid or has expired.";
            try
            {
                using var doc = JsonDocument.Parse(errJson);
                if (doc.RootElement.TryGetProperty("message", out var m))
                    errorMsg = m.GetString() ?? errorMsg;
            }
            catch { }

            ViewBag.ErrorMessage = errorMsg;
            return View("ClaimError");
        }

        var json = await response.Content.ReadAsStringAsync();
        GiftValidationViewModel? giftInfo = null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("data", out var dataEl))
            {
                giftInfo = JsonSerializer.Deserialize<GiftValidationViewModel>(dataEl.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch (Exception)
        {
            ViewBag.ErrorMessage = "Failed to parse gift information.";
            return View("ClaimError");
        }

        if (giftInfo == null)
        {
            ViewBag.ErrorMessage = "Gift data is empty.";
            return View("ClaimError");
        }

        ViewBag.Token = token;
        ViewBag.IsLoggedIn = HttpContext.Request.Cookies.ContainsKey("AccessToken");
        return View(giftInfo);
    }

    // UC-19: Claim Gift (AJAX) — User và Instructor phải đăng nhập mới nhận được quà
    [HttpPost]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> ClaimGiftAjax(string token)
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
        {
            return Json(new { success = false, message = "Please login first to claim your course." });
        }

        var response = await _api.PostJsonAsync("gift/claim", new { redemptionToken = token });
        if (response.IsSuccessStatusCode)
        {
            return Json(new { success = true });
        }

        var errJson = await response.Content.ReadAsStringAsync();
        string errorMsg = "Failed to claim the gift course.";
        try
        {
            using var doc = JsonDocument.Parse(errJson);
            if (doc.RootElement.TryGetProperty("message", out var m))
                errorMsg = m.GetString() ?? errorMsg;
        }
        catch { }

        return Json(new { success = false, message = errorMsg });
    }

    // UC-18: Stripe Checkout Success callback — User và Instructor
    [HttpGet]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> CheckoutSuccess(
        [FromQuery(Name = "session_id")] string? sessionId,
        [FromQuery(Name = "payment_intent_id")] string? paymentIntentId)
    {
        if (string.IsNullOrWhiteSpace(sessionId) && string.IsNullOrWhiteSpace(paymentIntentId))
        {
            TempData["Error"] = "Invalid checkout session or payment intent.";
            return Redirect("/");
        }

        try
        {
            HttpResponseMessage response;
            if (!string.IsNullOrWhiteSpace(paymentIntentId))
            {
                response = await _api.GetAsync($"checkout/gift-success-intent?payment_intent_id={Uri.EscapeDataString(paymentIntentId)}");
            }
            else
            {
                response = await _api.GetAsync($"checkout/success?session_id={Uri.EscapeDataString(sessionId!)}");
            }

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "🎉 Thank you! Your gift order has been placed successfully and the card email has been sent to the recipient.";
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                TempData["Error"] = "Successfully paid but failed to initialize gift delivery on our servers. Please contact support.";
            }
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while processing gift checkout success.";
        }

        return Redirect("/Course/MyCourses");
    }

    [HttpGet]
    public IActionResult CheckoutCancel()
    {
        TempData["Error"] = "Gift purchase was cancelled.";
        return Redirect("/");
    }
}

