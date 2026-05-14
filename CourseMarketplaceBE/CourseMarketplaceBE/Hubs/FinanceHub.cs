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

    // Instructor join vào group riêng của họ dựa trên InstructorId
    public async Task JoinInstructorGroup(string instructorId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"InstructorFinance_{instructorId}");
    }
}
