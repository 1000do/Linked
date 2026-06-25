using System.Text.Json;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CourseMarketplaceFE.Controllers;

/// <summary>
/// FE MVC Controller cho Giỏ hàng.
/// Dùng Cookie để lưu couponCode tạm thời — nhất quán với AccessToken cookie đang có sẵn.
/// Không cần Session, không cần AddDistributedMemoryCache().
/// ApiClient tự động gắn Bearer Token của user đang login khi gọi BE.
/// </summary>
[Authorize(Roles = "user,instructor")]
public class CartController : Controller
{
    private readonly ApiClient _api;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

    // Tên cookie lưu mã giảm giá (không nhạy cảm, chỉ là text ngắn)
    private const string CouponCookieName = "cart_coupon";

    public CartController(ApiClient api, Microsoft.Extensions.Configuration.IConfiguration config)
    {
        _api = api;
        _config = config;
    }

    private Dictionary<int, string> GetCouponMapFromCookie()
    {
        var cookie = HttpContext.Request.Cookies[CouponCookieName];
        var map = new Dictionary<int, string>();
        if (string.IsNullOrWhiteSpace(cookie)) return map;

        var parts = cookie.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var kvp = part.Split(':');
            if (kvp.Length == 2 && int.TryParse(kvp[0], out int cid))
            {
                map[cid] = kvp[1].Trim().ToUpper();
            }
        }
        return map;
    }

    private void SaveCouponMapToCookie(Dictionary<int, string> map)
    {
        var cookieValue = string.Join(",", map.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        if (string.IsNullOrWhiteSpace(cookieValue))
        {
            HttpContext.Response.Cookies.Delete(CouponCookieName);
        }
        else
        {
            HttpContext.Response.Cookies.Append(CouponCookieName, cookieValue, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddHours(1),
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });
        }
    }

    // ─── 1. XEM GIỎ HÀNG ─────────────────────────────────────────────────
    /// <summary>
    /// GET /Cart
    /// Đọc couponCode từ Cookie → gọi API summary → render View.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
            return Redirect("/Account/Login");

        // Đọc map coupon từ Cookie
        var couponMap = GetCouponMapFromCookie();
        var couponCode = string.Join(",", couponMap.Values.Distinct());

        // Gọi API BE summary, truyền danh sách couponCode vào query string nếu có
        var url = string.IsNullOrWhiteSpace(couponCode)
            ? "cart/summary"
            : $"cart/summary?couponCode={Uri.EscapeDataString(couponCode)}";

        var response = await _api.GetAsync(url);
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var errJson = await response.Content.ReadAsStringAsync();
            string errorMsg = "This coupon is invalid.";
            try
            {
                using var errDoc = JsonDocument.Parse(errJson);
                if (errDoc.RootElement.TryGetProperty("message", out var m))
                    errorMsg = m.GetString() ?? errorMsg;
            }
            catch { }

            TempData["CartError"] = errorMsg;

            if (TempData["PendingCouponCheck"] is int badPendingCourseId)
            {
                couponMap.Remove(badPendingCourseId);
                SaveCouponMapToCookie(couponMap);
            }
            else
            {
                SaveCouponMapToCookie(new Dictionary<int, string>());
            }

            var updatedCouponCode = string.Join(",", couponMap.Values.Distinct());
            url = string.IsNullOrWhiteSpace(updatedCouponCode)
                ? "cart/summary"
                : $"cart/summary?couponCode={Uri.EscapeDataString(updatedCouponCode)}";

            response = await _api.GetAsync(url);
        }

        var json = await response.Content.ReadAsStringAsync();
        var model = new CartViewModel();

        if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using var doc = JsonDocument.Parse(json);

                // BE trả về cấu trúc ApiResponse<CartSummaryResponse>: { success, message, data: {...} }
                if (doc.RootElement.TryGetProperty("data", out var dataEl))
                {
                    model.Items = dataEl.TryGetProperty("items", out var itemsEl)
                        ? JsonSerializer.Deserialize<List<CartItemViewModel>>(itemsEl.GetRawText(), opts) ?? new()
                        : new();

                    model.SubTotal = dataEl.TryGetProperty("subTotal", out var st) ? st.GetDecimal() : 0m;
                    model.DiscountAmount = dataEl.TryGetProperty("discountAmount", out var da) ? da.GetDecimal() : 0m;
                    model.Total = dataEl.TryGetProperty("total", out var tot) ? tot.GetDecimal() : 0m;
                    model.AppliedCouponCode = dataEl.TryGetProperty("appliedCouponCode", out var ac) ? ac.GetString() : null;
                    model.CouponMessage = dataEl.TryGetProperty("couponMessage", out var cm) ? cm.GetString() : null;

                    // Parse danh sách voucher khả dụng (Voucher Wallet)
                    model.AvailableCoupons = dataEl.TryGetProperty("availableCoupons", out var avEl)
                        ? JsonSerializer.Deserialize<List<AvailableCouponViewModel>>(avEl.GetRawText(), opts) ?? new()
                        : new();
                }
            }
            catch { /* json parse lỗi → model rỗng */ }
        }

        if (TempData["PendingCouponCheck"] is int pendingCourseId)
        {
            var enteredCode = couponMap.ContainsKey(pendingCourseId) ? couponMap[pendingCourseId] : null;
            if (!string.IsNullOrEmpty(enteredCode))
            {
                var cartItem = model.Items.FirstOrDefault(i => i.CourseId == pendingCourseId);
                if (cartItem == null || !string.Equals(cartItem.AppliedCouponCode, enteredCode, StringComparison.OrdinalIgnoreCase))
                {
                    // Invalid or ineligible! Remove from cookie map
                    couponMap.Remove(pendingCourseId);
                    SaveCouponMapToCookie(couponMap);
                    TempData["CartError"] = $"Voucher '{enteredCode}' is invalid or does not meet the requirements.";
                }
                else
                {
                    TempData["CartSuccess"] = $"Voucher '{enteredCode}' applied successfully!";
                }
            }
        }

        return View(model);
    }

    // ─── 2. ÁP DỤNG COUPON ───────────────────────────────────────────────
    /// <summary>
    /// POST /Cart/ApplyCoupon
    /// Lưu couponCode vào Cookie rồi redirect Index để recompute.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ApplyCoupon(string couponCode, int? courseId)
    {
        if (!string.IsNullOrWhiteSpace(couponCode) && courseId.HasValue)
        {
            var map = GetCouponMapFromCookie();
            map[courseId.Value] = couponCode.Trim().ToUpper();
            SaveCouponMapToCookie(map);
            TempData["PendingCouponCheck"] = courseId.Value;
        }

        return RedirectToAction(nameof(Index));
    }

    // ─── 3. XÓA COUPON ───────────────────────────────────────────────────
    /// <summary>
    /// POST /Cart/RemoveCoupon
    /// Xóa coupon của một khóa học cụ thể.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveCoupon(int courseId)
    {
        var map = GetCouponMapFromCookie();
        if (map.ContainsKey(courseId))
        {
            map.Remove(courseId);
            SaveCouponMapToCookie(map);
            TempData["CartSuccess"] = "Voucher removed successfully.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ─── 4. THÊM VÀO GIỎ ────────────────────────────────────────────────
    /// <summary>
    /// POST /Cart/AddToCart
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int courseId, string? returnUrl = null)
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
            return Redirect("/Account/Login");

        var response = await _api.PostAsync($"cart/add/{courseId}");

        if (!response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            string errorMsg = "An error occurred.";
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var m))
                    errorMsg = m.GetString() ?? errorMsg;
            }
            catch { }
            TempData["CartError"] = errorMsg;
        }
        else
        {
            TempData["CartSuccess"] = "Course added to cart!";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AddToCartAjax(int id)
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
            return Json(new { success = false, message = "Please login first." });

        var response = await _api.PostAsync($"cart/add/{id}");
        if (response.IsSuccessStatusCode)
        {
            return Json(new { success = true });
        }

        var json = await response.Content.ReadAsStringAsync();
        string errorMsg = "Failed to add to cart.";
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var m))
                errorMsg = m.GetString() ?? errorMsg;
        }
        catch { }
        return Json(new { success = false, message = errorMsg });
    }

    // ─── 5. XÓA KHỎI GIỎ ────────────────────────────────────────────────
    /// <summary>
    /// POST /Cart/RemoveFromCart
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromCart(int courseId)
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
            return Redirect("/Account/Login");

        var response = await _api.DeleteAsync($"cart/remove/{courseId}");

        if (!response.IsSuccessStatusCode)
            TempData["CartError"] = "Cannot remove course from cart.";
        else
            TempData["CartSuccess"] = "Course removed from cart.";

        return RedirectToAction(nameof(Index));
    }

    // --- BFF PROXY FOR AJAX --- //
    [HttpGet]
    public async Task<IActionResult> GetSummaryAjax(string? couponCode)
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
            return Unauthorized();
        
        var url = string.IsNullOrWhiteSpace(couponCode)
            ? "cart/summary"
            : $"cart/summary?couponCode={Uri.EscapeDataString(couponCode)}";
        var response = await _api.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        return Content(json, "application/json");
    }

    // ─── 6. CHECKOUT — TRANG THANH TOÁN EMbedded STRIPE ELEMENTS ────────
    /// <summary>
    /// GET /Cart/Checkout
    /// Hiển thị trang thanh toán và tạo Stripe PaymentIntent.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
            return Redirect("/Account/Login");

        // 6.1 Lấy thông tin giỏ hàng hiện tại
        var couponMap = GetCouponMapFromCookie();
        var couponCode = string.Join(",", couponMap.Values.Distinct());

        var url = string.IsNullOrWhiteSpace(couponCode)
            ? "cart/summary"
            : $"cart/summary?couponCode={Uri.EscapeDataString(couponCode)}";

        var summaryResponse = await _api.GetAsync(url);
        var summaryJson = await summaryResponse.Content.ReadAsStringAsync();
        var cartModel = new CartViewModel();

        if (summaryResponse.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(summaryJson))
        {
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using var doc = JsonDocument.Parse(summaryJson);
                if (doc.RootElement.TryGetProperty("data", out var dataEl))
                {
                    cartModel.Items = dataEl.TryGetProperty("items", out var itemsEl)
                        ? JsonSerializer.Deserialize<List<CartItemViewModel>>(itemsEl.GetRawText(), opts) ?? new()
                        : new();

                    cartModel.SubTotal = dataEl.TryGetProperty("subTotal", out var st) ? st.GetDecimal() : 0m;
                    cartModel.DiscountAmount = dataEl.TryGetProperty("discountAmount", out var da) ? da.GetDecimal() : 0m;
                    cartModel.Total = dataEl.TryGetProperty("total", out var tot) ? tot.GetDecimal() : 0m;
                    cartModel.AppliedCouponCode = dataEl.TryGetProperty("appliedCouponCode", out var ac) ? ac.GetString() : null;
                    cartModel.CouponMessage = dataEl.TryGetProperty("couponMessage", out var cm) ? cm.GetString() : null;
                }
            }
            catch { }
        }

        if (!cartModel.Items.Any())
        {
            TempData["CartError"] = "Your cart is empty. Cannot checkout.";
            return RedirectToAction(nameof(Index));
        }

        string clientSecret = "";
        string paymentIntentId = "";

        // 6.3 Lấy thông tin email người dùng từ Profile API
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

        // 6.4 Lấy Stripe PublishableKey
        var publishableKey = _config["Stripe:PublishableKey"] ?? "";

        var model = new CheckoutViewModel
        {
            PublishableKey = publishableKey,
            ClientSecret = clientSecret,
            PaymentIntentId = paymentIntentId,
            Email = userEmail,
            Cart = cartModel
        };

        return View(model);
    }

    // ─── 6.25. DYNAMIC PAYMENT INTENT CREATION VIA AJAX ───────────────────
    [HttpPost]
    public async Task<IActionResult> CreatePaymentIntentAjax(string? couponCode)
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
            return Unauthorized(new { message = "Unauthorized" });

        var response = await _api.PostJsonAsync("checkout/create-intent", new
        {
            couponCode = couponCode
        });

        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            return BadRequest(json);
        }

        return Content(json, "application/json");
    }

    // ─── 6.5. DIRECT CHECKOUT (MUA NGAY KHÔNG QUA GIỎ HÀNG) ──────────────
    [HttpPost]
    public async Task<IActionResult> DirectCheckout(int id)
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
            return Json(new { success = false, message = "Please login before purchasing." });

        var scheme = Request.Headers.ContainsKey("X-Forwarded-Proto") 
            ? Request.Headers["X-Forwarded-Proto"].ToString() 
            : Request.Scheme;
        var host = Request.Headers.ContainsKey("X-Forwarded-Host") 
            ? Request.Headers["X-Forwarded-Host"].ToString() 
            : Request.Host.ToString();
        var baseUrl = $"{scheme}://{host}";

        var response = await _api.PostJsonAsync("checkout/direct", new
        {
            courseId = id,
            successUrl = $"{baseUrl}/Cart/CheckoutSuccess?session_id={{CHECKOUT_SESSION_ID}}",
            cancelUrl = $"{baseUrl}/Cart/CheckoutCancel"
        });

        if (!response.IsSuccessStatusCode)
        {
            var errorJson = await response.Content.ReadAsStringAsync();
            string errorMsg = "An error occurred while creating checkout session.";
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(errorJson);
                if (doc.RootElement.TryGetProperty("message", out var m))
                    errorMsg = m.GetString() ?? errorMsg;
            }
            catch { }
            return Json(new { success = false, message = errorMsg });
        }

        var json = await response.Content.ReadAsStringAsync();
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("data", out var dataEl) &&
                dataEl.TryGetProperty("sessionUrl", out var urlEl))
            {
                var sessionUrl = urlEl.GetString();
                if (!string.IsNullOrEmpty(sessionUrl))
                {
                    return Json(new { success = true, sessionUrl = sessionUrl });
                }
            }
        }
        catch { }

        return Json(new { success = false, message = "Cannot read response data from server." });
    }

    // ─── 7. CHECKOUT SUCCESS — STRIPE REDIRECT VỀ ĐÂY ───────────────────
    /// <summary>
    /// GET /Cart/CheckoutSuccess
    /// Hỗ trợ cả Stripe Checkout (session_id) và Stripe Elements (payment_intent hoặc payment_intent_id).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckoutSuccess(
        [FromQuery(Name = "session_id")] string? sessionId,
        [FromQuery(Name = "payment_intent")] string? paymentIntent,
        [FromQuery(Name = "payment_intent_id")] string? paymentIntentId)
    {
        // 7.1 Ưu tiên kiểm tra PaymentIntent (Stripe Elements) trước
        var targetPaymentIntentId = paymentIntentId ?? paymentIntent;
        if (!string.IsNullOrWhiteSpace(targetPaymentIntentId))
        {
            Console.WriteLine($"[FE-CHECKOUT] ═══ CheckoutSuccess elements CALLED ═══ payment_intent_id={targetPaymentIntentId}");
            try
            {
                Console.WriteLine($"[FE-CHECKOUT] Calling BE: GET checkout/success-intent?payment_intent_id={targetPaymentIntentId}");
                var response = await _api.GetAsync($"checkout/success-intent?payment_intent_id={Uri.EscapeDataString(targetPaymentIntentId)}");
                Console.WriteLine($"[FE-CHECKOUT] BE responded: {(int)response.StatusCode} {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[FE-CHECKOUT] ✅ SUCCESS — redirecting to /Course/MyCourses");
                    TempData["CartSuccess"] = "🎉 Payment successful! Happy learning! Start learning your new courses below.";
                    
                    // Xóa cookie lưu coupon
                    HttpContext.Response.Cookies.Delete(CouponCookieName);
                    return Redirect("/Course/MyCourses");
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[FE-CHECKOUT] ❌ BE ERROR: {errorBody}");
                TempData["CartError"] = "An error occurred while processing payment. Please contact support.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FE-CHECKOUT] ❌ EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                TempData["CartError"] = "An error occurred while processing payment. Please contact support.";
                return RedirectToAction(nameof(Index));
            }
        }

        // 7.2 Fallback về Stripe Checkout Session (cũ)
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            Console.WriteLine($"[FE-CHECKOUT] ═══ CheckoutSuccess session CALLED ═══ session_id={sessionId}");
            try
            {
                Console.WriteLine($"[FE-CHECKOUT] Calling BE: GET checkout/success?session_id={sessionId}");
                var response = await _api.GetAsync($"checkout/success?session_id={Uri.EscapeDataString(sessionId)}");
                Console.WriteLine($"[FE-CHECKOUT] BE responded: {(int)response.StatusCode} {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("[FE-CHECKOUT] ✅ SUCCESS — redirecting to /Course/MyCourses");
                    TempData["CartSuccess"] = "🎉 Payment successful! Happy learning! Start learning your new courses below.";
                    
                    HttpContext.Response.Cookies.Delete(CouponCookieName);
                    return Redirect("/Course/MyCourses");
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[FE-CHECKOUT] ❌ BE ERROR: {errorBody}");
                TempData["CartError"] = "An error occurred while processing payment. Please contact support.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FE-CHECKOUT] ❌ EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                TempData["CartError"] = "An error occurred while processing payment. Please contact support.";
                return RedirectToAction(nameof(Index));
            }
        }

        Console.WriteLine("[FE-CHECKOUT] ❌ No valid session_id or payment_intent_id was found in callback query parameters.");
        TempData["CartError"] = "Invalid checkout session parameters.";
        return RedirectToAction(nameof(Index));
    }

    // ─── 8. CHECKOUT CANCEL — STRIPE REDIRECT VỀ ĐÂY KHI HỦY ─────────────────
    /// <summary>
    /// GET /Cart/CheckoutCancel?order_id=123
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckoutCancel([FromQuery(Name = "order_id")] int orderId)
    {
        if (orderId > 0)
        {
            // Gọi API backend để cập nhật trạng thái
            await _api.GetAsync($"checkout/cancel?order_id={orderId}");
        }

        TempData["CartError"] = "You have not paid successfully or cancelled the transaction. The current order is no longer valid.";
        return RedirectToAction(nameof(Index));
    }
}

