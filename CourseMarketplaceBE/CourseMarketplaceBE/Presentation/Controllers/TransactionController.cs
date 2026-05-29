using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

/// <summary>
/// API Controller cho Module Quản lý Giao dịch (UC-114, UC-115).
///
/// SOLID — SRP: Controller CHỈ làm 3 việc:
///   1. Xác thực request (JWT + [Authorize]).
///   2. Gọi ITransactionService (DIP).
///   3. Trả về HTTP response.
///
/// Không fetch Stripe API — chỉ đọc dữ liệu đã lưu trong DB.
/// </summary>
[Route("api/transactions")]
[ApiController]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IAdminFinanceService _financeService;

    public TransactionController(ITransactionService transactionService, IAdminFinanceService financeService)
    {
        _transactionService = transactionService;
        _financeService = financeService;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/transactions?page=1&pageSize=20
    // UC-115: Danh sách giao dịch có phân trang
    // ═══════════════════════════════════════════════════════════════════════
    /// <summary>
    /// UC-115: Lấy danh sách tất cả giao dịch, hỗ trợ phân trang.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] string? sortBy = "date_desc",
        [FromQuery] string? status = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        try
        {
            var result = await _transactionService.GetTransactionsAsync(page, pageSize, keyword, sortBy, status, year, month);
            return Ok(ApiResponse<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseMarketplaceBE.Application.DTOs.TransactionListDto>>.SuccessResponse(result, "Transaction list."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/transactions/{id}
    // UC-114: Chi tiết 1 giao dịch
    // ═══════════════════════════════════════════════════════════════════════
    /// <summary>
    /// UC-114: Lấy biên lai chi tiết 1 giao dịch theo ID nội bộ.
    /// Trả về phân bổ tài chính: GrossAmount → PlatformProfit + InstructorPayout.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTransactionDetail(int id)
    {
        try
        {
            var detail = await _transactionService.GetTransactionDetailAsync(id);
            if (detail == null)
                return NotFound(ApiResponse<string>.ErrorResponse($"Transaction #{id} not found."));

            return Ok(ApiResponse<TransactionDetailDto>.SuccessResponse(detail, "Transaction details."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/transactions/instructor?page=1&pageSize=20
    // Lấy danh sách giao dịch cho Giảng viên (Dùng token để xác định ID)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("instructor")]
    public async Task<IActionResult> GetInstructorTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] string? sortBy = "date_desc",
        [FromQuery] string? status = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        try
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid login session."));

            var result = await _transactionService.GetInstructorTransactionsAsync(userId, page, pageSize, keyword, sortBy, status, year, month);
            return Ok(ApiResponse<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseMarketplaceBE.Application.DTOs.TransactionListDto>>.SuccessResponse(result, "Instructor transaction list."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/transactions/my?page=1&pageSize=20
    // Lấy danh sách giao dịch cho Người mua (Dùng token để xác định ID)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("my")]
    public async Task<IActionResult> GetMyTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] string? sortBy = "date_desc",
        [FromQuery] string? status = null)
    {
        try
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid login session."));

            var result = await _transactionService.GetUserTransactionsAsync(userId, page, pageSize, keyword, sortBy, status);
            return Ok(ApiResponse<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseMarketplaceBE.Application.DTOs.TransactionListDto>>.SuccessResponse(result, "Your transaction list."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/transactions/{transactionId}/request-refund
    // Học viên gửi yêu cầu hoàn tiền
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("{transactionId:int}/request-refund")]
    public async Task<IActionResult> RequestRefund(int transactionId, [FromBody] StudentRefundRequest request)
    {
        try
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Invalid login session."));

            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest(ApiResponse<string>.ErrorResponse("Refund reason cannot be empty."));

            await _financeService.RequestRefundAsync(transactionId, userId, request.Reason);
            return Ok(ApiResponse<string>.SuccessResponse("Refund request submitted successfully. Please wait for Admin approval."));
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
