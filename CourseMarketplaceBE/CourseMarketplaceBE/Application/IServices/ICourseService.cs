using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface ICourseService
{
    Task<IEnumerable<CourseResponse>> GetAllPublishedCoursesAsync(int? userId = null);
    Task<PaginatedCoursesResponse> GetPublishedCoursesPagedAsync(
        string? query = null,
        string? category = null,
        string? sort = null,
        string? price = null,
        string? rating = null,
        int? page = null,
        int? pageSize = null,
        int? userId = null);
    Task<IEnumerable<CourseResponse>> GetInstructorCoursesAsync(int instructorId);
    Task<PaginatedCoursesResponse> GetInstructorCoursesPagedAsync(
        int instructorId,
        string? search = null,
        string? status = null,
        int? page = null,
        int? pageSize = null);
    Task<CourseDetailResponse?> GetCourseWithDetailsAsync(int courseId, int instructorId, int? userId = null);
    Task<bool> IsEnrolledAsync(int userId, int courseId);
    Task<CourseResponse> CreateCourseAsync(CourseCreateRequest request, int instructorId);
    Task<CourseResponse> UpdateCourseAsync(int courseId, CourseUpdateRequest request, int instructorId);
    Task UpdateCourseStatusAsync(int courseId, string status, int instructorId);
    Task DeleteCourseAsync(int courseId, int instructorId);
    Task<IEnumerable<CategoryResponse>> GetCategoriesAsync();
}
