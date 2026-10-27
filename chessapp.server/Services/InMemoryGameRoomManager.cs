using System.Collections.Concurrent;
using ChessApp.Contracts;
using ChessDotNet;

namespace ChessApp.Server.Services;

/// <summary>Owns authoritative state for active rooms. Replace it with distributed storage when scaling out.</summary>
public sealed class InMemoryGameRoomManager : IGameRoomManager
{
    private const int RoomCodeLength = 6;
    private const string RoomCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private readonly ConcurrentDictionary<string, GameRoom> rooms = new(StringComparer.Ordinal);

    public string CreateRoom(string connectionId)
    {
        string roomId;
        do
        {
            roomId = string.Create(RoomCodeLength, 0, static (buffer, _) =>
            {
                for (var index = 0; index < buffer.Length; index++)
                {
                    buffer[index] = RoomCodeAlphabet[Random.Shared.Next(RoomCodeAlphabet.Length)];
                }
            });
        }
        while (!rooms.TryAdd(roomId, new GameRoom(connectionId)));

        return roomId;
    }

    public JoinRoomResult JoinRoom(string roomId, string connectionId)
    {
        if (!rooms.TryGetValue(NormalizeRoomId(roomId), out var room))
        {
            return new(false, Error: "Room not found.");
        }

        lock (room.Gate)
        {
            if (room.WhiteConnectionId == connectionId)
            {
                return new(true, "White");
            }

            if (room.BlackConnectionId == connectionId)
            {
                return new(true, "Black");
            }

            if (room.BlackConnectionId is not null)
            {
                return new(false, Error: "Room is full. Only two players are allowed.");
            }

            room.BlackConnectionId = connectionId;
            return new(true, "Black", GameStarted: true);
        }
    }

    public MoveResult MakeMove(string roomId, string connectionId, MoveRequest move)
    {
        if (!rooms.TryGetValue(NormalizeRoomId(roomId), out var room))
        {
            return new(false, Error: "Room not found.");
        }

        lock (room.Gate)
        {
            if (room.BlackConnectionId is null)
            {
                return new(false, Error: "Both players must be connected before a move can be made.");
            }

            var player = room.WhiteConnectionId == connectionId ? Player.White
                : room.BlackConnectionId == connectionId ? Player.Black
                : (Player?)null;
            if (player is null)
            {
                return new(false, Error: "You are not participating in this room.");
            }

            if (room.Game.WhoseTurn != player)
            {
                return new(false, Error: "It is not your turn.");
            }

            if (!IsValidCoordinate(move?.From) || !IsValidCoordinate(move.To))
            {
                return new(false, Error: "Move coordinates must use chess notation, for example E2 and E4.");
            }

            var chessMove = new Move(move.From.ToUpperInvariant(), move.To.ToUpperInvariant(), player.Value);
            if (!room.Game.IsValidMove(chessMove))
            {
                return new(false, Error: "That is not a legal move.");
            }

            room.Game.MakeMove(chessMove, true);
            return new(true, room.Game.GetFen());
        }
    }

    public string? RemovePlayer(string connectionId)
    {
        foreach (var (roomId, room) in rooms)
        {
            lock (room.Gate)
            {
                if (room.WhiteConnectionId != connectionId && room.BlackConnectionId != connectionId)
                {
                    continue;
                }

                ((ICollection<KeyValuePair<string, GameRoom>>)rooms).Remove(new(roomId, room));
                return roomId;
            }
        }
        return null;
    }

    private static string NormalizeRoomId(string roomId) => roomId.Trim().ToUpperInvariant();

    private static bool IsValidCoordinate(string? coordinate) => coordinate is [var file, var rank]
        && (file is >= 'a' and <= 'h' or >= 'A' and <= 'H')
        && rank is >= '1' and <= '8';

    private sealed class GameRoom(string whiteConnectionId)
    {
        public object Gate { get; } = new();
        public ChessGame Game { get; } = new();
        public string WhiteConnectionId { get; } = whiteConnectionId;
        public string? BlackConnectionId { get; set; }
    }
}
