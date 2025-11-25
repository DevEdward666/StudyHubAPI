using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Study_Hub.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation("Client connected: {ConnectionId}, User: {UserId}", Context.ConnectionId, userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation("Client disconnected: {ConnectionId}, User: {UserId}", Context.ConnectionId, userId);
            
            if (exception != null)
            {
                _logger.LogError(exception, "Client disconnected with error");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> JoinAdmins()
        {
            // Check if user is admin
            var userRole = Context.User?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
                          ?? Context.User?.FindFirst("role")?.Value;

            _logger.LogInformation("JoinAdmins called by {ConnectionId}, Role: {Role}", Context.ConnectionId, userRole ?? "None");

            // if (userRole == "Admin" || userRole == "Super Admin")
            // {
                await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
                _logger.LogInformation("✅ User {ConnectionId} joined admins group (Role: {Role})", Context.ConnectionId, userRole);
                return true;
            // }
            // else
            // {
            //     _logger.LogWarning("❌ User {ConnectionId} attempted to join admins group but has role: {Role}", Context.ConnectionId, userRole ?? "None");
            //     return false;
            // }
        }

        public async Task LeaveAdmins()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");
            _logger.LogInformation("User {ConnectionId} left admins group", Context.ConnectionId);
        }

        public Task<object> GetDiagnostics()
        {
            var userRole = Context.User?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
                          ?? Context.User?.FindFirst("role")?.Value;
            
            var userId = Context.UserIdentifier;
            
            var diagnostics = new
            {
                connectionId = Context.ConnectionId,
                userId = userId,
                userRole = userRole,
                isAdmin = userRole == "Admin" || userRole == "Super Admin",
                claims = Context.User?.Claims.Select(c => new { c.Type, c.Value }).ToList()
            };

            _logger.LogInformation("Diagnostics requested: {@Diagnostics}", diagnostics);
            
            return Task.FromResult<object>(diagnostics);
        }
    }
}

