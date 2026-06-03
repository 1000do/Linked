using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;

    public CourseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Course>> GetInstructorCoursesAsync(int instructorId)
    {
        return await _context.Courses
            .Where(c => c.InstructorId == instructorId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(IEnumerable<Course> Courses, int TotalCount)> GetInstructorCoursesPagedAsync(
        int instructorId,
        string? search = null,
        string? status = null,
        int? page = null,
        int? pageSize = null)
    {
        var queryable = _context.Courses
            .Where(c => c.InstructorId == instructorId)
            .AsNoTracking();

        // 1. Filtering by search term
        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            queryable = queryable.Where(c => c.Title.ToLower().Contains(searchLower));
        }

        // 2. Filtering by status
        if (!string.IsNullOrEmpty(status) && !string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
        {
            queryable = queryable.Where(c => c.CourseStatus == status.ToLower());
        }

        // 3. Sorting (default: newest update / modified)
        queryable = queryable.OrderByDescending(c => c.UpdatedAt);

        var totalCount = await queryable.CountAsync();

        // 4. Pagination
        if (page.HasValue && pageSize.HasValue)
        {
            queryable = queryable.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        var courses = await queryable.ToListAsync();
        return (courses, totalCount);
    }

    public async Task<IEnumerable<Course>> GetAllPublishedCoursesAsync()
    {
        return await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
                .ThenInclude(i => i!.InstructorNavigation)
            .Where(c => c.CourseStatus == CourseStatus.Published.ToValue())
            .OrderByDescending(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(IEnumerable<Course> Courses, int TotalCount)> GetAllPublishedCoursesPagedAsync(
        string? search = null, 
        string? category = null, 
        string? sort = null, 
        string? price = null,
        string? rating = null,
        int? page = null, 
        int? pageSize = null)
    {
        var queryable = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
                .ThenInclude(i => i!.InstructorNavigation)
            .Where(c => c.CourseStatus == CourseStatus.Published.ToValue())
            .AsNoTracking();

        // 0.1. Filtering by price
        if (!string.IsNullOrEmpty(price) && price != "all")
        {
            if (price == "free")
            {
                queryable = queryable.Where(c => c.Price == 0);
            }
            else if (price == "paid")
            {
                queryable = queryable.Where(c => c.Price > 0);
            }
        }

        // 0.2. Filtering by rating
        if (!string.IsNullOrEmpty(rating) && rating != "all")
        {
            if (decimal.TryParse(rating, out decimal minRating))
            {
                queryable = from c in queryable
                            join s in _context.CourseStats on c.CourseId equals s.CourseId into statsGroup
                            from s in statsGroup.DefaultIfEmpty()
                            where s != null && s.RatingAverage >= (double)minRating
                            select c;
            }
        }

        // 1. Filtering by search term
        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            queryable = queryable.Where(c => c.Title.ToLower().Contains(searchLower) || 
                                             (c.Description != null && c.Description.ToLower().Contains(searchLower)));
        }

        // 2. Filtering by category
        if (!string.IsNullOrEmpty(category))
        {
            queryable = queryable.Where(c => c.Category != null && c.Category.CategoriesName == category);
        }

        // 3. Sorting & Joins
        if (sort == "price_asc")
        {
            queryable = queryable.OrderBy(c => c.Price);
        }
        else if (sort == "price_desc")
        {
            queryable = queryable.OrderByDescending(c => c.Price);
        }
        else if (sort == "rating")
        {
            queryable = from c in queryable
                        join s in _context.CourseStats on c.CourseId equals s.CourseId into statsGroup
                        from s in statsGroup.DefaultIfEmpty()
                        orderby s.RatingAverage descending
                        select c;
        }
        else // default: newest
        {
            queryable = queryable.OrderByDescending(c => c.CreatedAt);
        }

        var totalCount = await queryable.CountAsync();

        // 4. Pagination
        if (page.HasValue && pageSize.HasValue)
        {
            queryable = queryable.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        var courses = await queryable.ToListAsync();
        return (courses, totalCount);
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        return await _context.Categories.AsNoTracking().ToListAsync();
    }

    public async Task<bool> IsEnrolledAsync(int userId, int courseId)
    {
        return await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId && e.EnrollmentStatus != EnrollmentStatus.Revoked.ToValue());
    }

    public async Task<IEnumerable<Course>> GetEnrolledCoursesAsync(int userId)
    {
        return await _context.Enrollments
            .Where(e => e.UserId == userId && e.EnrollmentStatus != EnrollmentStatus.Revoked.ToValue())
            .Include(e => e.Course)
                .ThenInclude(c => c!.Instructor)
                    .ThenInclude(i => i!.InstructorNavigation)
            .Include(e => e.Course)
                .ThenInclude(c => c!.Category)
            .IgnoreQueryFilters() // Allow access to soft-deleted courses for enrolled students
            .Select(e => e.Course!)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Course?> GetCourseWithDetailsAsync(int courseId)
    {
        return await _context.Courses
            .IgnoreQueryFilters()
            .Include(c => c.Category)
            .Include(c => c.Coupon)
            .Include(c => c.Instructor)
                .ThenInclude(i => i!.InstructorNavigation)
                    .ThenInclude(u => u.UserNavigation)
            .Include(c => c.Lessons.Where(l => !l.IsRemoved).OrderBy(l => l.LessonId))
                .ThenInclude(l => l.LearningMaterials.Where(m => m.LearningStatus != LearningStatus.Removed.ToValue()).OrderBy(m => m.MaterialId))
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CourseId == courseId);
    }

    public async Task<Course?> GetByIdAsync(int courseId)
    {
        return await _context.Courses.FindAsync(courseId);
    }

    public async Task<bool> HasEnrollmentsAsync(int courseId)
    {
        return await _context.Enrollments.AnyAsync(e => e.CourseId == courseId);
    }

    public async Task<IEnumerable<CourseStats>> GetCourseStatsAsync(IEnumerable<int> courseIds)
    {
        var statsList = await _context.CourseStats
            .Where(s => courseIds.Contains(s.CourseId))
            .ToListAsync();

        if (statsList.Any())
        {
            var courses = await _context.Courses.AsNoTracking()
                .Where(c => courseIds.Contains(c.CourseId))
                .ToListAsync();

            foreach (var stats in statsList)
            {
                var course = courses.FirstOrDefault(c => c.CourseId == stats.CourseId);
                if (course != null && course.InstructorId.HasValue)
                {
                    var isInstructorEnrolled = await _context.Enrollments.AnyAsync(e => e.CourseId == stats.CourseId && e.UserId == course.InstructorId.Value);
                    if (isInstructorEnrolled)
                    {
                        stats.TotalStudents = Math.Max(0, stats.TotalStudents - 1);
                    }
                }
            }
        }
        return statsList;
    }

    public async Task<CourseStats?> GetCourseStatsAsync(int courseId)
    {
        var stats = await _context.CourseStats.FirstOrDefaultAsync(s => s.CourseId == courseId);
        if (stats != null)
        {
            var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course != null && course.InstructorId.HasValue)
            {
                var isInstructorEnrolled = await _context.Enrollments.AnyAsync(e => e.CourseId == courseId && e.UserId == course.InstructorId.Value);
                if (isInstructorEnrolled)
                {
                    stats.TotalStudents = Math.Max(0, stats.TotalStudents - 1);
                }
            }
        }
        return stats;
    }

    public async Task AddAsync(Course course)
    {
        await _context.Courses.AddAsync(course);
    }

    public void Update(Course course)
    {
        _context.Courses.Update(course);
    }

    public void Delete(Course course)
    {
        _context.Courses.Remove(course);
    }

    public async Task<int> GetTotalPublishedCoursesCountAsync()
    {
        return await _context.Courses.CountAsync(c => c.CourseStatus == CourseStatus.Published.ToValue());
    }

    public async Task<decimal> GetAveragePlatformRatingAsync()
    {
        var avg = await _context.CourseStats
            .Where(s => s.RatingAverage > 0)
            .Select(s => (double?)s.RatingAverage)
            .AverageAsync();
        return (decimal)(avg ?? 0.0);
    }

    public async Task<CourseModerationStatsDto> GetCourseModerationStatsAsync()
    {
        var today = DateTime.Today;

        var pendingCount = await _context.Courses
            .Where(c => c.CourseStatus == CourseStatus.Pending.ToValue() && !c.IsRemoved)
            .CountAsync();

        var resolvedTodayCount = await _context.Courses
            .Where(c => (c.CourseStatus == CourseStatus.Published.ToValue() || c.CourseStatus == CourseStatus.Rejected.ToValue())
                     && c.UpdatedAt >= today
                     && !c.IsRemoved)
            .CountAsync();

        return new CourseModerationStatsDto
        {
            PendingCount = pendingCount,
            ResolvedTodayCount = resolvedTodayCount
        };
    }

    public async Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseModerationDto>> GetPendingCoursesModerationAsync(ModerationFilterDto filter)
    {
        var query = _context.Courses
            .Include(c => c.Instructor).ThenInclude(i => i.InstructorNavigation)
            .Include(c => c.Category)
            .AsQueryable();

        // Always exclude draft courses from moderation interface
        query = query.Where(c => c.CourseStatus != CourseStatus.Draft.ToValue());

        // 1. Filter by Status (Default is all)
        var statusFilter = string.IsNullOrEmpty(filter.Status) ? "all" : filter.Status;
        if (statusFilter != "all")
        {
            query = query.Where(c => c.CourseStatus == statusFilter);
        }

        // 2. Search (Title or Instructor)
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(search) || 
                                   c.Instructor!.InstructorNavigation!.FullName.ToLower().Contains(search));
        }

        // 3. Category Filter
        if (!string.IsNullOrEmpty(filter.Category) && filter.Category != "0")
        {
            query = query.Where(c => c.Category!.CategoriesName == filter.Category || c.CategoryId.ToString() == filter.Category);
        }

        // 4. Sorting (Urgency: oldest first)
        if (filter.SortBy == "newest")
        {
            query = query.OrderByDescending(c => c.CreatedAt);
        }
        else
        {
            // default: oldest first (more urgent)
            query = query.OrderBy(c => c.CreatedAt);
        }

        var totalCount = await query.CountAsync();

        var courses = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
        var now = DateTime.Now;

        var items = courses.Select(c => {
            var dto = new CourseModerationDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                InstructorName = c.Instructor?.InstructorNavigation?.FullName ?? "Unknown",
                CategoryName = c.Category?.CategoriesName,
                Price = c.Price,
                CreatedAt = c.CreatedAt,
                CourseStatus = c.CourseStatus,
                CourseThumbnailUrl = c.CourseThumbnailUrl,
                FlagCount = c.CourseFlagCount ?? 0,
                IsRemoved = c.IsRemoved
            };

            // Calculate Urgency
            if (c.CreatedAt.HasValue && c.CourseStatus == CourseStatus.Pending.ToValue())
            {
                var diff = now - c.CreatedAt.Value;
                if (diff.TotalHours > 72)
                {
                    dto.UrgencyLevel = "High";
                    dto.UrgencyColor = "red";
                }
                else if (diff.TotalHours > 24)
                {
                    dto.UrgencyLevel = "Medium";
                    dto.UrgencyColor = "amber";
                }
                else
                {
                    dto.UrgencyLevel = "Normal";
                    dto.UrgencyColor = "slate";
                }
            }

            return dto;
        }).ToList();

        return new CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseModerationDto>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<bool> IsOwnerAsync(int userId, int courseId)
    {
        return await _context.Courses.AnyAsync(c => c.CourseId == courseId && c.InstructorId == userId);
    }

    public async Task<Course?> GetCourseWithInstructorAsync(int courseId)
    {
        return await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Coupon)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);
    }

    public async Task<Enrollment?> GetActiveEnrollmentAsync(int userId, int courseId)
    {
        return await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId
                                   && e.CourseId == courseId
                                   && e.EnrollmentStatus == EnrollmentStatus.Active.ToValue());
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
