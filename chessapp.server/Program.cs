using ChessApp.Server.Hubs;
using ChessApp.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IGameRoomManager, InMemoryGameRoomManager>();

// Allow CORS for the Blazor app and the server
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7196", "http://localhost:5124")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapHub<ChessHub>("/chessHub");

app.Run();
