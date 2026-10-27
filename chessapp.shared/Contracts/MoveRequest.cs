namespace ChessApp.Shared.Contracts;

/// <summary>Represents a move in long algebraic notation, for example E2 to E4.</summary>
public sealed record MoveRequest(string From, string To);
