using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace foll_backend.NotificationCommunication.Interfaces.Realtime;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class NotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var claim = Context.User?.FindFirst("userId")?.Value;
        if (!long.TryParse(claim, out var userId) || userId <= 0)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroupName(userId));
        await base.OnConnectedAsync();
    }

    public static string GetUserGroupName(long userId) => $"user:{userId}";
}
