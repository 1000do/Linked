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
    /// Gọi API BE rồi redirect về giỏ hàng (hoặc returnUrl nếu có).
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

        // Quay lại trang chi tiết khóa học nếu có returnUrl hợp lệ
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
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
}
