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
        private readonly IHubContext<InstructorApprovalHub> _instructorApprovalHubContext;

        public HubService(
            IHubContext<InstructorApprovalHub> instructorApprovalHubContext,
            IHubContext<ReportModerationHub> reportHubContext,
            IHubContext<ReviewModerationHub> reviewHubContext,
            IHubContext<CourseModerationHub> courseHubContext)
        {
            _instructorApprovalHubContext = instructorApprovalHubContext;
            _reportHubContext = reportHubContext;
            _reviewHubContext = reviewHubContext;
            _courseHubContext = courseHubContext;
        }

        public async Task SendInstructorApplicationUpdateAsync()
        {
            await _instructorApprovalHubContext.Clients.All.SendAsync("InstructorApplicationUpdated");
            await _instructorApprovalHubContext.Clients.Group("managers").SendAsync("InstructorApplicationUpdated");
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