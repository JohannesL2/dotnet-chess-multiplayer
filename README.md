# Chess Multiplayer

[![Build](https://github.com/JohannesL2/dotnet-chess-multiplayer/actions/workflows/build.yml/badge.svg)](https://github.com/JohannesL2/dotnet-chess-multiplayer/actions/workflows/build.yml)

A real-time, two-player chess app built with Blazor WebAssembly, ASP.NET Core SignalR, and ChessDotNet. Create a private room, share its code, and play directly in the browser.

<img width="492" alt="Chess Multiplayer game board" src="https://github.com/user-attachments/assets/ec8372e3-60b4-4c5c-a616-6735d53e2bd8" />

## Highlights

- Real-time play over SignalR with automatic reconnection.
- Server-authoritative move validation and turn enforcement.
- Private, six-character room codes for two players.
- Responsive board that flips for the black player.
- Health endpoint and OpenAPI document in development.

## Architecture

```text
Browser (Blazor WebAssembly)
        │ MoveRequest
        ▼
ASP.NET Core / SignalR Hub ──► InMemoryGameRoomManager ──► ChessDotNet
        │                                                    │
        └──────────── validated FEN to both players ◄────────┘
```

The solution separates transport, game-room orchestration, and shared contracts:

```text
chessapp.client/   Blazor WebAssembly UI and SignalR client
chessapp.server/   ASP.NET Core host, SignalR hub, and server-side game state
chessapp.shared/   Contracts shared safely between client and server
```

The server owns each game's state. Clients submit coordinates such as `E2` → `E4`; they never submit a board position to be trusted by the server.

## Run locally

Prerequisite: [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet restore
dotnet run --project chessapp.server
dotnet run --project chessapp.client
```

Open the client URL displayed in the terminal (normally `http://localhost:5124`) in two separate browser sessions. Create a room in one and join it from the other using the room code.

During development, the server also exposes:

- `GET /health` — basic health check
- `/openapi/v1.json` — OpenAPI document

## Quality gates

```bash
dotnet format --verify-no-changes
dotnet build chessapp.slnx --configuration Release
```

GitHub Actions runs these checks for pushes and pull requests to `main`. Package versions are managed centrally in `Directory.Packages.props`, while shared build rules live in `Directory.Build.props`.

## VS Code and Codespaces

Open the repository folder in VS Code and accept the recommended extensions. The workspace supplies format-on-save, build/test tasks, a server debugger, and a **run app** task that starts both projects. The included dev container provides the pinned .NET 10 development environment for Codespaces or the VS Code Dev Containers extension.

## Scaling note

`InMemoryGameRoomManager` is intentionally scoped to one server instance. For multi-instance deployment, replace it with a durable/distributed room store and configure a SignalR backplane.
