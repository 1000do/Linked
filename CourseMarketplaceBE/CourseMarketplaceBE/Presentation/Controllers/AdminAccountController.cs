using System;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

[Route("api/admin/accounts")]
[ApiController]
[Authorize(Roles = "admin")]
public class AdminAccountController : ControllerBase
{
    private readonly IAdminAccountService _accountService;

    public AdminAccountController(IAdminAccountService accountService)
    {
        _accountService = accountService;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/admin/accounts
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> GetAccounts(
        [FromQuery] string? keyword,
        [FromQuery] string? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _accountService.GetAccountsPagedAsync(keyword, role, page, pageSize);
            return Ok(new
            {
                status = 200,
                message = "Accounts retrieved successfully.",
                data = result.Items,
                totalCount = result.TotalCount,
                page = result.Page,
                pageSize = result.PageSize
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/admin/accounts/{id}
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountDetail(int id)
    {
        try
        {
            var detail = await _accountService.GetAccountDetailAsync(id);
            if (detail == null)
            {
                return NotFound(new { status = 404, message = "Account not found." });
            }
            return Ok(new { status = 200, message = "Account details retrieved successfully.", data = detail });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/admin/accounts/{id}/transactions
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("{id}/transactions")]
    public async Task<IActionResult> GetAccountTransactions(int id)
    {
        try
        {
            var summary = await _accountService.GetAccountTransactionsAsync(id);
            if (summary == null)
            {
                return NotFound(new { status = 404, message = "Account not found." });
            }
            return Ok(new { status = 200, message = "Account transactions retrieved successfully.", data = summary });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/admin/accounts/staff (Tạo Staff)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("staff")]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return BadRequest(new { status = 400, message = "Email, password, and display name are required." });
            }

            if (await _accountService.IsEmailExistsAsync(request.Email))
            {
                return BadRequest(new { status = 400, message = "Email is already registered." });
            }

            await _accountService.CreateStaffAsync(request);
            return Ok(new { status = 200, message = "Staff account created successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { status = 400, message = "Failed to create staff account" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PUT /api/admin/accounts/staff/{id} (Cập nhật Staff)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPut("staff/{id}")]
    public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(new { status = 400, message = errors });
            }

            if (string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return BadRequest(new { status = 400, message = "Display name is required." });
            }

            var success = await _accountService.UpdateStaffAsync(id, request);
            if (!success)
            {
                return NotFound(new { status = 404, message = "Staff account not found." });
            }

            return Ok(new { status = 200, message = "Staff account updated successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { status = 400, message = "Failed to update staff account" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/admin/accounts/{id}/toggle-ban (Khoá/mở khoá)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("{id}/toggle-ban")]
    public async Task<IActionResult> ToggleBan(int id)
    {
        try
        {
            var adminIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(adminIdStr, out int adminId);

            var newStatus = await _accountService.ToggleBanAsync(id, adminId);
            if (newStatus == null)
            {
                return NotFound(new { status = 404, message = "Account not found." });
            }

            return Ok(new { status = 200, message = $"Account status changed to {newStatus} successfully.", currentStatus = newStatus });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { status = 400, message = "Failed to toggle ban" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/admin/accounts/{id}/flag (Tăng flag count)
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("{id}/flag")]
    public async Task<IActionResult> FlagAccount(int id, [FromBody] FlagAccountRequest request)
    {
        try
        {
            var result = await _accountService.FlagAccountAsync(id, request.Reason);
            if (!result.Success)
            {
                if (result.ErrorMessage == "Account not found.")
                {
                    return NotFound(new { status = 404, message = result.ErrorMessage });
                }
                return BadRequest(new { status = 400, message = result.ErrorMessage });
            }

            return Ok(new { status = 200, message = "Account flagged successfully.", currentFlags = result.CurrentFlags, newStatus = result.NewStatus });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { status = 400, message = "Failed to flag account" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Internal server error: {ex.Message}" });
        }
    }
}
