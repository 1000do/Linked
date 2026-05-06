using Microsoft.AspNetCore.SignalR;

namespace CourseMarketplaceBE.Hubs;

/// <summary>
/// SignalR Hub để thông báo các sự kiện tài chính Real-time cho Admin.
/// </summary>
public class FinanceHub : Hub
{
    // Admin sẽ join vào group "AdminFinance" để nhận thông báo
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "AdminFinance");
    }
}
