using CourseMarketplaceBE.Application.DTOs;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices;

public interface IReviewAiModerationService
{
    Task<ReviewAiModerationResponse> HandleReviewAiModerationAsync(TempReviewDto tempDto);
}
