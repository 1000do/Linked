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

    [HttpGet]
    public async Task<IActionResult> GetAllCourses()
    {
        try
        {
            var courses = await _courseService.GetAllPublishedCoursesAsync();
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
            // For public view, instructorId doesn't matter much if we just want to view it
            var course = await _courseService.GetCourseWithDetailsAsync(id, 0); 
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
