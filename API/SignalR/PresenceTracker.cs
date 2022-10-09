namespace API.SignalR;

public class PresenceTracker
{
    private static readonly Dictionary<string, List<string>> OnlineUsers = new();

    public Task<bool> UserConnected(string username, string connectionId)
    {
        bool isOnline = false;
        lock (OnlineUsers)
        {
            if (OnlineUsers.ContainsKey(username))
            {
                OnlineUsers[username].Add(connectionId);
            }
            else
            {
                OnlineUsers.Add(username, new List<string> { connectionId });
                isOnline = true;
            }
        }
        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string username, string connectionId)
    {
        bool isOffline = false;

        if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

        lock (OnlineUsers)
        {
            OnlineUsers[username].Remove(connectionId);
            if (OnlineUsers[username].Count == 0) 
            {
                OnlineUsers.Remove(username);
                isOffline = true;
            }
            return Task.FromResult(isOffline);
        }
    }

    public Task<string[]> GetOnlineUsers()
    {
        string[] onlineUsers = OnlineUsers.OrderBy(x => x.Key).Select(x => x.Key).ToArray();
        return Task.FromResult(onlineUsers);
    }

    public Task<List<string>> GetUserConnections(string username)
    {
        List<string> connectionIds = OnlineUsers.GetValueOrDefault(username);

        return Task.FromResult(connectionIds);
    }
}