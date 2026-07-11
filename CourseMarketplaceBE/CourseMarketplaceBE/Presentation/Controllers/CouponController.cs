using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CourseMarketplaceBE.Presentation.Filters;

namespace CourseMarketplaceBE.Presentation.Controllers;

/// <summary>
/// API Controller cho Module Quản lý Khuyến Mãi (UC-62, 63, 64, 65).
///
/// SOLID — SRP: Controller CHỈ làm 3 việc:
///   1. Xác thực request (JWT + [Authorize]).
///   2. Gọi ICouponService (DIP).
///   3. Trả về HTTP response chuẩn ApiResponse.
///
/// ★ 2 nhóm endpoint:
///   1. /api/coupon/*           — Admin CRUD (Roles = "admin")
///   2. /api/coupon/platform/*  — Instructor browse & attach (Roles = "instructor")
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CouponController : ControllerBase
{
    private readonly ICouponService _service;

    public CouponController(ICouponService service) => _service = service;

    private int GetUserId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // ═══════════════════════════════════════════════════════════════════════
    //  ADMIN: CRUD Kho Voucher
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>UC-63: Danh sách tất cả mã giảm giá (Admin).</summary>
    [HttpGet]
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: true, "admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isActive,
        [FromQuery] string? type,
        [FromQuery] string? search)
    {
        try
        {
            var isAdmin = User.IsInRole("admin");
            var data = await _service.GetAll(GetUserId(), isActive, type, search, isAdmin);
            return Ok(ApiResponse<List<CouponResponse>>.SuccessResponse(data, "Voucher list."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    /// <summary>UC-63: Chi tiết 1 mã giảm giá (Admin).</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: true, "admin")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var isAdmin = User.IsInRole("admin");
            var data = await _service.GetById(id, GetUserId(), isAdmin);
            if (data == null)
                return NotFound(ApiResponse<string>.ErrorResponse($"Coupon #{id} not found."));
            return Ok(ApiResponse<CouponResponse>.SuccessResponse(data, "Coupon details."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    /// <summary>UC-62: Tạo mã giảm giá mới (Admin).</summary>
    [HttpPost]
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: true, "admin")]
    public async Task<IActionResult> Create([FromBody] CreateCouponRequest req)
    {
        try
        {
            await _service.Create(req, GetUserId());
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Coupon created successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to create coupon"));
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

    /// <summary>
    /// UC-64: Sửa mã giảm giá (Admin).
    /// ★ CHỈ cho phép sửa: end_date, usage_limit, is_active.
    /// </summary>
    [HttpPut("{id:int}")]
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: true, "admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCouponRequest req)
    {
        try
        {
            var isAdmin = User.IsInRole("admin");
            await _service.Update(id, req, GetUserId(), isAdmin);
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Coupon updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to update coupon"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    /// <summary>
    /// UC-65: Xóa mềm mã giảm giá (Admin).
    /// ★ Luôn soft-delete: set is_active = false, end_date = hôm qua.
    /// </summary>
    [HttpDelete("{id:int}")]
    [AllowAnonymous]
    [CustomAuthorize(requireAuth: true, "admin")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        try
        {
            var isAdmin = User.IsInRole("admin");
            await _service.SoftDelete(id, GetUserId(), isAdmin);
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Coupon disabled successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to disable coupon"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  INSTRUCTOR: Browse & Attach Coupon → Course
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Lấy danh sách mã giảm giá đang hoạt động trên nền tảng.
    /// Instructor dùng để chọn gắn vào khóa học.
    /// </summary>
    [HttpGet("platform/active")]
    [Authorize]
    public async Task<IActionResult> GetActivePlatformCoupons()
    {
        try
        {
            var data = await _service.GetActivePlatformCouponsAsync();
            return Ok(ApiResponse<List<CouponResponse>>.SuccessResponse(data, "Available coupon list."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Giảng viên gắn mã giảm giá vào khóa học.
    /// ★ JWT ownership: Chỉ chủ khóa học mới được phép.
    /// </summary>
    [HttpPost("platform/apply")]
    [Authorize]
    public async Task<IActionResult> ApplyCouponToCourse([FromBody] ApplyCouponToCourseRequest req)
    {
        try
        {
            await _service.ApplyCouponToCourseAsync(req.CourseId, req.CouponId, GetUserId());
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Coupon applied to course successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to apply coupon to course"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Giảng viên gỡ mã giảm giá khỏi khóa học.
    /// ★ JWT ownership: Chỉ chủ khóa học mới được phép.
    /// </summary>
    [HttpDelete("platform/remove/{courseId:int}")]
    [Authorize]
    public async Task<IActionResult> RemoveCouponFromCourse(int courseId)
    {
        try
        {
            await _service.RemoveCouponFromCourseAsync(courseId, GetUserId());
            return Ok(ApiResponse<string>.SuccessResponse("OK", "Coupon removed from course successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse($"Failed to remove coupon from course"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse($"Error: {ex.Message}"));
        }
    }
}
