using Microsoft.AspNetCore.SignalR;

namespace ChessApp.Server.Hubs;

public class ChessHub : Hub
{
    public async Task JoinGame(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        await Clients.Group(gameId).SendAsync("PlayerJoined", Context.ConnectionId);
    }

    public async Task SendMove(string gameId, string moveFen)
    {
        // Send the move to all clients in the same game room except the sender
        await Clients.OthersInGroup(gameId).SendAsync("ReceiveMove", moveFen);
    }
}
