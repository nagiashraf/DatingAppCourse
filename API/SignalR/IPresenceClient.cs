using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public interface IPresenceClient
    {
        Task UserIsOnline(string username);
        Task UserIsOffline(string username);
        Task GetOnlineUsers(string[] onlineUsers);
    }
}