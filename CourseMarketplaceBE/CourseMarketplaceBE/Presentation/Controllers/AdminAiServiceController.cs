using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;

namespace CourseMarketplaceBE.Presentation.Controllers
{
    [ApiController]
    [Route("api/admin/ai-service")]
    [Authorize(Roles = "admin")]
    public class AdminAiServiceController : ControllerBase
    {
        private readonly IAiModelManagementService _aiModelService;
        private readonly IAiConfigurationService _aiConfigService;
        private readonly IAiModerationLogService _aiLogService;

        public AdminAiServiceController(
            IAiModelManagementService aiModelService,
            IAiConfigurationService aiConfigService,
            IAiModerationLogService aiLogService)
        {
            _aiModelService = aiModelService;
            _aiConfigService = aiConfigService;
            _aiLogService = aiLogService;
        }

        // ==========================
        // MODELS
        // ==========================

        [HttpGet("models")]
        public async Task<IActionResult> GetPagedModels([FromQuery] PagedRequestDto request)
        {
            try
            {
                var pagedResult = await _aiModelService.GetPagedModelsAsync(request);
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
                var models = await _aiModelService.GetAllModelsAsync();
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
                var model = await _aiModelService.GetModelByIdAsync(id);
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
                var model = await _aiModelService.AddModelAsync(req);
                return Ok(ApiResponse<object>.SuccessResponse(model, "Model added successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (BadRequestException ex)
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
                var model = await _aiModelService.UpdateModelAsync(id, req);
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
            catch (BadRequestException ex)
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
                await _aiModelService.ToggleModelStatusAsync(id);
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
            catch (BadRequestException ex)
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
            var configs = await _aiConfigService.GetConfigurationsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(configs, "Configurations retrieved successfully"));
        }

        [HttpPut("configs/thresholds")]
        public async Task<IActionResult> UpdateThresholds([FromBody] UpdateThresholdsRequest req)
        {
            try
            {
                await _aiConfigService.UpdateThresholdsAsync(req);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Thresholds updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (BadRequestException ex)
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
                await _aiConfigService.UpdateIntegrationAsync(req);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Integration updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (BadRequestException ex)
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
        public async Task<IActionResult> GetCourseModerationLogs([FromQuery] PagedRequestDto request)
        {
            try
            {
                var pagedResult = await _aiLogService.GetCourseModerationLogsAsync(request);
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
                var log = await _aiLogService.GetCourseModerationLogDetailAsync(id);
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
        public async Task<IActionResult> GetCourseReviewModerationLogs([FromQuery] PagedRequestDto request)
        {
            try
            {
                var pagedResult = await _aiLogService.GetCourseReviewModerationLogsAsync(request);
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
                var log = await _aiLogService.GetCourseReviewModerationLogDetailAsync(id);
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
        public async Task<IActionResult> GetLessonReviewModerationLogs([FromQuery] PagedRequestDto request)
        {
            try
            {
                var pagedResult = await _aiLogService.GetLessonReviewModerationLogsAsync(request);
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
                var log = await _aiLogService.GetLessonReviewModerationLogDetailAsync(id);
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
