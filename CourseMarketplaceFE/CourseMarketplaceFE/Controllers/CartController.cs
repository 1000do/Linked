using System.Text.Json;
using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceFE.Controllers;

/// <summary>
/// FE MVC Controller cho Giỏ hàng.
/// Dùng Cookie để lưu couponCode tạm thời — nhất quán với AccessToken cookie đang có sẵn.
/// Không cần Session, không cần AddDistributedMemoryCache().
/// ApiClient tự động gắn Bearer Token của user đang login khi gọi BE.
/// </summary>
public class CartController : Controller
{
    private readonly ApiClient _api;

    // Tên cookie lưu mã giảm giá (không nhạy cảm, chỉ là text ngắn)
    private const string CouponCookieName = "cart_coupon";

    public CartController(ApiClient api)
    {
        _api = api;
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

        // Đọc coupon code từ Cookie (nhất quán với cách dùng AccessToken cookie)
        var couponCode = HttpContext.Request.Cookies[CouponCookieName];

        // Gọi API BE summary, truyền couponCode vào query string nếu có
        var url = string.IsNullOrWhiteSpace(couponCode)
            ? "cart/summary"
            : $"cart/summary?couponCode={Uri.EscapeDataString(couponCode)}";

        var response = await _api.GetAsync(url);
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

                    model.SubTotal          = dataEl.TryGetProperty("subTotal",          out var st)  ? st.GetDecimal()  : 0m;
                    model.DiscountAmount    = dataEl.TryGetProperty("discountAmount",     out var da)  ? da.GetDecimal()  : 0m;
                    model.Total             = dataEl.TryGetProperty("total",              out var tot) ? tot.GetDecimal() : 0m;
                    model.AppliedCouponCode = dataEl.TryGetProperty("appliedCouponCode",  out var ac)  ? ac.GetString()  : null;
                    model.CouponMessage     = dataEl.TryGetProperty("couponMessage",      out var cm)  ? cm.GetString()  : null;

                    // Parse danh sách voucher khả dụng (Voucher Wallet)
                    model.AvailableCoupons = dataEl.TryGetProperty("availableCoupons", out var avEl)
                        ? JsonSerializer.Deserialize<List<AvailableCouponViewModel>>(avEl.GetRawText(), opts) ?? new()
                        : new();
                }
            }
            catch { /* json parse lỗi → model rỗng */ }
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
    public IActionResult ApplyCoupon(string couponCode)
    {
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            // Lưu vào Cookie — hết hạn sau 1 giờ, không cần HttpOnly (không nhạy cảm)
            HttpContext.Response.Cookies.Append(CouponCookieName, couponCode.Trim().ToUpper(), new CookieOptions
            {
                Expires  = DateTimeOffset.UtcNow.AddHours(1),
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
                Path     = "/"
            });
        }

        return RedirectToAction(nameof(Index));
    }

    // ─── 3. XÓA COUPON ───────────────────────────────────────────────────
    /// <summary>
    /// POST /Cart/RemoveCoupon
    /// Xóa cookie coupon.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveCoupon()
    {
        HttpContext.Response.Cookies.Delete(CouponCookieName);
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
            string errorMsg = "Có lỗi xảy ra.";
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
            TempData["CartSuccess"] = "Đã thêm khóa học vào giỏ hàng!";
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
            TempData["CartError"] = "Không thể xóa khóa học khỏi giỏ hàng.";
        else
            TempData["CartSuccess"] = "Đã xóa khóa học khỏi giỏ hàng.";

        return RedirectToAction(nameof(Index));
    }

    // ─── 6. CHECKOUT — GỌI API THANH TOÁN ────────────────────────────────
    /// <summary>
    /// POST /Cart/Checkout
    /// Gọi BE API tạo Stripe session → redirect user tới Stripe Checkout Page.
    /// Cookie JWT tự động được ApiClient gắn vào request (DIP — FE không cần biết Stripe).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        if (!HttpContext.Request.Cookies.ContainsKey("AccessToken"))
            return Redirect("/Account/Login");

        // Đọc coupon code từ Cookie (nhất quán với trang giỏ hàng)
        var couponCode = HttpContext.Request.Cookies[CouponCookieName];

        // Tạo base URL của FE (dựa vào Request context)
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        
        // Gọi BE API checkout/process
        var response = await _api.PostJsonAsync("checkout/process", new
        {
            couponCode = couponCode,
            successUrl = $"{baseUrl}/Cart/CheckoutSuccess?session_id={{CHECKOUT_SESSION_ID}}",
            cancelUrl = $"{baseUrl}/Cart"
        });

        if (!response.IsSuccessStatusCode)
        {
            var errorJson = await response.Content.ReadAsStringAsync();
            string errorMsg = "Có lỗi xảy ra khi tạo phiên thanh toán.";
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(errorJson);
                if (doc.RootElement.TryGetProperty("message", out var m))
                    errorMsg = m.GetString() ?? errorMsg;
            }
            catch { }
            TempData["CartError"] = errorMsg;
            return RedirectToAction(nameof(Index));
        }

        // Parse response để lấy Stripe Session URL
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
                    // Xóa coupon cookie sau khi checkout thành công
                    HttpContext.Response.Cookies.Delete(CouponCookieName);

                    // Redirect tới Stripe Checkout Page
                    return Redirect(sessionUrl);
                }
            }
        }
        catch { }

        TempData["CartError"] = "Không thể tạo phiên thanh toán. Vui lòng thử lại.";
        return RedirectToAction(nameof(Index));
    }

    // ─── 7. CHECKOUT SUCCESS — STRIPE REDIRECT VỀ ĐÂY ───────────────────
    /// <summary>
    /// GET /Cart/CheckoutSuccess?session_id=cs_xxx
    /// Stripe redirect user về đây sau khi thanh toán thành công.
    /// Gọi BE API để finalize (cấp enrollment, chia tiền...) rồi redirect My Learning.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckoutSuccess([FromQuery(Name = "session_id")] string sessionId)
    {
        Console.WriteLine($"[FE-CHECKOUT] ═══ CheckoutSuccess CALLED ═══ session_id={sessionId}");

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            Console.WriteLine("[FE-CHECKOUT] ❌ session_id is EMPTY!");
            TempData["CartError"] = "Phiên thanh toán không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            Console.WriteLine($"[FE-CHECKOUT] Calling BE: GET checkout/success?session_id={sessionId}");
            
            // Gọi BE API success endpoint
            var response = await _api.GetAsync($"checkout/success?session_id={Uri.EscapeDataString(sessionId)}");
            
            Console.WriteLine($"[FE-CHECKOUT] BE responded: {(int)response.StatusCode} {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("[FE-CHECKOUT] ✅ SUCCESS — redirecting to /Course");
                TempData["CartSuccess"] = "🎉 Thanh toán thành công! Chúc bạn học vui vẻ!";
                return Redirect("/Course");
            }

            // ★ Log chi tiết lỗi từ BE
            var errorBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[FE-CHECKOUT] ❌ BE ERROR: {errorBody}");

            TempData["CartError"] = "Có lỗi xảy ra khi xử lý thanh toán. Vui lòng liên hệ hỗ trợ.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FE-CHECKOUT] ❌ EXCEPTION: {ex.Message}\n{ex.StackTrace}");
            TempData["CartError"] = "Có lỗi xảy ra khi xử lý thanh toán. Vui lòng liên hệ hỗ trợ.";
            return RedirectToAction(nameof(Index));
        }
    }
}

