using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.Services
{
    public class HubService : IHubService
    {
        private readonly IHubContext<InstructorApprovalHub> _instructorApprovalHubContext;

        public HubService(IHubContext<InstructorApprovalHub> instructorApprovalHubContext)
        {
            _instructorApprovalHubContext = instructorApprovalHubContext;
        }

        public async Task SendInstructorApplicationUpdateAsync()
        {
            await _instructorApprovalHubContext.Clients.All.SendAsync("InstructorApplicationUpdated");
            await _instructorApprovalHubContext.Clients.Group("managers").SendAsync("InstructorApplicationUpdated");
        }
    }
}
