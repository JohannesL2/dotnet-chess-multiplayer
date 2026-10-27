using ChessApp.Server.Services;
using ChessApp.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace ChessApp.Server.Hubs;

public sealed class ChessHub(IGameRoomManager roomManager) : Hub<IChessClient>
{
    public async Task<string> CreateRoom()
    {
        var roomId = roomManager.CreateRoom(Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Caller.AssignColor("White");
        return roomId;
    }

    public async Task JoinRoom(string roomId)
    {
        var result = roomManager.JoinRoom(roomId, Context.ConnectionId);
        if (!result.Succeeded)
        {
            await Clients.Caller.RoomError(result.Error!);
            return;
        }

        var normalizedRoomId = roomId.Trim().ToUpperInvariant();
        await Groups.AddToGroupAsync(Context.ConnectionId, normalizedRoomId);
        await Clients.Caller.AssignColor(result.Color!);
        if (result.GameStarted)
        {
            await Clients.Group(normalizedRoomId).GameStarted();
        }
    }

    public async Task SendMove(string roomId, MoveRequest move)
    {
        var result = roomManager.MakeMove(roomId, Context.ConnectionId, move);
        if (!result.Succeeded)
        {
            await Clients.Caller.MoveError(result.Error!);
            return;
        }

        await Clients.Group(roomId.Trim().ToUpperInvariant()).ReceiveMove(result.Fen!);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var roomId = roomManager.RemovePlayer(Context.ConnectionId);
        if (roomId is not null)
        {
            await Clients.Group(roomId).RoomDestroyed("Opponent disconnected. The room has been closed.");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
