using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub : Hub<IPresenceClient>
{
    private readonly PresenceTracker _tracker;

    public PresenceHub(PresenceTracker tracker)
    {
        _tracker = tracker;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.User.FindFirst(ClaimTypes.Name)?.Value;

        var isOnline = await _tracker.UserConnected(username, Context.ConnectionId);

        if (isOnline)
            await Clients.Others.UserIsOnline(username);

        var onlineUsers = await _tracker.GetOnlineUsers();
        await Clients.Caller.GetOnlineUsers(onlineUsers);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = Context.User.FindFirst(ClaimTypes.Name)?.Value;

        var isOffline = await _tracker.UserDisconnected(username, Context.ConnectionId);

        if (isOffline)
            await Clients.Others.UserIsOffline(username);

        await base.OnDisconnectedAsync(exception);
    }
}