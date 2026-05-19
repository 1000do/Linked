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
[Authorize(Roles = "admin")]
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
            return Ok(ApiResponse<FinancialSummaryResponse>.SuccessResponse(summary, "Financial statistics."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
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
            return Ok(ApiResponse<List<PayoutDetailResponse>>.SuccessResponse(payouts, "Payout list."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
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
            return Ok(ApiResponse<decimal>.SuccessResponse(rate, "Current share rate."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
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
                $"Updated: Instructor receives {request.Rate}%, Platform receives {100 - request.Rate}%.",
                "Updated successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/admin/finance/payouts/{payoutId}/mark-paid
    // Đánh dấu khoản thanh toán là đã thanh toán
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("payouts/{payoutId:int}/mark-paid")]
    public async Task<IActionResult> MarkPayoutAsPaid(int payoutId)
    {
        try
        {
            await _financeService.MarkPayoutAsPaidAsync(payoutId);
            return Ok(ApiResponse<string>.SuccessResponse("Marked this payment as Paid.", "Success."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/admin/finance/payouts/{payoutId}/stripe-transfer
    // Chuyển tiền thật qua Stripe Connect
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("payouts/{payoutId:int}/stripe-transfer")]
    public async Task<IActionResult> TransferViaStripe(int payoutId)
    {
        try
        {
            var transferId = await _financeService.PerformStripeTransferAsync(payoutId);
            return Ok(ApiResponse<string>.SuccessResponse(transferId, "Funds transferred successfully via Stripe."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"System error: {ex.Message}"));
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
            return Ok(ApiResponse<string>.SuccessResponse(result, "Reset successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PLATFORM WITHDRAWAL — Rút tiền lợi nhuận Sàn về ngân hàng
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>GET /api/admin/finance/balance — Lấy số dư Stripe Platform</summary>
    [HttpGet("balance")]
    public async Task<IActionResult> GetPlatformBalance()
    {
        try
        {
            var balance = await _financeService.GetPlatformBalanceAsync();
            return Ok(ApiResponse<PlatformBalanceResponse>.SuccessResponse(balance, "Stripe Platform balance."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    /// <summary>POST /api/admin/finance/withdraw — Tạo lệnh rút tiền</summary>
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
    {
        try
        {
            // Lấy manager ID từ JWT claims
            var accountIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int managerId = accountIdClaim != null ? int.Parse(accountIdClaim) : 0;

            var result = await _financeService.CreateWithdrawalAsync(request, managerId);
            return Ok(ApiResponse<WithdrawResponse>.SuccessResponse(result, "Withdrawal request created successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    /// <summary>GET /api/admin/finance/withdrawals — Lịch sử rút tiền</summary>
    [HttpGet("withdrawals")]
    public async Task<IActionResult> GetWithdrawalHistory()
    {
        try
        {
            var history = await _financeService.GetWithdrawalHistoryAsync();
            return Ok(ApiResponse<List<WithdrawalHistoryItem>>.SuccessResponse(history, "Withdrawal history."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/admin/finance/transactions/{transactionId}/refund
    // Hoàn tiền toàn bộ cho 1 giao dịch qua Stripe
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("transactions/{transactionId:int}/refund")]
    public async Task<IActionResult> RefundTransaction(int transactionId, [FromBody] RefundRequest? request)
    {
        try
        {
            var result = await _financeService.RefundTransactionAsync(transactionId, request?.Reason);
            return Ok(ApiResponse<RefundResultResponse>.SuccessResponse(result, "Refunded successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"System error: {ex.Message}"));
        }
    }
}
