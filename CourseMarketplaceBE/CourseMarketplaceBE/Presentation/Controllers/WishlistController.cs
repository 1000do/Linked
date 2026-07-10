using CourseMarketplaceBE.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourseMarketplaceBE.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> GetWishlist()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var wishlist = await _wishlistService.GetWishlistAsync(userId.Value);
        return Ok(new { data = wishlist });
    }

    [HttpPost("{courseId}")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> AddToWishlist(int courseId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            await _wishlistService.AddToWishlistAsync(userId.Value, courseId);
            return Ok(new { message = "Added to wishlist successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = "Failed to add to wishlist" });
        }
    }

    [HttpDelete("{courseId}")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> RemoveFromWishlist(int courseId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            await _wishlistService.RemoveFromWishlistAsync(userId.Value, courseId);
            return Ok(new { message = "Removed from wishlist successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = "Failed to remove from wishlist" });
        }
    }

    [HttpPost("toggle/{courseId}")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> ToggleWishlist(int courseId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var isInWishlist = await _wishlistService.ToggleWishlistAsync(userId.Value, courseId);
            return Ok(new 
            { 
                message = isInWishlist ? "Added to wishlist successfully" : "Removed from wishlist successfully", 
                isInWishlist 
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = "Failed to toggle wishlist" });
        }
    }

    [HttpGet("check/{courseId}")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> CheckWishlist(int courseId)
    {
        var userId = GetUserId();
        if (userId == null) 
            return Ok(new { isInWishlist = false });

        var exists = await _wishlistService.IsInWishlistAsync(userId.Value, courseId);
        return Ok(new { isInWishlist = exists });
    }

    [HttpGet("count")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> GetWishlistCount()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var count = await _wishlistService.GetWishlistCountAsync(userId.Value);
        return Ok(new { count });
    }

    private int? GetUserId()
    {
        var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(str, out int id) ? id : null;
    }
}
