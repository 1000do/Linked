using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using Microsoft.AspNetCore.SignalR;
using CourseMarketplaceBE.Hubs;
using CourseMarketplaceBE.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        foreach (var item in items)
        {
            var response = await MapToWishlistResponseAsync(item, userId);
            responses.Add(response);
        }
        
        return responses;
    }

    public async Task AddToWishlistAsync(int userId, int courseId)
    {
        await EnsureCourseIsValidForWishlistAsync(courseId);

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
        await _wishlistRepository.SaveChangesAsync();
    }

    public async Task RemoveFromWishlistAsync(int userId, int courseId)
    {
        var item = await _wishlistRepository.GetByUserAndCourseAsync(userId, courseId);
        if (item == null)
        {
            throw new KeyNotFoundException("Course not found in your wishlist.");
        }

        await _wishlistRepository.RemoveAsync(item);
        await _wishlistRepository.SaveChangesAsync();
    }

    public async Task<bool> ToggleWishlistAsync(int userId, int courseId)
    {
        var item = await _wishlistRepository.GetByUserAndCourseAsync(userId, courseId);
        if (item != null)
        {
            await _wishlistRepository.RemoveAsync(item);
            await _wishlistRepository.SaveChangesAsync();
            await NotifyWishlistCountUpdatedAsync(userId);
            
            return false; // Removed
        }
        else
        {
            await EnsureCourseIsValidForWishlistAsync(courseId);

            var newItem = new WishlistItem
            {
                UserId = userId,
                CourseId = courseId,
                AddedDate = DateTime.UtcNow
            };
            
            await _wishlistRepository.AddAsync(newItem);
            await _wishlistRepository.SaveChangesAsync();
            await NotifyWishlistCountUpdatedAsync(userId);
            
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

    // ─── PRIVATE HELPERS ──────────────────────────────────────────────────────

    private async Task<WishlistResponse> MapToWishlistResponseAsync(WishlistItem item, int userId)
    {
        int courseId = item.CourseId ?? 0;
        var isEnrolled = await _courseRepository.IsEnrolledAsync(userId, courseId);
        var stats = await _courseRepository.GetCourseStatsAsync(courseId);

        return new WishlistResponse
        {
            Id = item.Id,
            CourseId = courseId,
            Title = item.Course?.Title ?? "",
            CourseThumbnailUrl = item.Course?.CourseThumbnailUrl,
            Price = item.Course?.Price ?? 0,
            InstructorName = item.Course?.Instructor?.InstructorNavigation?.FullName ?? "Unknown",
            RatingAverage = stats != null ? (decimal)stats.RatingAverage : 4.5m,
            TotalStudents = stats?.TotalStudents ?? 0,
            IsEnrolled = isEnrolled,
            AddedDate = item.AddedDate ?? DateTime.UtcNow
        };
    }

    private async Task EnsureCourseIsValidForWishlistAsync(int courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null || course.CourseStatus != CourseStatus.Published.ToValue())
        {
            throw new InvalidOperationException("Course does not exist or is not published.");
        }
    }

    private async Task NotifyWishlistCountUpdatedAsync(int userId)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdateWishlistCount");
    }
}
