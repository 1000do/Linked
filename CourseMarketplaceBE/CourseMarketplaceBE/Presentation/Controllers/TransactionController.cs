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

    public TransactionController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
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
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _transactionService.GetTransactionsAsync(page, pageSize);
            return Ok(ApiResponse<TransactionPagedResult>.SuccessResponse(result, "Danh sách giao dịch."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi: {ex.Message}"));
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
                return NotFound(ApiResponse<string>.ErrorResponse($"Không tìm thấy giao dịch #{id}."));

            return Ok(ApiResponse<TransactionDetailDto>.SuccessResponse(detail, "Chi tiết giao dịch."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/transactions/instructor?page=1&pageSize=20
    // Lấy danh sách giao dịch cho Giảng viên (Dùng token để xác định ID)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("instructor")]
    public async Task<IActionResult> GetInstructorTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Phiên đăng nhập không hợp lệ."));

            var result = await _transactionService.GetInstructorTransactionsAsync(userId, page, pageSize);
            return Ok(ApiResponse<TransactionPagedResult>.SuccessResponse(result, "Danh sách giao dịch của giảng viên."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Lỗi: {ex.Message}"));
        }
    }
}
