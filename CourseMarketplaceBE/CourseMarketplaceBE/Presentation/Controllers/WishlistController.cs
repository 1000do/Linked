using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CourseMarketplaceBE.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly AppDbContext _context;

    public WishlistController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var wishlist = await _context.WishlistItems
            .Where(w => w.UserId == userId)
            .Include(w => w.Course)
            .ThenInclude(c => c.Instructor)
            .Select(w => new WishlistResponse
            {
                Id = w.Id,
                CourseId = w.CourseId ?? 0,
                Title = w.Course!.Title,
                CourseThumbnailUrl = w.Course.CourseThumbnailUrl,
                Price = w.Course.Price,
                InstructorName = w.Course.Instructor!.InstructorNavigation!.FullName, // Assuming instructor name is here
                AddedDate = w.AddedDate ?? DateTime.UtcNow
            })
            .ToListAsync();

        return Ok(new { data = wishlist });
    }

    [HttpPost("{courseId}")]
    public async Task<IActionResult> AddToWishlist(int courseId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var existing = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.CourseId == courseId);

        if (existing != null) return BadRequest(new { message = "Khóa học đã có trong danh sách yêu thích" });

        var newItem = new WishlistItem
        {
            UserId = userId,
            CourseId = courseId,
            AddedDate = DateTime.UtcNow
        };

        _context.WishlistItems.Add(newItem);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đã thêm vào danh sách yêu thích" });
    }

    [HttpDelete("{courseId}")]
    public async Task<IActionResult> RemoveFromWishlist(int courseId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.CourseId == courseId);

        if (item == null) return NotFound(new { message = "Không tìm thấy khóa học trong danh sách yêu thích" });

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đã xóa khỏi danh sách yêu thích" });
    }

    [HttpGet("check/{courseId}")]
    public async Task<IActionResult> CheckWishlist(int courseId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId)) 
            return Ok(new { isInWishlist = false });

        var exists = await _context.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.CourseId == courseId);

        return Ok(new { isInWishlist = exists });
    }
}
