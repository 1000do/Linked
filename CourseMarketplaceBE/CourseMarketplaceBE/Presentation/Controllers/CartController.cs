using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourseMarketplaceBE.Presentation.Controllers;

/// <summary>
/// API Controller cho chức năng Giỏ hàng (Cart).
/// Tất cả endpoint đều yêu cầu JWT hợp lệ [Authorize].
/// userId KHÔNG BAO GIỜ nhận từ body/query — luôn lấy từ JWT Claim.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly IAuthService _authService;

    public CartController(ICartService cartService, IAuthService authService)
    {
        _cartService = cartService;
        _authService = authService;
    }

    // ─── Lấy userId từ JWT Claim (helper dùng chung) ─────────────────────
    private int? GetUserId()
    {
        var str = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(str, out int id) ? id : null;
    }

    // ─── 1. THÊM KHÓA HỌC VÀO GIỎ ───────────────────────────────────────
    /// <summary>
    /// POST /api/cart/add/{courseId}
    /// Thêm courseId vào giỏ của người đang login.
    /// </summary>
    [HttpPost("add/{courseId:int}")]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> AddToCart(int courseId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid login session."));

        if (!await _authService.IsEmailVerifiedAsync(userId.Value))
            return BadRequest(ApiResponse<string>.ErrorResponse("Please verify your email address."));

        try
        {
            await _cartService.AddToCartAsync(userId.Value, courseId);
            return Ok(ApiResponse<string>.SuccessResponse("ok", "Course added to cart successfully."));
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

    // ─── 2. XÓA KHÓA HỌC KHỎI GIỎ ──────────────────────────────────────
    /// <summary>
    /// DELETE /api/cart/remove/{courseId}
    /// Xóa courseId khỏi giỏ của người đang login.
    /// </summary>
    [HttpDelete("remove/{courseId:int}")]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> RemoveFromCart(int courseId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid login session."));

        try
        {
            await _cartService.RemoveFromCartAsync(userId.Value, courseId);
            return Ok(ApiResponse<string>.SuccessResponse("ok", "Course removed from cart successfully."));
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

    // ─── 3. XEM GIỎ HÀNG + TÍNH COUPON ─────────────────────────────────
    /// <summary>
    /// GET /api/cart/summary?couponCode=SALE20
    /// Trả về danh sách sản phẩm, SubTotal, DiscountAmount, Total.
    /// couponCode là optional — không truyền = không áp dụng coupon.
    /// </summary>
    [HttpGet("summary")]
    [Authorize(Roles = "user,instructor")]
    public async Task<IActionResult> GetSummary([FromQuery] string? couponCode)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid login session."));

        try
        {
            var summary = await _cartService.GetCartSummaryAsync(userId.Value, couponCode);
            return Ok(ApiResponse<CartSummaryResponse>.SuccessResponse(summary));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            var fullError = ex.InnerException != null
                ? $"{ex.Message} → {ex.InnerException.Message}"
                : ex.Message;
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Server error: {fullError} | StackTrace: {ex.StackTrace}"));
        }
    }
}
