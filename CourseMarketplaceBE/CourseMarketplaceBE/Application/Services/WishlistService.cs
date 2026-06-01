using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.AspNetCore.SignalR;
using CourseMarketplaceBE.Hubs;

namespace CourseMarketplaceBE.Application.Services;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ICourseRepository _courseRepository;

    public WishlistService(IWishlistRepository wishlistRepository, IHubContext<NotificationHub> hubContext, ICourseRepository courseRepository)
    {
        _wishlistRepository = wishlistRepository;
        _hubContext = hubContext;
        _courseRepository = courseRepository;
    }

    public async Task<List<WishlistResponse>> GetWishlistAsync(int userId)
    {
        var items = await _wishlistRepository.GetByUserIdAsync(userId);
        
        var responses = new List<WishlistResponse>();
        foreach (var w in items)
        {
            int courseId = w.CourseId ?? 0;
            var isEnrolled = await _courseRepository.IsEnrolledAsync(userId, courseId);
            var stats = await _courseRepository.GetCourseStatsAsync(courseId);

            responses.Add(new WishlistResponse
            {
                Id = w.Id,
                CourseId = courseId,
                Title = w.Course?.Title ?? "",
                CourseThumbnailUrl = w.Course?.CourseThumbnailUrl,
                Price = w.Course?.Price ?? 0,
                InstructorName = w.Course?.Instructor?.InstructorNavigation?.FullName ?? "Unknown",
                RatingAverage = stats != null ? (decimal)stats.RatingAverage : 4.5m,
                TotalStudents = stats?.TotalStudents ?? 0,
                IsEnrolled = isEnrolled,
                AddedDate = w.AddedDate ?? DateTime.UtcNow
            });
        }
        return responses;
    }

    public async Task AddToWishlistAsync(int userId, int courseId)
    {
        if (await _wishlistRepository.ExistsAsync(userId, courseId))
        {
            throw new InvalidOperationException("Course is already in your wishlist.");
        }

        var newItem = new WishlistItem
        {
            UserId = userId,
            CourseId = courseId,
            AddedDate = DateTime.UtcNow
        };

        await _wishlistRepository.AddAsync(newItem);
        int numberOfRowsAffected = await _wishlistRepository.SaveChangesAsync();
        if (numberOfRowsAffected <= 0)
            throw new InvalidOperationException("Failed to save changes");
    }

    public async Task RemoveFromWishlistAsync(int userId, int courseId)
    {
        var item = await _wishlistRepository.GetByUserAndCourseAsync(userId, courseId);
        if (item == null)
        {
            throw new KeyNotFoundException("Course not found in your wishlist.");
        }

        await _wishlistRepository.RemoveAsync(item);
        int numberOfRowsAffected = await _wishlistRepository.SaveChangesAsync();
        if (numberOfRowsAffected <= 0)
            throw new InvalidOperationException("Failed to save changes");
    }

    public async Task<bool> ToggleWishlistAsync(int userId, int courseId)
    {
        var item = await _wishlistRepository.GetByUserAndCourseAsync(userId, courseId);
        if (item != null)
        {
            await _wishlistRepository.RemoveAsync(item);
            int numberOfRowsAffected = await _wishlistRepository.SaveChangesAsync();
            if (numberOfRowsAffected <= 0)
                throw new InvalidOperationException("Failed to save changes");
            
            // Notify via SignalR
            await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdateWishlistCount");
            
            return false; // Removed
        }
        else
        {
            var newItem = new WishlistItem
            {
                UserId = userId,
                CourseId = courseId,
                AddedDate = DateTime.UtcNow
            };
            await _wishlistRepository.AddAsync(newItem);
            int numberOfRowsAffected = await _wishlistRepository.SaveChangesAsync();
            if (numberOfRowsAffected <= 0)
                throw new InvalidOperationException("Failed to save changes");
            
            // Notify via SignalR
            await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdateWishlistCount");
            
            return true; // Added
        }
    }

    public async Task<bool> IsInWishlistAsync(int userId, int courseId)
    {
        return await _wishlistRepository.ExistsAsync(userId, courseId);
    }

    public async Task<int> GetWishlistCountAsync(int userId)
    {
        var items = await _wishlistRepository.GetByUserIdAsync(userId);
        return items.Count;
    }
}
