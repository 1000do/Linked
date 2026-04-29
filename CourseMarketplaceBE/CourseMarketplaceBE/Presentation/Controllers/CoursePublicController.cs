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
    private readonly ICourseService _courseService;

    public CoursePublicController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    private int? GetUserId()
    {
        var str = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(str, out int id) ? id : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCourses()
    {
        try
        {
            var userId = GetUserId();
            var courses = await _courseService.GetAllPublishedCoursesAsync(userId);
            return Ok(ApiResponse<object>.SuccessResponse(courses, "Retrieved courses successfully."));
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
            // For public view, instructorId doesn't matter much if we just want to view it
            var course = await _courseService.GetCourseWithDetailsAsync(id, 0, userId); 
            if (course == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Course not found."));
            }
            return Ok(ApiResponse<object>.SuccessResponse(course, "Retrieved course successfully."));
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
            var categories = await _courseService.GetCategoriesAsync();
            return Ok(ApiResponse<object>.SuccessResponse(categories, "Retrieved categories successfully."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}
