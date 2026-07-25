using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Infrastructure.Data;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class ReviewModerationRecordRepository : IReviewModerationRecordRepository
{
    private readonly AppDbContext _context;

    public ReviewModerationRecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddCourseReviewModerationRecordAsync(CourseReviewModerationRecord record)
    {
        await _context.CourseReviewModerationRecords.AddAsync(record);
    }

    public async Task AddLessonReviewModerationRecordAsync(LessonReviewModerationRecord record)
    {
        await _context.LessonReviewModerationRecords.AddAsync(record);
    }

    public async Task UpdateCourseReviewModerationRecordAsync(CourseReviewModerationRecord record)
    {
        _context.CourseReviewModerationRecords.Update(record);
        await Task.CompletedTask;
    }

    public async Task UpdateLessonReviewModerationRecordAsync(LessonReviewModerationRecord record)
    {
        _context.LessonReviewModerationRecords.Update(record);
        await Task.CompletedTask;
    }

    public async Task<CourseReviewModerationRecord?> GetCourseReviewModerationRecordByIdAsync(int recordId)
    {
        return await _context.CourseReviewModerationRecords
            .Include(r => r.CourseReview)
            .FirstOrDefaultAsync(r => r.RecordId == recordId);
    }

    public async Task<LessonReviewModerationRecord?> GetLessonReviewModerationRecordByIdAsync(int recordId)
    {
        return await _context.LessonReviewModerationRecords
            .Include(r => r.LessonReview)
            .FirstOrDefaultAsync(r => r.RecordId == recordId);
    }

    public async Task<int> CountByModerationStatusAsync(string moderationStatus)
    {
        var targetStatus = moderationStatus.ToLower();
        var courseCount = await _context.CourseReviewModerationRecords.CountAsync(r => r.ModerationStatus == targetStatus);
        var lessonCount = await _context.LessonReviewModerationRecords.CountAsync(r => r.ModerationStatus == targetStatus);
        return courseCount + lessonCount;
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new ReviewModerationException("Database operation failed due to a constraint violation or data issue while saving Review Moderation Record.", ex);
        }
    }

    public async Task<(System.Collections.Generic.List<CourseReviewModerationRecord> Items, int TotalCount)> GetCourseReviewModerationRecordsAsync(PagedReviewModerationRequest request)
    {
        var query = _context.CourseReviewModerationRecords
            .Include(r => r.CourseReview)
                .ThenInclude(cr => cr.Enrollment)
                    .ThenInclude(e => e.User)
                        .ThenInclude(u => u.UserNavigation)
            .Include(r => r.CourseReview)
                .ThenInclude(cr => cr.Enrollment)
                    .ThenInclude(e => e.Course)
            .AsNoTracking()
            .AsQueryable();

        string pendingStatus = ModerationStatus.Pending.ToValue().ToLower();
        string approvedStatus = ModerationStatus.Approved.ToValue().ToLower();
        string rejectedStatus = ModerationStatus.Rejected.ToValue().ToLower();
        string flaggedStatus = ModerationStatus.Flagged.ToValue().ToLower();
        string manualAuditStatus = ModerationStatus.ManualAudit.ToValue().ToLower();

        // Filters
        if (!string.IsNullOrEmpty(request.ModerationStatus) && request.ModerationStatus.ToLower() != "all")
        {
            query = query.Where(r => r.ModerationStatus == request.ModerationStatus.ToLower());
        }

        if (!string.IsNullOrEmpty(request.RequestType) && request.RequestType.ToLower() != "both")
        {
            bool isUpdate = request.RequestType.ToLower() == "edit";
            query = query.Where(r => r.IsUpdate == isUpdate);
        }

        if (!string.IsNullOrEmpty(request.AiModerationStatus) && request.AiModerationStatus.ToLower() != "all")
        {
            query = query.Where(r => r.AiModerationStatus == request.AiModerationStatus.ToLower());
        }

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search;
            query = query.Where(r => 
                EF.Functions.ILike(r.TempComment, $"%{s}%") ||
                (r.CourseReview.Comment != null && EF.Functions.ILike(r.CourseReview.Comment, $"%{s}%")) ||
                (r.CourseReview.Enrollment.User.UserNavigation.Username != null && EF.Functions.ILike(r.CourseReview.Enrollment.User.UserNavigation.Username, $"%{s}%")) ||
                EF.Functions.ILike(r.CourseReview.Enrollment.User.FullName, $"%{s}%") ||
                (r.CourseReview.Enrollment.User.UserNavigation.Email != null && EF.Functions.ILike(r.CourseReview.Enrollment.User.UserNavigation.Email, $"%{s}%")) ||
                EF.Functions.ILike(r.CourseReview.Enrollment.Course.Title, $"%{s}%")
            );
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "updated_at_desc" => query.OrderByDescending(r => r.UpdatedAt),
            "updated_at_asc" => query.OrderBy(r => r.UpdatedAt),
            "temp_rating_desc" => query.OrderByDescending(r => r.TempRating),
            "temp_rating_asc" => query.OrderBy(r => r.TempRating),
            "priority_desc" => query.OrderByDescending(r => r.ModerationStatus == pendingStatus ? 3 : r.ModerationStatus == rejectedStatus ? 2 : r.ModerationStatus == approvedStatus ? 1 : 0).ThenByDescending(r => r.UpdatedAt),
            "priority_asc" => query.OrderBy(r => r.ModerationStatus == pendingStatus ? 3 : r.ModerationStatus == rejectedStatus ? 2 : r.ModerationStatus == approvedStatus ? 1 : 0).ThenByDescending(r => r.UpdatedAt),
            "threat_desc" => query.OrderByDescending(r => r.AiModerationStatus == flaggedStatus ? 4 : r.AiModerationStatus == manualAuditStatus ? 3 : r.AiModerationStatus == approvedStatus ? 2 : 1),
            "threat_asc" => query.OrderBy(r => r.AiModerationStatus == flaggedStatus ? 4 : r.AiModerationStatus == manualAuditStatus ? 3 : r.AiModerationStatus == approvedStatus ? 2 : 1),
            _ => query.OrderByDescending(r => r.ModerationStatus == pendingStatus ? 3 : r.ModerationStatus == rejectedStatus ? 2 : r.ModerationStatus == approvedStatus ? 1 : 0).ThenByDescending(r => r.UpdatedAt)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((request.Page - 1) * request.PageSize)
                               .Take(request.PageSize)
                               .ToListAsync();

        return (items, total);
    }

    public async Task<(System.Collections.Generic.List<LessonReviewModerationRecord> Items, int TotalCount)> GetLessonReviewModerationRecordsAsync(PagedReviewModerationRequest request)
    {
        var query = _context.LessonReviewModerationRecords
            .Include(r => r.LessonReview)
                .ThenInclude(lr => lr.Enrollment)
                    .ThenInclude(e => e.User)
                        .ThenInclude(u => u.UserNavigation)
            .Include(r => r.LessonReview)
                .ThenInclude(lr => lr.Lesson)
            .Include(r => r.LessonReview)
                .ThenInclude(lr => lr.Enrollment)
                    .ThenInclude(e => e.Course)
            .AsNoTracking()
            .AsQueryable();

        string pendingStatus = ModerationStatus.Pending.ToValue().ToLower();
        string approvedStatus = ModerationStatus.Approved.ToValue().ToLower();
        string rejectedStatus = ModerationStatus.Rejected.ToValue().ToLower();
        string flaggedStatus = ModerationStatus.Flagged.ToValue().ToLower();
        string manualAuditStatus = ModerationStatus.ManualAudit.ToValue().ToLower();

        // Filters
        if (!string.IsNullOrEmpty(request.ModerationStatus) && request.ModerationStatus.ToLower() != "all")
        {
            query = query.Where(r => r.ModerationStatus == request.ModerationStatus.ToLower());
        }

        if (!string.IsNullOrEmpty(request.RequestType) && request.RequestType.ToLower() != "both")
        {
            bool isUpdate = request.RequestType.ToLower() == "edit";
            query = query.Where(r => r.IsUpdate == isUpdate);
        }

        if (!string.IsNullOrEmpty(request.AiModerationStatus) && request.AiModerationStatus.ToLower() != "all")
        {
            query = query.Where(r => r.AiModerationStatus == request.AiModerationStatus.ToLower());
        }

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search;
            query = query.Where(r => 
                EF.Functions.ILike(r.TempComment, $"%{s}%") ||
                (r.LessonReview.Comment != null && EF.Functions.ILike(r.LessonReview.Comment, $"%{s}%")) ||
                (r.LessonReview.Enrollment.User.UserNavigation.Username != null && EF.Functions.ILike(r.LessonReview.Enrollment.User.UserNavigation.Username, $"%{s}%")) ||
                EF.Functions.ILike(r.LessonReview.Enrollment.User.FullName, $"%{s}%") ||
                (r.LessonReview.Enrollment.User.UserNavigation.Email != null && EF.Functions.ILike(r.LessonReview.Enrollment.User.UserNavigation.Email, $"%{s}%")) ||
                EF.Functions.ILike(r.LessonReview.Enrollment.Course.Title, $"%{s}%") ||
                (r.LessonReview.Lesson != null && EF.Functions.ILike(r.LessonReview.Lesson.Title, $"%{s}%"))
            );
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "updated_at_desc" => query.OrderByDescending(r => r.UpdatedAt),
            "updated_at_asc" => query.OrderBy(r => r.UpdatedAt),
            "temp_rating_desc" => query.OrderByDescending(r => r.TempRating),
            "temp_rating_asc" => query.OrderBy(r => r.TempRating),
            "priority_desc" => query.OrderByDescending(r => r.ModerationStatus == pendingStatus ? 3 : r.ModerationStatus == rejectedStatus ? 2 : r.ModerationStatus == approvedStatus ? 1 : 0).ThenByDescending(r => r.UpdatedAt),
            "priority_asc" => query.OrderBy(r => r.ModerationStatus == pendingStatus ? 3 : r.ModerationStatus == rejectedStatus ? 2 : r.ModerationStatus == approvedStatus ? 1 : 0).ThenByDescending(r => r.UpdatedAt),
            "threat_desc" => query.OrderByDescending(r => r.AiModerationStatus == flaggedStatus ? 4 : r.AiModerationStatus == manualAuditStatus ? 3 : r.AiModerationStatus == approvedStatus ? 2 : 1),
            "threat_asc" => query.OrderBy(r => r.AiModerationStatus == flaggedStatus ? 4 : r.AiModerationStatus == manualAuditStatus ? 3 : r.AiModerationStatus == approvedStatus ? 2 : 1),
            _ => query.OrderByDescending(r => r.ModerationStatus == pendingStatus ? 3 : r.ModerationStatus == rejectedStatus ? 2 : r.ModerationStatus == approvedStatus ? 1 : 0).ThenByDescending(r => r.UpdatedAt)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((request.Page - 1) * request.PageSize)
                               .Take(request.PageSize)
                               .ToListAsync();

        return (items, total);
    }
}
