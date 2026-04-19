using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CourseMarketplaceBE.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // Hub này để trống cũng được, nó dùng để định danh kết nối
    }
}
