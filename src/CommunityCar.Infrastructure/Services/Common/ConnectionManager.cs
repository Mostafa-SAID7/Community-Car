using System.Collections.Concurrent;

namespace CommunityCar.Infrastructure.Services.Common;

public interface IConnectionManager
{
    void OnConnected(Guid userId, string connectionId);
    void OnDisconnected(Guid userId, string connectionId);
    
    // Group Management
    void AddToGroup(string groupName, string connectionId);
    void RemoveFromGroup(string groupName, string connectionId);
    
    // Queries
    bool IsUserOnline(Guid userId);
    List<string> GetUserConnections(Guid userId);
    int GetOnlineUserCount();
    List<Guid> GetOnlineUserIds();
    int GetGroupConnectionCount(string groupName);
}

public class ConnectionManager : IConnectionManager
{
    // Track user connections (userId -> set of connectionIds)
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();
    
    // Track connection to user mapping (connectionId -> userId)
    private readonly ConcurrentDictionary<string, Guid> _connectionUsers = new();
    
    // Track group memberships (groupName -> set of connectionIds)
    private readonly ConcurrentDictionary<string, HashSet<string>> _groupConnections = new();

    public void OnConnected(Guid userId, string connectionId)
    {
        _userConnections.AddOrUpdate(
            userId,
            new HashSet<string> { connectionId },
            (_, connections) =>
            {
                lock (connections)
                {
                    connections.Add(connectionId);
                }
                return connections;
            });

        _connectionUsers[connectionId] = userId;
    }

    public void OnDisconnected(Guid userId, string connectionId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
            }

            if (connections.Count == 0)
            {
                _userConnections.TryRemove(userId, out _);
            }
        }

        _connectionUsers.TryRemove(connectionId, out _);
    }

    public void AddToGroup(string groupName, string connectionId)
    {
        _groupConnections.AddOrUpdate(
            groupName,
            new HashSet<string> { connectionId },
            (_, connections) =>
            {
                lock (connections)
                {
                    connections.Add(connectionId);
                }
                return connections;
            });
    }

    public void RemoveFromGroup(string groupName, string connectionId)
    {
        if (_groupConnections.TryGetValue(groupName, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
            }

            if (connections.Count == 0)
            {
                _groupConnections.TryRemove(groupName, out _);
            }
        }
    }

    public bool IsUserOnline(Guid userId)
    {
        return _userConnections.ContainsKey(userId) && _userConnections[userId].Count > 0;
    }

    public List<string> GetUserConnections(Guid userId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                return connections.ToList();
            }
        }
        return new List<string>();
    }

    public int GetOnlineUserCount()
    {
        return _userConnections.Count;
    }

    public List<Guid> GetOnlineUserIds()
    {
        return _userConnections.Keys.ToList();
    }

    public int GetGroupConnectionCount(string groupName)
    {
        return _groupConnections.TryGetValue(groupName, out var connections) ? connections.Count : 0;
    }
}
