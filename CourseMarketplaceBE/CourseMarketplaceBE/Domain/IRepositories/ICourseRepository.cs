using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface ICourseRepository
{
    Task<IEnumerable<Course>> GetInstructorCoursesAsync(int instructorId);
    Task<(IEnumerable<Course> Courses, int TotalCount)> GetInstructorCoursesPagedAsync(
        int instructorId,
        string? search = null,
        string? status = null,
        int? page = null,
        int? pageSize = null);
    Task<Course?> GetCourseWithDetailsAsync(int courseId);
    Task<Course?> GetByIdAsync(int courseId);
    Task<bool> HasEnrollmentsAsync(int courseId);
    Task<IEnumerable<Course>> GetAllPublishedCoursesAsync();
    Task<(IEnumerable<Course> Courses, int TotalCount)> GetAllPublishedCoursesPagedAsync(
        string? search = null, 
        string? category = null, 
        string? sort = null, 
        string? price = null,
        string? rating = null,
        int? page = null, 
        int? pageSize = null);
    Task<bool> IsEnrolledAsync(int userId, int courseId);
    Task<IEnumerable<Course>> GetEnrolledCoursesAsync(int userId);
    Task<IEnumerable<Category>> GetCategoriesAsync();
    Task<IEnumerable<CourseStats>> GetCourseStatsAsync(IEnumerable<int> courseIds);
    Task<CourseStats?> GetCourseStatsAsync(int courseId);
    Task AddAsync(Course course);
    void Update(Course course);
    void Delete(Course course);
    Task<int> GetTotalPublishedCoursesCountAsync();
    Task<decimal> GetAveragePlatformRatingAsync();
    Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseModerationDto>> GetPendingCoursesModerationAsync(ModerationFilterDto filter);
    Task<CourseModerationStatsDto> GetCourseModerationStatsAsync();
    Task<bool> IsOwnerAsync(int userId, int courseId);
    Task SaveChangesAsync();
}
