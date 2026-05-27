using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Application.IServices;

public interface ICourseCommandService
{
    Task<CourseResponse> CreateCourseAsync(CourseCreateRequest request, int instructorId);
    Task<CourseResponse> UpdateCourseAsync(int courseId, CourseUpdateRequest request, int instructorId);
    Task UpdateCourseStatusAsync(int courseId, string status, int instructorId);
    Task DeleteCourseAsync(int courseId, int instructorId);
    Task<CourseAIIntegrationResult> IntegrateAItoCourseAsync(CourseAIIntegrationCommand command);
    Task UpdateCourseStatusAndFeedbackAsync(int courseId, string status, string? feedback);
}
