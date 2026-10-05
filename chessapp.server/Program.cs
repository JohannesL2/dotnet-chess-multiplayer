using ChessApp.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Register signalR services
builder.Services.AddSignalR();

// Allow CORS for the Blazor app and the server
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:5180", "http://localhost:5124") // Server post and Blazor-app port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();

// connect to the hub from the client
app.MapHub<ChessHub>("/chessHub");

app.Run();
