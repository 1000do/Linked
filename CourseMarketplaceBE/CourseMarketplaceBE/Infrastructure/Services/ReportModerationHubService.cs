using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Infrastructure.Services
{
    public class ReportModerationHubService : IReportModerationHubService
    {
        private readonly IHubContext<ReportModerationHub> _hubContext;

        public ReportModerationHubService(IHubContext<ReportModerationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendReportUpdateAsync()
        {
            await _hubContext.Clients.Group("managers").SendAsync("ReceiveReportUpdate");
        }
    }
}
