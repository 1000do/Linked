using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseMarketplaceBE.Presentation.Filters;

namespace CourseMarketplaceBE.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GiftController : ControllerBase
{
    private readonly IGiftService _giftService;
    private readonly ICourseQueryService _courseQueryService;

    public GiftController(IGiftService giftService, ICourseQueryService courseQueryService)
    {
        _giftService = giftService;
        _courseQueryService = courseQueryService;
    }

    private int? GetUserId()
    {
        var str = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(str, out int id) ? id : null;
    }

    // UC-19: Validate gift token — public, không cần auth (người nhận check trước khi login)
    [HttpGet("validate/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateGiftToken(string token)
    {
        try
        {
            var response = await _giftService.ValidateGiftTokenAsync(token);
            return Ok(ApiResponse<GiftValidationResponse>.SuccessResponse(response));
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

    // UC-19: Receive a Gift — User hoặc Instructor đã đăng nhập mới được claim
    [HttpPost("claim")]
    [CustomAuthorize(requireAuth: true, "user", "instructor")]
    public async Task<IActionResult> ClaimGift([FromBody] GiftClaimRequest request)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid login session. Please log in."));

        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.ErrorResponse("Token is required."));

        try
        {
            await _giftService.ClaimGiftAsync(userId.Value, request.RedemptionToken);
            return Ok(ApiResponse<string>.SuccessResponse("ok", "Course claimed successfully. Added to your library."));
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

    // UC-17: Gift Courses — Người tặng (user/instructor) kiểm tra người nhận trước khi tặng
    [HttpGet("check-recipient")]
    [CustomAuthorize(requireAuth: true, "user", "instructor")]
    public async Task<IActionResult> CheckRecipientEnrollment([FromQuery] string email, [FromQuery] int courseId)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(ApiResponse<string>.ErrorResponse("Email is required."));

        try
        {
            var isEnrolled = await _giftService.IsRecipientEnrolledAsync(email, courseId);
            return Ok(ApiResponse<bool>.SuccessResponse(isEnrolled));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    // UC-17/19: Lấy thông tin khóa học để hiển thị trang Gift/Claim — cần đăng nhập
    [HttpGet("course/{id}")]
    [CustomAuthorize(requireAuth: true, "user", "instructor")]
    public async Task<IActionResult> GetCourseDetails(int id)
    {
        try
        {
            var userId = GetUserId();
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var course = await _courseQueryService.GetCourseWithDetailsAsync(id, userId, role);
            return Ok(ApiResponse<object>.SuccessResponse(course, "Retrieved course successfully."));
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}
