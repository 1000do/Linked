using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourseMarketplaceBE.Presentation.Controllers;

/// <summary>
/// API Controller xử lý luồng Giảng viên:
/// POST /api/instructor/apply         → Learner nộp đơn
/// PUT  /api/instructor/{id}/approve   → Admin duyệt đơn
/// POST /api/instructor/setup-payout   → Instructor setup Stripe
/// GET  /api/instructor/stripe-return  → Stripe redirect về → verify
/// GET  /api/instructor/applications   → Admin xem danh sách đơn
/// GET  /api/instructor/dashboard      → Instructor xem dashboard
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class InstructorController : ControllerBase
{
    private readonly IInstructorService _instructorService;

    public InstructorController(IInstructorService instructorService)
    {
        _instructorService = instructorService;
    }

    // ─── 1. NỘP ĐƠN ĐĂNG KÝ ────────────────────────────────────────
    /// <summary>
    /// POST /api/instructor/apply
    /// Learner gửi form đăng ký Giảng viên (có upload file CV/ID).
    /// Nếu bị Rejected trước đó sẽ UPDATE lại record cũ (không INSERT mới).
    /// </summary>
    [HttpPost("apply")]
    [Authorize]
    public async Task<IActionResult> Apply([FromForm] InstructorApplicationRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid login session." });

        try
        {
            var result = await _instructorService.SubmitApplicationAsync(userId.Value, request);
            return Ok(new { status = 200, message = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { status = 400, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Server error: {ex.Message}" });
        }
    }

    // ─── 1b. LẤY THÔNG TIN ĐƠN CŨ (Để ĐIỀN SẴN KHI BỊ REJECTED) ─────────
    /// <summary>
    /// GET /api/instructor/apply-info
    /// Trả về thông tin đơn cũ khi user bị Rejected, dùng để FE điền sẵn vào form.
    /// </summary>
    [HttpGet("apply-info")]
    [Authorize]
    public async Task<IActionResult> GetApplyInfo()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid login session." });

        var info = await _instructorService.GetRejectedApplicationInfoAsync(userId.Value);
        if (info == null) return NotFound(new { status = 404, message = "Rejected application not found." });

        return Ok(new { status = 200, data = info });
    }

    // ─── 2. ADMIN DUYỆT ĐƠN ─────────────────────────────────────────
    /// <summary>
    /// PUT /api/instructor/{id}/approve?status=Approved
    /// Admin duyệt hoặc từ chối đơn. Yêu cầu Role = manager.
    /// </summary>
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> Approve(int id, [FromQuery] string status)
    {
        try
        {
            await _instructorService.ApproveApplicationAsync(id, status);
            return Ok(new { status = 200, message = $"Status updated to '{status}'." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { status = 400, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Server error: {ex.Message}" });
        }
    }

    // ─── 3. SETUP STRIPE PAYOUT ──────────────────────────────────────
    /// <summary>
    /// POST /api/instructor/setup-payout
    /// Instructor đã Approved → tạo Stripe Express account + trả URL Onboarding.
    /// </summary>
    [HttpPost("setup-payout")]
    [Authorize]
    public async Task<IActionResult> SetupPayout()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid login session." });

        try
        {
            var result = await _instructorService.SetupStripePayoutAsync(userId.Value);
            return Ok(new
            {
                status = 200,
                url = result.OnboardingUrl,
                stripeAccountId = result.StripeAccountId
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { status = 400, message = ex.Message });
        }
        catch (Stripe.StripeException ex)
        {
            return StatusCode(502, new { status = 502, message = "Stripe error.", detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Server error: {ex.Message}" });
        }
    }

    // ─── 4. STRIPE RETURN → VERIFY ───────────────────────────────────
    /// <summary>
    /// GET /api/instructor/stripe-return?instructorId=123
    /// Stripe redirect User về đây sau khi hoàn tất (hoặc bỏ dở) onboarding.
    /// Verify PayoutsEnabled & ChargesEnabled → redirect về FE Dashboard.
    /// </summary>
    [HttpGet("stripe-return")]
    [Authorize]
    public async Task<IActionResult> StripeReturn([FromQuery] int instructorId)
    {
        try
        {
            var stripeStatus = await _instructorService.VerifyStripeOnboardingAsync(instructorId);

            if (stripeStatus == "Active")
            {
                return Ok(new
                {
                    status = 200,
                    message = "Stripe Onboarding completed! You can now receive payments.",
                    stripeStatus = "Active"
                });
            }
            else if (stripeStatus == "Processing")
            {
                return Ok(new
                {
                    status = 200,
                    message = "Stripe account details saved! Please wait for Stripe verification to activate payments.",
                    stripeStatus = "Processing"
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Stripe setup is not completed.",
                stripeStatus = "Pending"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { status = 400, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Server error: {ex.Message}" });
        }
    }

    // ─── 5. ADMIN XEM DANH SÁCH ĐƠN ─────────────────────────────────
    [HttpGet("applications")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> GetApplications()
    {
        var list = await _instructorService.GetAllApplicationsAsync();
        return Ok(new { status = 200, data = list });
    }

    // ─── 6. INSTRUCTOR DASHBOARD ─────────────────────────────────────
    [HttpGet("dashboard")]
    [Authorize]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid login session." });

        var data = await _instructorService.GetInstructorDashboardAsync(userId.Value);
        if (data == null) return NotFound(new { status = 404, message = "Not registered as an instructor yet." });

        return Ok(new { status = 200, data });
    }

    // ─── 7. LẤY DANH SÁCH QUỐC GIA STRIPE HỖ TRỢ ───────────────────
    /// <summary>
    /// GET /api/instructor/stripe-countries
    /// Trả về danh sách quốc gia Stripe Connect hỗ trợ (từ system_configs.StripeCountries).
    /// </summary>
    [HttpGet("stripe-countries")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStripeCountries(
        [FromServices] CourseMarketplaceBE.Domain.IRepositories.IAdminFinanceRepository financeRepo)
    {
        try
        {
            var json = await financeRepo.GetConfigValueAsync("StripeCountries");
            if (string.IsNullOrEmpty(json))
                return Ok(new { status = 200, data = new List<object>() });

            var countries = System.Text.Json.JsonSerializer.Deserialize<List<object>>(json);
            return Ok(new { status = 200, data = countries });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = ex.Message });
        }
    }

    // ─── 8. GIẢNG VIÊN CHỌN QUỐC GIA STRIPE ─────────────────────────
    /// <summary>
    /// PUT /api/instructor/stripe-country
    /// Giảng viên chọn quốc gia cho tài khoản Stripe Connect.
    /// Phải gọi TRƯỚC khi setup-payout.
    /// </summary>
    [HttpPut("stripe-country")]
    [Authorize]
    public async Task<IActionResult> SetStripeCountry([FromBody] SetCountryRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid login session." });

        try
        {
            await _instructorService.SetStripeCountryAsync(userId.Value, request.CountryCode);
            return Ok(new { status = 200, message = $"Saved Stripe country: {request.CountryCode}" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { status = 400, message = ex.Message });
        }
    }

    // ─── 9. LẤY LỊCH SỬ THANH TOÁN ───────────────────────────────────
    [HttpGet("payouts")]
    [Authorize]
    public async Task<IActionResult> GetPayouts(int page = 1, int pageSize = 10, string? keyword = null, string? sortBy = "date_desc", string? status = null, [FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid login session." });

        try
        {
            var pagedResult = await _instructorService.GetPayoutsAsync(userId.Value, page, pageSize, keyword, sortBy, status, year, month);
            return Ok(new { status = 200, data = pagedResult });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Server error: {ex.Message}" });
        }
    }

    // ─── 9b. ĐỒNG BỘ PAYOUT TỪ STRIPE (PULL MODEL) ──────────────────
    /// <summary>
    /// POST /api/instructor/sync-payouts
    /// Chủ động gọi Stripe để lấy danh sách Payouts và cập nhật vào DB local.
    /// Dùng khi bị lỡ Webhook hoặc muốn làm mới dữ liệu ngay lập tức.
    /// </summary>
    [HttpPost("sync-payouts")]
    [Authorize]
    public async Task<IActionResult> SyncPayouts()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid login session." });

        try
        {
            await _instructorService.SyncPayoutsWithStripeAsync(userId.Value);
            var pagedResult = await _instructorService.GetPayoutsAsync(userId.Value, 1, 10, null, "date_desc", null);
            return Ok(new { status = 200, message = "Synchronized successfully!", data = pagedResult });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Sync error: {ex.Message}" });
        }
    }

    // ─── 10. KIỂM TRA STRIPE STATUS ────────────────────────────────
    /// <summary>
    /// GET /api/instructor/stripe-status
    /// Trả về trạng thái Stripe của giảng viên hiện tại.
    /// FE dùng để quyết định cho phép đặt giá hay chỉ tạo khóa Free.
    /// </summary>
    [HttpGet("stripe-status")]
    [Authorize]
    public async Task<IActionResult> GetStripeStatus()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { message = "Invalid login session." });

        var data = await _instructorService.GetInstructorDashboardAsync(userId.Value);
        if (data == null) return NotFound(new { status = 404, message = "Not registered as an instructor yet." });

        var isStripeActive = !string.IsNullOrEmpty(data.StripeAccountId)
            && string.Equals(data.StripeOnboardingStatus, "Active", StringComparison.OrdinalIgnoreCase);

        return Ok(new
        {
            status = 200,
            isStripeActive,
            stripeOnboardingStatus = data.StripeOnboardingStatus,
            stripeAccountId = data.StripeAccountId
        });
    }

    // ─── 11. PUBLIC PROFILE ──────────────────────────────────────────
    /// <summary>
    /// GET /api/instructor/profile/{id}
    /// Trả về thông tin công khai của giảng viên (không yêu cầu login).
    /// </summary>
    [HttpGet("profile/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicProfile(int id)
    {
        try
        {
            var profile = await _instructorService.GetPublicProfileAsync(id);
            if (profile == null)
                return NotFound(new { status = 404, message = "Instructor profile not found or application not approved yet." });

            return Ok(new { status = 200, data = profile });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Server error: {ex.Message}" });
        }
    }

    // ─── HELPER ──────────────────────────────────────────────────────
    private int? GetUserId()
    {
        var str = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(str, out int id) ? id : null;
    }
}

/// <summary>
/// Request body for PUT /api/instructor/stripe-country
/// </summary>
public class SetCountryRequest
{
    public string CountryCode { get; set; } = null!;
}
