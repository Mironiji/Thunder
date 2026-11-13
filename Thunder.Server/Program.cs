using Thunder.Server.Storage;
using Thunder.Server.ThunderWSockServer;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ServerStorage>();
builder.Services.AddSingleton<WebSocketServer>();

var app = builder.Build();
app.Services.GetRequiredService<ServerStorage>();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
    KeepAliveTimeout = TimeSpan.FromSeconds(15)
});

app.MapGet("/", () => Results.Ok(new { status = "ok" }));
app.Map("/ws", (HttpContext context, WebSocketServer server) => server.HandleAsync(context));

app.Run();
