using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface ICourseQueryService
{
    Task<IEnumerable<CourseResponse>> GetAllPublishedCoursesAsync(int? userId = null);
    Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseMarketplaceBE.Application.DTOs.CourseResponse>> GetPublishedCoursesPagedAsync(
        string? query = null,
        string? category = null,
        string? sort = null,
        string? price = null,
        string? rating = null,
        int? page = null,
        int? pageSize = null,
        int? userId = null);
    Task<IEnumerable<CourseResponse>> GetInstructorCoursesAsync(int instructorId);
    Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<CourseMarketplaceBE.Application.DTOs.CourseResponse>> GetInstructorCoursesPagedAsync(
        int instructorId,
        string? search = null,
        string? status = null,
        int? page = null,
        int? pageSize = null);
    Task<CourseDetailResponse?> GetCourseWithDetailsAsync(int courseId, int instructorId, int? userId = null);
    Task<bool> IsEnrolledAsync(int userId, int courseId);
    Task<IEnumerable<CategoryResponse>> GetCategoriesAsync();
    Task<CourseAiIntegrationResponse> GetByModelAndCourseAsync(int modelId, int courseId);
}
