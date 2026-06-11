using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GiftController : ControllerBase
{
    private readonly IGiftService _giftService;

    public GiftController(IGiftService giftService)
    {
        _giftService = giftService;
    }

    private int? GetUserId()
    {
        var str = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(str, out int id) ? id : null;
    }

    [HttpGet("validate/{token}")]
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

    [HttpPost("claim")]
    [Authorize]
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

    [HttpGet("check-recipient")]
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
}
