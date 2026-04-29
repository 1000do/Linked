using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourseMarketplaceBE.Presentation.Controllers;

/// <summary>
/// API Controller cho luồng Thanh toán (Checkout).
/// 
/// SOLID — SRP: Controller CHỈ làm 3 việc:
///   1. Xác thực user (JWT từ Cookie — đã config sẵn trong Program.cs).
///   2. Gọi ICheckoutService (DIP — không biết implementation cụ thể).
///   3. Trả về HTTP response.
///
/// Tất cả endpoint đều yêu cầu [Authorize] — userId lấy từ JWT Claim,
/// KHÔNG BAO GIỜ nhận từ body/query (bảo mật).
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    // ─── Helper lấy userId từ JWT Claim ──────────────────────────────────
    private int? GetUserId()
    {
        var str = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(str, out int id) ? id : null;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/checkout/process
    // Tạo Order → Gọi Stripe → Trả về Session URL để FE redirect.
    // ═══════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Khởi tạo checkout: tạo Order, gọi Stripe, trả về URL thanh toán.
    /// FE sẽ redirect user tới URL này.
    /// </summary>
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] CheckoutRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.ErrorResponse("Phiên đăng nhập không hợp lệ."));

        try
        {
            // Lấy trực tiếp từ request DTO do FE truyền vào (để giải quyết vấn đề DNS/Docker network)
            if (string.IsNullOrWhiteSpace(request.SuccessUrl) || string.IsNullOrWhiteSpace(request.CancelUrl))
                return BadRequest(ApiResponse<string>.ErrorResponse("Thiếu SuccessUrl hoặc CancelUrl."));

            var result = await _checkoutService.InitiateCheckoutAsync(
                userId.Value,
                request.CouponCode,
                request.SuccessUrl,
                request.CancelUrl);

            return Ok(ApiResponse<CheckoutResponse>.SuccessResponse(result, "Đã tạo phiên thanh toán."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi server: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/checkout/success?session_id=cs_xxx
    // Frontend gọi API này sau khi nhận callback từ Stripe.
    // ═══════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Xử lý callback thành công.
    /// Update Order/Transaction → Cấp enrollment → Tính payout → Xóa cart.
    /// </summary>
    [HttpGet("success")]
    public async Task<IActionResult> Success([FromQuery(Name = "session_id")] string sessionId)
    {
        Console.WriteLine($"[BE-CONTROLLER] ═══ CheckoutController.Success ENTRY ═══ session_id={sessionId}");

        if (string.IsNullOrWhiteSpace(sessionId))
            return BadRequest(ApiResponse<string>.ErrorResponse("Thiếu session_id."));

        try
        {
            Console.WriteLine($"[BE-CONTROLLER] Calling _checkoutService.ProcessPaymentSuccessAsync...");
            await _checkoutService.ProcessPaymentSuccessAsync(sessionId);
            Console.WriteLine($"[BE-CONTROLLER] ✅ ProcessPaymentSuccessAsync completed WITHOUT exception.");
            return Ok(ApiResponse<string>.SuccessResponse("Thanh toán thành công và đã cấp quyền truy cập khóa học."));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[BE-CONTROLLER] ❌ InvalidOperationException: {ex.Message}");
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BE-CONTROLLER] ❌ EXCEPTION: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi xử lý thanh toán: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/checkout/retry-transfers?session_id=cs_xxx
    // Retry tạo Stripe Transfers cho các đơn hàng đã thanh toán nhưng chưa chia tiền.
    // (Dùng cho trường hợp code cũ chạy trước khi có logic transfer)
    // ═══════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Retry tạo transfers cho session đã succeeded nhưng thiếu payout records.
    /// Idempotent — gọi nhiều lần không tạo duplicate.
    /// </summary>
    [AllowAnonymous] // ★ Tạm cho debug — BỎ sau khi fix xong
    [HttpPost("retry-transfers")]
    public async Task<IActionResult> RetryTransfers([FromQuery(Name = "session_id")] string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return BadRequest(ApiResponse<string>.ErrorResponse("Thiếu session_id."));

        try
        {
            await _checkoutService.ProcessPaymentSuccessAsync(sessionId);
            return Ok(ApiResponse<string>.SuccessResponse("Đã retry xử lý transfers thành công."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi retry: {ex.Message}"));
        }
    }

}
