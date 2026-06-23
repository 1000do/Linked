using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Application.IServices;

public interface IModerationPenaltyService
{
    Task<bool> ProcessCourseStrikeAsync(int courseId, string resolutionNote);
    Task<bool> ProcessReviewStrikeAsync(int userId, string resolutionNote, string? linkAction);
    Task<bool> NotifyStudentsAboutInstructorSuspensionAsync(int instructorId);
}
