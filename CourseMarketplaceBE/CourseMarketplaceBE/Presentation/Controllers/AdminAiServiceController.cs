using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.IServices;

namespace CourseMarketplaceBE.Presentation.Controllers
{
    [ApiController]
    [Route("api/admin/ai-service")]
    [Authorize(Roles = "admin")]
    public class AdminAiServiceController : ControllerBase
    {
        private readonly IAdminAiService _adminAiService;

        public AdminAiServiceController(IAdminAiService adminAiService)
        {
            _adminAiService = adminAiService;
        }

        // ==========================
        // MODELS
        // ==========================

        [HttpGet("models")]
        public async Task<IActionResult> GetPagedModels([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (items, totalCount) = await _adminAiService.GetPagedModelsAsync(page, pageSize);
                var pagedResult = new PagedResult<AiModelAdminDto>(items, totalCount, page, pageSize);
                return Ok(ApiResponse<object>.SuccessResponse(pagedResult, "Models retrieved successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("models/all")]
        public async Task<IActionResult> GetAllModels()
        {
            try
            {
                var models = await _adminAiService.GetAllModelsAsync();
                return Ok(ApiResponse<object>.SuccessResponse(models, "Models retrieved successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("models/{id}")]
        public async Task<IActionResult> GetModelById(int id)
        {
            try
            {
                var model = await _adminAiService.GetModelByIdAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(model, "Model retrieved successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("models")]
        public async Task<IActionResult> AddModel([FromBody] CreateAiModelRequest req)
        {
            try
            {
                var model = await _adminAiService.AddModelAsync(req);
                return Ok(ApiResponse<object>.SuccessResponse(model, "Model added successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("models/{id}")]
        public async Task<IActionResult> UpdateModel(int id, [FromBody] UpdateAiModelRequest req)
        {
            try
            {
                var model = await _adminAiService.UpdateModelAsync(id, req);
                return Ok(ApiResponse<object>.SuccessResponse(model, "Model updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPatch("models/{id}/status")]
        public async Task<IActionResult> ToggleModelStatus(int id)
        {
            try
            {
                await _adminAiService.ToggleModelStatusAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Model status toggled successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        // ==========================
        // CONFIGS
        // ==========================

        [HttpGet("configs")]
        public async Task<IActionResult> GetConfigurations()
        {
            var configs = await _adminAiService.GetConfigurationsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(configs, "Configurations retrieved successfully"));
        }

        [HttpPut("configs/thresholds")]
        public async Task<IActionResult> UpdateThresholds([FromBody] UpdateThresholdsRequest req)
        {
            try
            {
                await _adminAiService.UpdateThresholdsAsync(req);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Thresholds updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("configs/integrations")]
        public async Task<IActionResult> UpdateIntegration([FromBody] UpdateIntegrationRequest req)
        {
            try
            {
                await _adminAiService.UpdateIntegrationAsync(req);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Integration updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        // ==========================
        // LOGS
        // ==========================

        [HttpGet("logs/course")]
        public async Task<IActionResult> GetCourseModerationLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (items, totalCount) = await _adminAiService.GetCourseModerationLogsAsync(page, pageSize);
                var pagedResult = new PagedResult<CourseModerationLogAdminDto>(items, totalCount, page, pageSize);
                return Ok(ApiResponse<object>.SuccessResponse(pagedResult, "Course logs retrieved"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("logs/course/{id}")]
        public async Task<IActionResult> GetCourseModerationLogDetail(int id)
        {
            try
            {
                var log = await _adminAiService.GetCourseModerationLogDetailAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(log, "Log detail retrieved"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("logs/course-review")]
        public async Task<IActionResult> GetCourseReviewModerationLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (items, totalCount) = await _adminAiService.GetCourseReviewModerationLogsAsync(page, pageSize);
                var pagedResult = new PagedResult<ReviewModerationLogAdminDto>(items, totalCount, page, pageSize);
                return Ok(ApiResponse<object>.SuccessResponse(pagedResult, "Course review logs retrieved"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("logs/course-review/{id}")]
        public async Task<IActionResult> GetCourseReviewModerationLogDetail(int id)
        {
            try
            {
                var log = await _adminAiService.GetCourseReviewModerationLogDetailAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(log, "Log detail retrieved"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("logs/lesson-review")]
        public async Task<IActionResult> GetLessonReviewModerationLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (items, totalCount) = await _adminAiService.GetLessonReviewModerationLogsAsync(page, pageSize);
                var pagedResult = new PagedResult<ReviewModerationLogAdminDto>(items, totalCount, page, pageSize);
                return Ok(ApiResponse<object>.SuccessResponse(pagedResult, "Lesson review logs retrieved"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("logs/lesson-review/{id}")]
        public async Task<IActionResult> GetLessonReviewModerationLogDetail(int id)
        {
            try
            {
                var log = await _adminAiService.GetLessonReviewModerationLogDetailAsync(id);
                return Ok(ApiResponse<object>.SuccessResponse(log, "Log detail retrieved"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
    }
}
