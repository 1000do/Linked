using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Application.IServices;

public interface ICourseService
{
    Task<IEnumerable<CourseResponse>> GetAllPublishedCoursesAsync(int? userId = null);
    Task<IEnumerable<CourseResponse>> GetInstructorCoursesAsync(int instructorId);
    Task<CourseDetailResponse?> GetCourseWithDetailsAsync(int courseId, int instructorId, int? userId = null);
    Task<bool> IsEnrolledAsync(int userId, int courseId);
    Task<CourseResponse> CreateCourseAsync(CourseCreateRequest request, int instructorId);
    Task<CourseResponse> UpdateCourseAsync(int courseId, CourseUpdateRequest request, int instructorId);
    Task UpdateCourseStatusAsync(int courseId, string status, int instructorId);
    Task DeleteCourseAsync(int courseId, int instructorId);
    Task<IEnumerable<CategoryResponse>> GetCategoriesAsync();
    Task<CourseAIIntegrationResult> IntegrateAItoCourseAsync(CourseAIIntegrationCommand command);
    Task<CourseAiIntegrationResponse> GetByModelAndCourseAsync(int modelId, int courseId);
    Task UpdateCourseStatusAndFeedbackAsync(int courseId, string status, string? feedback);
}
