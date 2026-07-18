using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseMarketplaceBE.Presentation.Filters;

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
    // UC-136 (2.2.3.136): Filter Financial Dashboard by Period — Chỉ Admin
    [HttpGet("summary")]
    [CustomAuthorize(requireAuth: true, "admin")]
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
    // UC-124: View Instructor Payout — Chỉ Admin
    [HttpGet("payouts")]
    [CustomAuthorize(requireAuth: true, "admin")]
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
    [AllowAnonymous] // Cho phép ghi đè [Authorize(Roles="admin")] của Controller
    [CustomAuthorize(requireAuth: true, "admin", "instructor","user")] // Chỉ Admin và Giảng viên mới được gọi API này
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
    // UC-134: Set Transfer Rate — Chỉ Admin
    [HttpPost("transfer-rate")]
    [CustomAuthorize(requireAuth: true, "admin")]
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
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to set transfer rate"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/admin/finance/payout-days
    // ═══════════════════════════════════════════════════════════════════════
    // UC-135: Set Payout Schedule — Chỉ Admin
    [HttpGet("payout-days")]
    [CustomAuthorize(requireAuth: true, "admin")]
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
    // UC-135: Set Payout Schedule — Chỉ Admin
    [HttpPost("payout-days")]
    [CustomAuthorize(requireAuth: true, "admin")]
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
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to set payout days"));
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
    // UC-124: Mark Payout Paid — Chỉ Admin
    [HttpPost("payouts/{payoutId:int}/mark-paid")]
    [CustomAuthorize(requireAuth: true, "admin")]
    public async Task<IActionResult> MarkPayoutAsPaid(int payoutId)
    {
        try
        {
            await _financeService.MarkPayoutAsPaidAsync(payoutId);
            return Ok(ApiResponse<string>.SuccessResponse("Marked this payment as Paid.", "Success."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to mark payout as paid"));
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
    // UC-124: Stripe Transfer (Pay Instructor) — Chỉ Admin
    [HttpPost("payouts/{payoutId:int}/stripe-transfer")]
    [CustomAuthorize(requireAuth: true, "admin")]
    public async Task<IActionResult> TransferViaStripe(int payoutId)
    {
        try
        {
            var transferId = await _financeService.PerformStripeTransferAsync(payoutId);
            return Ok(ApiResponse<string>.SuccessResponse(transferId, "Funds transferred successfully via Stripe."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to transfer via Stripe"));
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
    // UC-124: Sync All Payouts — Chỉ Admin
    [HttpPost("payouts/sync-all")]
    [CustomAuthorize(requireAuth: true, "admin")]
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
    // Admin reset Stripe account Instructor bị lỗi — Chỉ Admin
    [HttpPost("reset-stripe/{instructorId:int}")]
    [CustomAuthorize(requireAuth: true, "admin")]
    public async Task<IActionResult> ResetStripeAccount(int instructorId)
    {
        try
        {
            var result = await _instructorService.ResetStripeAccountAsync(instructorId);
            return Ok(ApiResponse<string>.SuccessResponse(result, "Reset successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to reset stripe account"));
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
    // UC-123: View Balance (For System) — Chỉ Admin
    [HttpGet("balance")]
    [CustomAuthorize(requireAuth: true, "admin")]
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
    // UC-136: Withdraw Funds — Chỉ Admin
    [HttpPost("withdraw")]
    [CustomAuthorize(requireAuth: true, "admin")]
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
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to withdraw"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    /// <summary>GET /api/admin/finance/withdrawals — Lịch sử rút tiền</summary>
    // UC-125: View Funds Withdrawal History — Chỉ Admin
    [HttpGet("withdrawals")]
    [CustomAuthorize(requireAuth: true, "admin")]
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
    // Admin Refund trực tiếp 1 transaction — Chỉ Admin
    [HttpPost("transactions/{transactionId:int}/refund")]
    [CustomAuthorize(requireAuth: true, "admin")]
    public async Task<IActionResult> RefundTransaction(int transactionId, [FromBody] RefundRequest? request)
    {
        try
        {
            var result = await _financeService.RefundTransactionAsync(transactionId, request?.Reason);
            return Ok(ApiResponse<RefundResultResponse>.SuccessResponse(result, "Refunded successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to refund transaction"));
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
    // UC-126: View List of Refund Requests — Chỉ Admin
    [HttpGet("refunds/pending")]
    [CustomAuthorize(requireAuth: true, "admin")]
    public async Task<IActionResult> GetPendingRefundRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var list = await _financeService.GetPendingRefundRequestsAsync(page, pageSize);
            return Ok(ApiResponse<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseMarketplaceBE.Application.DTOs.TransactionListDto>>.SuccessResponse(list, "List of pending refund requests."));
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
    // UC-127: Approve Refund Request — Chỉ Admin
    [HttpPost("refunds/{transactionId:int}/approve")]
    [CustomAuthorize(requireAuth: true, "admin")]
    public async Task<IActionResult> ApproveRefund(int transactionId, [FromBody] RefundDecisionRequest request)
    {
        try
        {
            await _financeService.ApproveRefundAsync(transactionId, request.AdminNote ?? "Approved by Admin.");
            return Ok(ApiResponse<string>.SuccessResponse("Refund approved successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to approve refund"));
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
    // UC-128: Reject Refund Request — Chỉ Admin
    [HttpPost("refunds/{transactionId:int}/reject")]
    [CustomAuthorize(requireAuth: true, "admin")]
    public async Task<IActionResult> RejectRefund(int transactionId, [FromBody] RefundDecisionRequest request)
    {
        try
        {
            await _financeService.RejectRefundAsync(transactionId, request.AdminNote ?? "Rejected by Admin.");
            return Ok(ApiResponse<string>.SuccessResponse("Refund request rejected."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to reject refund"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"System error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/admin/finance/instructor-courses-revenue
    // ═══════════════════════════════════════════════════════════════════════
    // UC-74: View Instructor's Course Revenue (For Admin View) — Chỉ Admin
    [HttpGet("instructor-courses-revenue")]
    [CustomAuthorize(requireAuth: true, "admin")]
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
