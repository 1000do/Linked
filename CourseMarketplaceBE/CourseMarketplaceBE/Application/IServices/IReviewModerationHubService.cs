using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IReviewModerationHubService
    {
        Task SendReviewUpdateAsync();
    }
}
