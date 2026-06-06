using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseMarketplaceBE.Presentation.Controllers;

[ApiController]
[Route("api/cache")]
public class CacheController : ControllerBase
{
    private readonly IRedisService _redisService;
    

    public CacheController(IRedisService redisService)
    {
        _redisService = redisService;
        
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetCache(string key)
    {
        try
        {
            var value = _redisService.GetCacheAsync<object>(key);
            if (value == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Value not found."));
            }
            return Ok(ApiResponse<object>.SuccessResponse(value, "Retrieved value successfully."));
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


    

    [HttpDelete("{key}")]
    public async Task<IActionResult> InvalidateCache(string key)
    {
        try
        {
            
            await _redisService.RemoveCacheAsync(key);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Cache invalidated successfully."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
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

   
}

