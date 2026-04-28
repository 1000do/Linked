using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
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

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
