using Microsoft.AspNetCore.SignalR;

namespace Study_Hub.Hubs
{
    public class NotificationHub : Hub
    {
        // Call this from admin clients to join the admins group
        public async Task JoinAdmins()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }

        // Optional: allow leaving
        public async Task LeaveAdmins()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");
        }
    }
}

