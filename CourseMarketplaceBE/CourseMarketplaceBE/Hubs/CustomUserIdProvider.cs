using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace CourseMarketplaceBE.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Trình định danh SignalR sẽ khớp với ID (int) lưu trong JWT của bạn
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
