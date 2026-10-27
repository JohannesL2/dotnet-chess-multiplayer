using ChessApp.Shared.Contracts;

namespace ChessApp.Server.Services;

public interface IGameRoomManager
{
    string CreateRoom(string connectionId);
    JoinRoomResult JoinRoom(string roomId, string connectionId);
    MoveResult MakeMove(string roomId, string connectionId, MoveRequest move);
    string? RemovePlayer(string connectionId);
}

public sealed record JoinRoomResult(bool Succeeded, string? Color = null, bool GameStarted = false, string? Error = null);
public sealed record MoveResult(bool Succeeded, string? Fen = null, string? Error = null);
