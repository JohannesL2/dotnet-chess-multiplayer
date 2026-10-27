namespace ChessApp.Server.Hubs;

public interface IChessClient
{
    Task AssignColor(string color);
    Task GameStarted();
    Task ReceiveMove(string fen);
    Task RoomError(string message);
    Task MoveError(string message);
    Task RoomDestroyed(string message);
}
