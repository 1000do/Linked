using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IHubService
    {
        Task SendReportUpdateAsync();
        Task SendReviewUpdateAsync();
        Task SendCourseUpdateAsync();
    }
}
