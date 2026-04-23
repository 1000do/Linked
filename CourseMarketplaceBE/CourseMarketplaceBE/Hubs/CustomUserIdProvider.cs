using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

public class CustomUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        // Lấy ID từ Claim NameIdentifier (trùng với lúc bạn tạo Token)
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}