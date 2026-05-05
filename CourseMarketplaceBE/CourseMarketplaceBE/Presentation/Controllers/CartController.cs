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
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
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
    public async Task<IActionResult> AddToCart(int courseId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.ErrorResponse("Phiên đăng nhập không hợp lệ."));

        try
        {
            await _cartService.AddToCartAsync(userId.Value, courseId);
            return Ok(ApiResponse<string>.SuccessResponse("ok", "Đã thêm khóa học vào giỏ hàng."));
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

    // ─── 2. XÓA KHÓA HỌC KHỎI GIỎ ──────────────────────────────────────
    /// <summary>
    /// DELETE /api/cart/remove/{courseId}
    /// Xóa courseId khỏi giỏ của người đang login.
    /// </summary>
    [HttpDelete("remove/{courseId:int}")]
    public async Task<IActionResult> RemoveFromCart(int courseId)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.ErrorResponse("Phiên đăng nhập không hợp lệ."));

        try
        {
            await _cartService.RemoveFromCartAsync(userId.Value, courseId);
            return Ok(ApiResponse<string>.SuccessResponse("ok", "Đã xóa khóa học khỏi giỏ hàng."));
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

    // ─── 3. XEM GIỎ HÀNG + TÍNH COUPON ─────────────────────────────────
    /// <summary>
    /// GET /api/cart/summary?couponCode=SALE20
    /// Trả về danh sách sản phẩm, SubTotal, DiscountAmount, Total.
    /// couponCode là optional — không truyền = không áp dụng coupon.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string? couponCode)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.ErrorResponse("Phiên đăng nhập không hợp lệ."));

        try
        {
            var summary = await _cartService.GetCartSummaryAsync(userId.Value, couponCode);
            return Ok(ApiResponse<CartSummaryResponse>.SuccessResponse(summary));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi server: {ex.Message}"));
        }
    }
}
