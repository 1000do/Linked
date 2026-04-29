using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

/// <summary>
/// API Controller cho Module Quản lý Tài chính Admin.
/// </summary>
[Route("api/admin/finance")]
[ApiController]
[Authorize]
public class AdminFinanceController : ControllerBase
{
    private readonly IAdminFinanceService _financeService;
    private readonly IInstructorService _instructorService;

    public AdminFinanceController(
        IAdminFinanceService financeService,
        IInstructorService instructorService)
    {
        _financeService = financeService;
        _instructorService = instructorService;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/admin/finance/summary
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("summary")]
    public async Task<IActionResult> GetFinancialSummary()
    {
        try
        {
            var summary = await _financeService.GetFinancialSummaryAsync();
            return Ok(ApiResponse<FinancialSummaryResponse>.SuccessResponse(summary, "Thống kê tài chính."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/admin/finance/payouts
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("payouts")]
    public async Task<IActionResult> GetPayouts()
    {
        try
        {
            var payouts = await _financeService.GetInstructorPayoutsAsync();
            return Ok(ApiResponse<List<PayoutDetailResponse>>.SuccessResponse(payouts, "Danh sách chia tiền."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/admin/finance/transfer-rate
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("transfer-rate")]
    public async Task<IActionResult> GetTransferRate()
    {
        try
        {
            var rate = await _financeService.GetCurrentTransferRateAsync();
            return Ok(ApiResponse<decimal>.SuccessResponse(rate, "Tỷ lệ chia sẻ hiện tại."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/admin/finance/transfer-rate
    // UC-120: Cập nhật tỷ lệ chia sẻ doanh thu
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("transfer-rate")]
    public async Task<IActionResult> SetTransferRate([FromBody] SetTransferRateRequest request)
    {
        try
        {
            await _financeService.SetTransferRateAsync(request.Rate);
            return Ok(ApiResponse<string>.SuccessResponse(
                $"Đã cập nhật: Giảng viên nhận {request.Rate}%, Sàn nhận {100 - request.Rate}%.",
                "Cập nhật thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/admin/finance/reset-stripe/{instructorId}
    // ★ Reset Stripe account bị lỗi region → xóa account cũ, cho phép tạo lại
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("reset-stripe/{instructorId:int}")]
    public async Task<IActionResult> ResetStripeAccount(int instructorId)
    {
        try
        {
            var result = await _instructorService.ResetStripeAccountAsync(instructorId);
            return Ok(ApiResponse<string>.SuccessResponse(result, "Reset thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi: {ex.Message}"));
        }
    }
}
