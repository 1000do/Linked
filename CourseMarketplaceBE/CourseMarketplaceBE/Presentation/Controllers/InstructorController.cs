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
        if (userId == null) return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });

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
            return StatusCode(500, new { status = 500, message = $"Lỗi server: {ex.Message}" });
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
        if (userId == null) return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });

        var info = await _instructorService.GetRejectedApplicationInfoAsync(userId.Value);
        if (info == null) return NotFound(new { status = 404, message = "Không tìm thấy đơn bị từ chối." });

        return Ok(new { status = 200, data = info });
    }

    // ─── 2. ADMIN DUYỆT ĐƠN ─────────────────────────────────────────
    /// <summary>
    /// PUT /api/instructor/{id}/approve?status=Approved
    /// Admin duyệt hoặc từ chối đơn. Yêu cầu Role = manager.
    /// </summary>
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> Approve(int id, [FromQuery] string status)
    {
        try
        {
            await _instructorService.ApproveApplicationAsync(id, status);
            return Ok(new { status = 200, message = $"Đã cập nhật trạng thái thành '{status}'." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { status = 400, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Lỗi server: {ex.Message}" });
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
        if (userId == null) return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });

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
            return StatusCode(502, new { status = 502, message = "Lỗi Stripe.", detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Lỗi server: {ex.Message}" });
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
                    message = "Stripe Onboarding hoàn tất! Bạn đã có thể nhận thanh toán.",
                    stripeStatus = "Active"
                });
            }
            else if (stripeStatus == "Processing")
            {
                return Ok(new
                {
                    status = 200,
                    message = "Đã lưu hồ sơ Stripe! Vui lòng chờ Stripe xác minh để kích hoạt tính năng thanh toán.",
                    stripeStatus = "Processing"
                });
            }

            return Ok(new
            {
                status = 200,
                message = "Bạn chưa hoàn tất thiết lập Stripe.",
                stripeStatus = "Pending"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { status = 400, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = 500, message = $"Lỗi server: {ex.Message}" });
        }
    }

    // ─── 5. ADMIN XEM DANH SÁCH ĐƠN ─────────────────────────────────
    [HttpGet("applications")]
    [Authorize(Roles = "manager")]
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
        if (userId == null) return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });

        var data = await _instructorService.GetInstructorDashboardAsync(userId.Value);
        if (data == null) return NotFound(new { status = 404, message = "Chưa đăng ký Giảng viên." });

        return Ok(new { status = 200, data });
    }

    // ─── HELPER ──────────────────────────────────────────────────────
    private int? GetUserId()
    {
        var str = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(str, out int id) ? id : null;
    }
}
