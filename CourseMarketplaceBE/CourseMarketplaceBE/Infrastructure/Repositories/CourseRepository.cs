using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public async Task<IEnumerable<Course>> GetAllPublishedCoursesAsync()
    {
        return await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
                .ThenInclude(i => i!.InstructorNavigation)
            .Where(c => c.CourseStatus == "published")
            .OrderByDescending(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        return await _context.Categories.AsNoTracking().ToListAsync();
    }

    public async Task<bool> IsEnrolledAsync(int userId, int courseId)
    {
        return await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
    }

    public async Task<IEnumerable<Course>> GetEnrolledCoursesAsync(int userId)
    {
        return await _context.Enrollments
            .Where(e => e.UserId == userId)
            .Include(e => e.Course)
                .ThenInclude(c => c!.Instructor)
                    .ThenInclude(i => i!.InstructorNavigation)
            .Include(e => e.Course)
                .ThenInclude(c => c!.Category)
            .Select(e => e.Course!)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Course?> GetCourseWithDetailsAsync(int courseId)
    {
        return await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
                .ThenInclude(i => i!.InstructorNavigation)
                    .ThenInclude(u => u.UserNavigation)
            .Include(c => c.Lessons)
                .ThenInclude(l => l.LearningMaterials)
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
        return await _context.CourseStats
            .Where(s => courseIds.Contains(s.CourseId))
            .ToListAsync();
    }

    public async Task<CourseStats?> GetCourseStatsAsync(int courseId)
    {
        return await _context.CourseStats.FirstOrDefaultAsync(s => s.CourseId == courseId);
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
        return await _context.Courses.CountAsync(c => c.CourseStatus == "published");
    }

    public async Task<decimal> GetAveragePlatformRatingAsync()
    {
        return await _context.CourseStats
            .Where(s => s.RatingAverage > 0)
            .Select(s => (decimal)s.RatingAverage)
            .DefaultIfEmpty(0)
            .AverageAsync();
    }

    public async Task<List<CourseModerationDto>> GetPendingCoursesModerationAsync(ModerationFilterDto filter)
    {
        var query = _context.Courses
            .Include(c => c.Instructor).ThenInclude(i => i.InstructorNavigation)
            .Include(c => c.Category)
            .AsQueryable();

        // 1. Filter by Status (Default is pending)
        var statusFilter = string.IsNullOrEmpty(filter.Status) ? "pending" : filter.Status;
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

        var courses = await query.ToListAsync();
        var now = DateTime.Now;

        return courses.Select(c => {
            var dto = new CourseModerationDto
            {
                CourseId = c.CourseId,
                Title = c.Title,
                InstructorName = c.Instructor?.InstructorNavigation?.FullName ?? "Unknown",
                CategoryName = c.Category?.CategoriesName,
                Price = c.Price,
                CreatedAt = c.CreatedAt,
                CourseStatus = c.CourseStatus,
                CourseThumbnailUrl = c.CourseThumbnailUrl
            };

            // Calculate Urgency
            if (c.CreatedAt.HasValue && c.CourseStatus == "pending")
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
    }

    public async Task<bool> IsOwnerAsync(int userId, int courseId)
    {
        return await _context.Courses.AnyAsync(c => c.CourseId == courseId && c.InstructorId == userId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
