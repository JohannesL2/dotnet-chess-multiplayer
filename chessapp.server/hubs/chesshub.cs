using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChessApp.Server.Hubs;

public class ChessHub : Hub
{
    private static readonly ConcurrentDictionary<string, GameRoom> Rooms = new();

    public class GameRoom
    {
        public string RoomId { get; set; } = "";
        public string? WhiteConnectionId { get; set; }
        public string? BlackConnectionId { get; set; }
        public string CurrentTurn { get; set; } = "White";
    }
    public async Task<string> CreateRoom()
    {
        string roomId = Guid.NewGuid().ToString()[..6].ToUpper();
        var room = new GameRoom
        {
            RoomId = roomId,
            WhiteConnectionId = Context.ConnectionId
        };

        Rooms[roomId] = room;
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Caller.SendAsync("AssignColor", "White");

        return roomId;
    }
    public async Task JoinRoom(string roomId)
    {
        roomId = roomId.Trim().ToUpper();

        if (!Rooms.TryGetValue(roomId, out var room))
        {
            await Clients.Caller.SendAsync("RoomError", "Room not found!");
            return;
        }

        lock (room)
        {
            if (room.WhiteConnectionId == Context.ConnectionId)
            {
                Clients.Caller.SendAsync("AssignColor", "White");
                return;
            }
            if (room.BlackConnectionId == Context.ConnectionId)
            {
                Clients.Caller.SendAsync("AssignColor", "Black");
                return;
            }

            if (room.BlackConnectionId == null)
            {
                room.BlackConnectionId = Context.ConnectionId;
                Groups.AddToGroupAsync(Context.ConnectionId, roomId);

                Clients.Caller.SendAsync("AssignColor", "Black");

                Clients.Group(roomId).SendAsync("GameStarted");
            }
            else
            {
                Clients.Caller.SendAsync("RoomError", "Room is full! Only 2 players allowed.");
            }
        }
    }

    public async Task SendMove(string roomId, string moveFen)
    {
        roomId = roomId.Trim().ToUpper();

        if (!Rooms.TryGetValue(roomId, out var room)) return;

        if (string.IsNullOrEmpty(room.WhiteConnectionId) || string.IsNullOrEmpty(room.BlackConnectionId))
        {
            await Clients.Caller.SendAsync("MoveError", "Both players must be connected before a move can be made.");
            return;
        }

        bool isWhite = room.WhiteConnectionId == Context.ConnectionId;
        bool isBlack = room.BlackConnectionId == Context.ConnectionId;

        if (!isWhite && !isBlack)
        {
            await Clients.Caller.SendAsync("MoveError", "You are not participating in this room.");
            return;
        }

        if ((isWhite && room.CurrentTurn != "White") || (isBlack && room.CurrentTurn != "Black"))
        {
            await Clients.Caller.SendAsync("MoveError", "It's not your turn!");
            return;
        }

        room.CurrentTurn = (room.CurrentTurn == "White") ? "Black" : "White";
        await Clients.OthersInGroup(roomId).SendAsync("ReceiveMove", moveFen);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var entry in Rooms)
        {
            var room = entry.Value;
            if (room.WhiteConnectionId == Context.ConnectionId || room.BlackConnectionId == Context.ConnectionId)
            {
                await Clients.Group(entry.Key).SendAsync("RoomDestroyed", "Opponent disconnected. The room has been closed.");
                Rooms.TryRemove(entry.Key, out _);
                break;
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}