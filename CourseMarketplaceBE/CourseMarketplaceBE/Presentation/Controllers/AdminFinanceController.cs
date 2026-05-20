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
    public async Task<IActionResult> GetPayouts([FromQuery] int? year, [FromQuery] int? month)
    {
        try
        {
            var payouts = await _financeService.GetInstructorPayoutsAsync(year, month);
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
    // POST /api/admin/finance/payouts/{payoutId}/mark-paid
    // Đánh dấu khoản thanh toán là đã thanh toán
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("payouts/{payoutId:int}/mark-paid")]
    public async Task<IActionResult> MarkPayoutAsPaid(int payoutId)
    {
        try
        {
            await _financeService.MarkPayoutAsPaidAsync(payoutId);
            return Ok(ApiResponse<string>.SuccessResponse("Đã đánh dấu khoản thanh toán này là Đã trả.", "Thành công."));
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
    // POST /api/admin/finance/payouts/{payoutId}/stripe-transfer
    // Chuyển tiền thật qua Stripe Connect
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("payouts/{payoutId:int}/stripe-transfer")]
    public async Task<IActionResult> TransferViaStripe(int payoutId)
    {
        try
        {
            var transferId = await _financeService.PerformStripeTransferAsync(payoutId);
            return Ok(ApiResponse<string>.SuccessResponse(transferId, "Chuyển tiền thành công qua Stripe."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi hệ thống: {ex.Message}"));
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
            return Ok(ApiResponse<string>.SuccessResponse("Đồng bộ trạng thái payout thành công.", "Thành công."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi hệ thống: {ex.Message}"));
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
            return Ok(ApiResponse<PlatformBalanceResponse>.SuccessResponse(balance, "Số dư Stripe Platform."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi: {ex.Message}"));
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
            return Ok(ApiResponse<WithdrawResponse>.SuccessResponse(result, "Lệnh rút tiền đã được tạo thành công."));
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

    /// <summary>GET /api/admin/finance/withdrawals — Lịch sử rút tiền</summary>
    [HttpGet("withdrawals")]
    public async Task<IActionResult> GetWithdrawalHistory([FromQuery] int? year, [FromQuery] int? month)
    {
        try
        {
            var history = await _financeService.GetWithdrawalHistoryAsync();
            if (year.HasValue && month.HasValue)
            {
                history = history.Where(w => w.CreatedAt.Year == year.Value && w.CreatedAt.Month == month.Value).ToList();
            }
            return Ok(ApiResponse<List<WithdrawalHistoryItem>>.SuccessResponse(history, "Lịch sử rút tiền."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi: {ex.Message}"));
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
            return Ok(ApiResponse<RefundResultResponse>.SuccessResponse(result, "Hoàn tiền thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi hệ thống: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/admin/finance/refunds/pending
    // Lấy danh sách các yêu cầu hoàn tiền đang chờ duyệt
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("refunds/pending")]
    public async Task<IActionResult> GetPendingRefundRequests()
    {
        try
        {
            var list = await _financeService.GetPendingRefundRequestsAsync();
            return Ok(ApiResponse<List<Domain.Entities.Transaction>>.SuccessResponse(list, "Danh sách yêu cầu hoàn tiền đang chờ duyệt."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi: {ex.Message}"));
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
            await _financeService.ApproveRefundAsync(transactionId, request.AdminNote ?? "Được chấp nhận bởi Admin.");
            return Ok(ApiResponse<string>.SuccessResponse("Đã phê duyệt hoàn tiền thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi hệ thống: {ex.Message}"));
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
            await _financeService.RejectRefundAsync(transactionId, request.AdminNote ?? "Bị từ chối bởi Admin.");
            return Ok(ApiResponse<string>.SuccessResponse("Đã từ chối yêu cầu hoàn tiền."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi hệ thống: {ex.Message}"));
        }
    }
}

public class RefundDecisionRequest
{
    public string? AdminNote { get; set; }
}
