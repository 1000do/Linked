using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CourseMarketplaceBE.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // Hub này d? tr?ng cung du?c, nó dùng d? d?nh danh k?t n?i
    }
}
