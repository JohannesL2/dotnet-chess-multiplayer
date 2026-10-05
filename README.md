# ♟️ Chess Multiplayer (.NET Frontend & Backend)

A full-stack, real-time multiplayer chess application built with **Blazor**, **ASP.NET Core SignalR**, and **ChessDotNet**.

This repository contains a decoupled solution architecture featuring a dedicated backend Web API/SignalR Hub and a WebAssembly/Blazor client frontend.

## Architecture & Flow
<img width="492" height="242" alt="flow" src="https://github.com/user-attachments/assets/e61aba3e-994e-4538-93ac-d1d733cdc6de" />

---

## Features

- **Real-Time Synchronization:** Seamless, zero-latency move updates between players powered by ASP.NET Core SignalR WebSockets.
- **Matchmaking & Room System:** Create private rooms with randomly generated codes or join active games instantly.
- **Move & Rule Enforcement:** Built-in chess engine logic via `ChessDotNet` validating legal moves, turn ordering, and board states.
- **Responsive UI:** Clean visual design with dynamic board flipping (perspective changes depending on assigned piece color: White/Black) and real-time status indicators.
- **Session Resilience:** Graceful handling of player disconnections, state cleanups, and game resetting.

---

## Tech Stack

- **Frontend:** Blazor (C# / HTML5 / CSS3)
- **Backend:** ASP.NET Core SignalR Hub, Web API
- **Domain Logic:** `ChessDotNet`
- **Communication:** SignalR (WebSockets / Long Polling fallback)

---

## Project Structure

```text
├── ChessApp.Server/    # ASP.NET Core SignalR Hub & backend logic
└── ChessApp.Client/    # Blazor frontend component UI
```

---

## How to use

Start server
```bash
dotnet run --project ChessApp.Server
```
Start client
```bash
dotnet run --project ChessApp.Client
```
Open two separate browser tabs (or incognito windows) at http://localhost:5124 to create a room, copy the room code, and test the real-time chess multiplayer.
