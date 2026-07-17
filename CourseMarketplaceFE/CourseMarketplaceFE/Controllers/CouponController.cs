using CourseMarketplaceFE.Helpers;
using CourseMarketplaceFE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CourseMarketplaceFE.Controllers
{
    public class CouponController : Controller
    {
        private readonly ApiClient _api;

        public CouponController(ApiClient api)
        {
            _api = api;
        }

        // ── Razor Views ───────────────────────────────────────────────────────
        public IActionResult Index() => View();
        [Authorize(Roles = "admin")]
        public IActionResult Admin() => View();

        // ── Proxy APIs (for JavaScript – bypasses HttpOnly cookie & Docker networking) ──

        /// <summary>
        /// Proxy: GET /Coupon/PlatformActive → Backend api/coupon/platform/active
        /// Called by JS in Editor.cshtml and Create.cshtml
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PlatformActive()
        {
            var resp = await _api.GetAsync("coupon/platform/active");
            var body = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body)) {
                body = "{\"success\":false,\"message\":\"Error or unauthorized access.\"}";
            }
            return Content(body, "application/json");
        }

        /// <summary>
        /// Proxy: POST /Coupon/PlatformApply → Backend api/coupon/platform/apply
        /// Body: { courseId, couponId }
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PlatformApply([FromBody] ApplyCouponRequest req)
        {
            var resp = await _api.PostJsonAsync("coupon/platform/apply", req);
            var body = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body)) {
                body = "{\"success\":false,\"message\":\"Error or unauthorized access.\"}";
            }
            return Content(body, "application/json");
        }

        /// <summary>
        /// Proxy: DELETE /Coupon/PlatformRemove/{courseId} → Backend api/coupon/platform/remove/{courseId}
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> PlatformRemove([FromQuery] int courseId)
        {
            var resp = await _api.DeleteAsync($"coupon/platform/remove/{courseId}");
            var body = await resp.Content.ReadAsStringAsync();
            return Content(body, "application/json");
        }
    }
}
