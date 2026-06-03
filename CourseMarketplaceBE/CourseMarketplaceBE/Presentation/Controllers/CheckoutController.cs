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
    private readonly IAuthService _authService;

    public CheckoutController(ICheckoutService checkoutService, IAuthService authService)
    {
        _checkoutService = checkoutService;
        _authService = authService;
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
            return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid login session."));

        if (!await _authService.IsEmailVerifiedAsync(userId.Value))
            return BadRequest(ApiResponse<string>.ErrorResponse("Please verify your email address."));

        try
        {
            // Lấy trực tiếp từ request DTO do FE truyền vào (để giải quyết vấn đề DNS/Docker network)
            if (string.IsNullOrWhiteSpace(request.SuccessUrl) || string.IsNullOrWhiteSpace(request.CancelUrl))
                return BadRequest(ApiResponse<string>.ErrorResponse("SuccessUrl or CancelUrl is missing."));

            var result = await _checkoutService.InitiateCheckoutAsync(
                userId.Value,
                request.CouponCode,
                request.SuccessUrl,
                request.CancelUrl);

            return Ok(ApiResponse<CheckoutResponse>.SuccessResponse(result, "Checkout session created."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Server error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/checkout/create-intent
    // Tạo Stripe PaymentIntent và trả về Client Secret cho FE.
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("create-intent")]
    public async Task<IActionResult> CreateIntent([FromBody] CheckoutRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid login session."));

        if (!await _authService.IsEmailVerifiedAsync(userId.Value))
            return BadRequest(ApiResponse<string>.ErrorResponse("Please verify your email address."));

        try
        {
            var result = await _checkoutService.InitiatePaymentIntentAsync(userId.Value, request.CouponCode);
            return Ok(ApiResponse<CheckoutResponse>.SuccessResponse(result, "Payment intent created successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Server error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/checkout/direct
    // Mua trực tiếp 1 khóa học sử dụng Destination Charges (bỏ giỏ hàng)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("direct")]
    public async Task<IActionResult> DirectProcess([FromBody] DirectCheckoutRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid login session."));

        if (!await _authService.IsEmailVerifiedAsync(userId.Value))
            return BadRequest(ApiResponse<string>.ErrorResponse("Please verify your email address."));

        try
        {
            if (string.IsNullOrWhiteSpace(request.SuccessUrl) || string.IsNullOrWhiteSpace(request.CancelUrl))
                return BadRequest(ApiResponse<string>.ErrorResponse("SuccessUrl or CancelUrl is missing."));

            var result = await _checkoutService.InitiateDirectCheckoutAsync(
                userId.Value,
                request.CourseId,
                request.CouponCode,
                request.SuccessUrl,
                request.CancelUrl);

            return Ok(ApiResponse<CheckoutResponse>.SuccessResponse(result, "Checkout session created."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Server error: {ex.Message}"));
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
            return BadRequest(ApiResponse<string>.ErrorResponse("Missing session_id."));

        try
        {
            Console.WriteLine($"[BE-CONTROLLER] Calling _checkoutService.ProcessPaymentSuccessAsync...");
            await _checkoutService.ProcessPaymentSuccessAsync(sessionId);
            Console.WriteLine($"[BE-CONTROLLER] ✅ ProcessPaymentSuccessAsync completed WITHOUT exception.");
            return Ok(ApiResponse<string>.SuccessResponse("Payment successful and course access granted."));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[BE-CONTROLLER] ❌ InvalidOperationException: {ex.Message}");
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BE-CONTROLLER] ❌ EXCEPTION: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Payment processing error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/checkout/success-intent?payment_intent_id=pi_xxx
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("success-intent")]
    public async Task<IActionResult> SuccessIntent([FromQuery(Name = "payment_intent_id")] string paymentIntentId)
    {
        Console.WriteLine($"[BE-CONTROLLER] ═══ CheckoutController.SuccessIntent ENTRY ═══ payment_intent_id={paymentIntentId}");

        if (string.IsNullOrWhiteSpace(paymentIntentId))
            return BadRequest(ApiResponse<string>.ErrorResponse("Missing payment_intent_id."));

        try
        {
            Console.WriteLine($"[BE-CONTROLLER] Calling _checkoutService.ProcessPaymentIntentSuccessAsync...");
            await _checkoutService.ProcessPaymentIntentSuccessAsync(paymentIntentId);
            Console.WriteLine($"[BE-CONTROLLER] ✅ ProcessPaymentIntentSuccessAsync completed WITHOUT exception.");
            return Ok(ApiResponse<string>.SuccessResponse("Payment successful and course access granted."));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[BE-CONTROLLER] ❌ InvalidOperationException: {ex.Message}");
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BE-CONTROLLER] ❌ EXCEPTION: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Payment processing error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/checkout/cancel?order_id=123
    // Frontend gọi API này sau khi user hủy thanh toán trên Stripe.
    // ═══════════════════════════════════════════════════════════════════════
    /// <summary>
    // Xử lý khi user bấm "Quay lại" hoặc hủy trên trang thanh toán của Stripe.
    /// Update Order/Transaction thành failed để không bị pending mãi mãi.
    /// </summary>
    [HttpGet("cancel")]
    public async Task<IActionResult> Cancel([FromQuery(Name = "order_id")] int orderId = 0)
    {
        Console.WriteLine($"[BE-CONTROLLER] ═══ CheckoutController.Cancel ENTRY ═══ order_id={orderId}");

        if (orderId <= 0)
            return BadRequest(ApiResponse<string>.ErrorResponse("Missing valid order_id."));

        try
        {
            // Không còn rác pending trong DB để dọn dẹp nên trả về thành công luôn
            await _checkoutService.ProcessPaymentCancelAsync(orderId);
            return Ok(ApiResponse<string>.SuccessResponse("Payment cancellation status updated."));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BE-CONTROLLER] ❌ EXCEPTION: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error updating payment cancellation: {ex.Message}"));
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
            return BadRequest(ApiResponse<string>.ErrorResponse("Missing session_id."));

        try
        {
            await _checkoutService.ProcessPaymentSuccessAsync(sessionId, true);
            return Ok(ApiResponse<string>.SuccessResponse("Transfers retry completed successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Retry error: {ex.Message}"));
        }
    }

}
