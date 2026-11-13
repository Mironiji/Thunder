using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Thunder.Client.CLI;
using Thunder.Client.Interfaces;
using Thunder.Client.Service;
using Thunder.Client.Transport;

var builder = Host.CreateApplicationBuilder(args);

var serverUrl = builder.Configuration["WebSocket:Url"] ?? "ws://localhost:5037/ws";
builder.Services.AddSingleton<ITransport>(_ => new ThunderClientTransport(new Uri(serverUrl)));
builder.Services.AddSingleton<ThunderClientService>();
builder.Services.AddSingleton<ThunderClientCLI>();

using var host = builder.Build();

var cli = host.Services.GetRequiredService<ThunderClientCLI>();

await cli.RunAsync();
