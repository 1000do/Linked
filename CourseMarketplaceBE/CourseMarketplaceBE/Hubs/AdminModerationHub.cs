using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Hubs
{
    [Authorize]
    public class AdminModerationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var roleClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value 
                            ?? Context.User?.FindFirst("role")?.Value;

            if (roleClaim == "admin" || roleClaim == "staff")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "admin_moderators");
            }
            await base.OnConnectedAsync();
        }
    }
}
