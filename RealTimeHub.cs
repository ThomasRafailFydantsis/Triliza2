using Microsoft.AspNetCore.SignalR;

namespace triliza
{
    public class RealTimeHub:Hub
    {
        //public async Task SendMessage(string user, string message)
        //{
        //    await Clients.All.SendAsync("ReceiveMessage", user, message);
        //}

        //public async Task SendNotification(string message)
        //{
        //    await Clients.All.SendAsync("ReceiveNotification", message);
        //}
        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("PlayerJoined", Context.User?.Identity?.Name ?? "Unknown");
        }

        public async Task SendMove(string roomId, object move)
        {

            var g = Clients.Group(roomId);
            await g.SendAsync("ReceiveMove", Context.User?.Identity?.Name, move);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
