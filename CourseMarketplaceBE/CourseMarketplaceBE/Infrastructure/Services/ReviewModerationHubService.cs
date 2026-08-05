using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Infrastructure.Services
{
    public class ReviewModerationHubService : IReviewModerationHubService
    {
        private readonly IHubContext<ReviewModerationHub> _hubContext;

        public ReviewModerationHubService(IHubContext<ReviewModerationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendReviewUpdateAsync()
        {
            await _hubContext.Clients.Group("managers").SendAsync("ReceiveReviewUpdate");
        }
    }
}
