using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;

namespace API.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, List<string>> OnlineUsers = [];

        public Task<bool> UserConnected(string username, string connectionId)
        {
            var isOnline = false;
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    OnlineUsers.Add(username, [connectionId]);
                    isOnline = true;
                }
            }
            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            var isOffline = false;
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);
                OnlineUsers[username].Remove(connectionId);

                if (OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                    isOffline = true;
                }
            }
            return Task.FromResult(isOffline);
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;
            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(x => x.Key).ToArray();
            }
            return Task.FromResult(onlineUsers);
        }

        public static Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            if (OnlineUsers.TryGetValue(username, out var connections))
            {
                lock (connections)
                {
                    connectionIds = [.. connections]; //This is new feature of c#12, previous way of doing it is, connections.ToList();

                }
            }
            else
            {
                connectionIds = [];
            }
            return Task.FromResult(connectionIds);
        }
    }
}