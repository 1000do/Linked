using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IReportModerationHubService
    {
        Task SendReportUpdateAsync();
    }
}
