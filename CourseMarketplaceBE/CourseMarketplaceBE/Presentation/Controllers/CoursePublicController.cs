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
    [AllowAnonymous]
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
    [AllowAnonymous]
    public async Task<IActionResult> GetCourseDetails(int id)
    {
        try
        {
            var userId = GetUserId();
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var course = await _courseQueryService.GetCourseWithDetailsAsync(id, userId, role);
            return Ok(ApiResponse<object>.SuccessResponse(course, "Retrieved course successfully."));
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
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
    [AllowAnonymous]
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

