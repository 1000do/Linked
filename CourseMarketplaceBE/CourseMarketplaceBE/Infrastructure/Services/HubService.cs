using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Hubs;

namespace CourseMarketplaceBE.Infrastructure.Services
{
    public class HubService : IHubService
    {
        private readonly IHubContext<ReportModerationHub> _reportHubContext;
        private readonly IHubContext<ReviewModerationHub> _reviewHubContext;
        private readonly IHubContext<CourseModerationHub> _courseHubContext;

        public HubService(
            IHubContext<ReportModerationHub> reportHubContext,
            IHubContext<ReviewModerationHub> reviewHubContext,
            IHubContext<CourseModerationHub> courseHubContext)
        {
            _reportHubContext = reportHubContext;
            _reviewHubContext = reviewHubContext;
            _courseHubContext = courseHubContext;
        }

        public async Task SendReportUpdateAsync()
        {
            await _reportHubContext.Clients.All.SendAsync("ReceiveReportUpdate");
        }

        public async Task SendReviewUpdateAsync()
        {
            await _reviewHubContext.Clients.All.SendAsync("ReceiveReviewUpdate");
        }

        public async Task SendCourseUpdateAsync()
        {
            await _courseHubContext.Clients.All.SendAsync("ReceiveCourseUpdate");
        }
    }
}
