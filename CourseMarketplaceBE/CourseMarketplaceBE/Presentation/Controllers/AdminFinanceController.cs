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
    public async Task<IActionResult> GetFinancialSummary([FromQuery] int? year, [FromQuery] int? month)
    {
        try
        {
            var summary = await _financeService.GetFinancialSummaryAsync(year, month);
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
    public async Task<IActionResult> GetPayouts([FromQuery] int? year, [FromQuery] int? month, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var payouts = await _financeService.GetInstructorPayoutsAsync(year, month, page, pageSize);
            return Ok(ApiResponse<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<PayoutDetailResponse>>.SuccessResponse(payouts, "Payout list."));
            
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
    // GET /api/admin/finance/payout-days
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("payout-days")]
    public async Task<IActionResult> GetPayoutDays()
    {
        try
        {
            var days = await _financeService.GetPayoutDaysConfigAsync();
            return Ok(ApiResponse<string>.SuccessResponse(days, "Current payout days."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/admin/finance/payout-days
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("payout-days")]
    public async Task<IActionResult> SetPayoutDays([FromBody] SetPayoutDaysRequest request)
    {
        try
        {
            await _financeService.SetPayoutDaysConfigAsync(request.PayoutDays);
            return Ok(ApiResponse<string>.SuccessResponse(
                $"Updated payout days to: {request.PayoutDays}",
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

    public class SetPayoutDaysRequest
    {
        public string PayoutDays { get; set; } = "15";
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
    // POST /api/admin/finance/payouts/sync-all
    // Đồng bộ tất cả Payouts của giảng viên từ Stripe
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("payouts/sync-all")]
    public async Task<IActionResult> SyncAllPayouts()
    {
        try
        {
            await _financeService.SyncAllPayoutsWithStripeAsync();
            return Ok(ApiResponse<string>.SuccessResponse("Synchronized payout status successfully.", "Success."));
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
    public async Task<IActionResult> GetWithdrawalHistory([FromQuery] int? year, [FromQuery] int? month, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var history = await _financeService.GetWithdrawalHistoryAsync(year, month, page, pageSize);
            return Ok(ApiResponse<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<WithdrawalHistoryItem>>.SuccessResponse(history,"Withdrawal history."));
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

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/admin/finance/refunds/pending
    // Lấy danh sách các yêu cầu hoàn tiền đang chờ duyệt
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("refunds/pending")]
    public async Task<IActionResult> GetPendingRefundRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var list = await _financeService.GetPendingRefundRequestsAsync(page, pageSize);
            return Ok(ApiResponse<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<Domain.Entities.Transaction>>.SuccessResponse(list, "List of pending refund requests."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/admin/finance/refunds/{transactionId}/approve
    // Phê duyệt yêu cầu hoàn tiền (gọi Stripe)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("refunds/{transactionId:int}/approve")]
    public async Task<IActionResult> ApproveRefund(int transactionId, [FromBody] RefundDecisionRequest request)
    {
        try
        {
            await _financeService.ApproveRefundAsync(transactionId, request.AdminNote ?? "Approved by Admin.");
            return Ok(ApiResponse<string>.SuccessResponse("Refund approved successfully."));
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
    // POST /api/admin/finance/refunds/{transactionId}/reject
    // Từ chối yêu cầu hoàn tiền
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("refunds/{transactionId:int}/reject")]
    public async Task<IActionResult> RejectRefund(int transactionId, [FromBody] RefundDecisionRequest request)
    {
        try
        {
            await _financeService.RejectRefundAsync(transactionId, request.AdminNote ?? "Rejected by Admin.");
            return Ok(ApiResponse<string>.SuccessResponse("Refund request rejected."));
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
    // GET /api/admin/finance/instructor-courses-revenue
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("instructor-courses-revenue")]
    public async Task<IActionResult> GetInstructorCourseRevenues([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var revenues = await _financeService.GetInstructorCourseRevenuesAsync(year, month);
            return Ok(ApiResponse<List<InstructorCourseRevenueResponse>>.SuccessResponse(revenues, "Instructor course revenues list."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }
}

public class RefundDecisionRequest
{
    public string? AdminNote { get; set; }
}
