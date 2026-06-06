using CourseMarketplaceBE.Domain.Constants;
using System;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

[ApiController]
[Route("api/public/courses")]
[AllowAnonymous]
public class CoursePublicController : ControllerBase
{
    private readonly ICourseQueryService _courseQueryService;

    public CoursePublicController(ICourseQueryService courseQueryService)
    {
        _courseQueryService = courseQueryService;
    }

    private int? GetUserId()
    {
        var str = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(str, out int id) ? id : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCourses(
        [FromQuery] string? query = null,
        [FromQuery] string? category = null,
        [FromQuery] string? sort = null,
        [FromQuery] string? price = null,
        [FromQuery] string? rating = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
    {
        try
        {
            var userId = GetUserId();
            var result = await _courseQueryService.GetPublishedCoursesPagedAsync(query, category, sort, price, rating, page, pageSize, userId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Retrieved courses successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourseDetails(int id)
    {
        try
        {
            var userId = GetUserId();
            var course = await _courseQueryService.GetCourseWithDetailsAsync(id, 0, userId); 
            if (course == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Course not found."));
            }

            if (!string.Equals(course.CourseStatus, CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), StringComparison.OrdinalIgnoreCase))
            {
                bool isOwner = userId.HasValue && course.InstructorId == userId.Value;
                bool isEnrolled = course.IsEnrolled;
                bool isStaffOrAdmin = false;

                if (userId.HasValue)
                {
                    var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                    if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase) || 
                        string.Equals(role, "staff", StringComparison.OrdinalIgnoreCase))
                    {
                        isStaffOrAdmin = true;
                    }
                }

                if (!isOwner && !isEnrolled && !isStaffOrAdmin)
                {
                    return StatusCode(403, ApiResponse<object>.ErrorResponse("You do not have permission to view this course."));
                }
            }

            return Ok(ApiResponse<object>.SuccessResponse(course, "Retrieved course successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _courseQueryService.GetCategoriesAsync();
            return Ok(ApiResponse<object>.SuccessResponse(categories, "Retrieved categories successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}

