using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace POSIMSWebApi.SignalR
{
    
    [Authorize]
    public sealed class NotificationHub : Hub
    {
        private static readonly Dictionary<string, string> _connections = new Dictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name; // Or however you get the user's ID
            if (!string.IsNullOrEmpty(userId))
            {
                _connections[Context.ConnectionId] = userId;
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            if (_connections.ContainsKey(connectionId))
            {
                _connections.Remove(connectionId);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string message, string userId)
        {
            var connectionId = _connections.FirstOrDefault(c => c.Value == userId).Key;
            if (connectionId != null)
            {
                await Clients.Client(connectionId).SendAsync("AdminNotification", message);
            }
        }
    }
}
