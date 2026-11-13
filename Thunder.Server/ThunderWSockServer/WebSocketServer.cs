using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Thunder.Contracts.Requests;
using Thunder.Server.Storage;

namespace Thunder.Server.ThunderWSockServer;

internal sealed class WebSocketServer(
    ILogger<WebSocketServer> logger, IHostApplicationLifetime lifetime, ServerStorage storage)
{
    private const int MaxMessageBytes = 16 * 1024;

    public async Task HandleAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            context.RequestAborted, lifetime.ApplicationStopping);
        var cancellationToken = cancellation.Token;

        try
        {
            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            await ReceiveMessagesAsync(socket, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The client disconnected or the application is stopping.
        }
        catch (WebSocketException exception)
        {
            logger.LogDebug(exception, "WebSocket connection ended.");
        }
    }

    private async Task ReceiveMessagesAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        // The extra byte detects overflow even when the last fragment is empty.
        var buffer = new byte[MaxMessageBytes + 1];
        var messageLength = 0;

        while (socket.State == WebSocketState.Open)
        {
            var received = await socket.ReceiveAsync(buffer.AsMemory(messageLength), cancellationToken);

            if (received.MessageType == WebSocketMessageType.Close)
            {
                await CloseAsync(socket, socket.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                    socket.CloseStatusDescription, cancellationToken);
                return;
            }

            if (received.MessageType != WebSocketMessageType.Text)
            {
                await CloseAsync(socket, WebSocketCloseStatus.InvalidMessageType,
                    "Only text messages are supported.", cancellationToken);
                return;
            }

            messageLength += received.Count;
            if (messageLength > MaxMessageBytes)
            {
                await CloseAsync(socket, WebSocketCloseStatus.MessageTooBig,
                    "Message exceeds 16 KiB.", cancellationToken);
                return;
            }

            if (!received.EndOfMessage)
            {
                continue;
            }

            var response = JsonSerializer.SerializeToUtf8Bytes(HandleMessage(buffer.AsSpan(0, messageLength)));
            await socket.SendAsync(response.AsMemory(), WebSocketMessageType.Text, true, cancellationToken);
            messageLength = 0;
        }
    }

    private object HandleMessage(ReadOnlySpan<byte> message)
    {
        try
        {
            var request = JsonSerializer.Deserialize<RequestEnvelope>(message);

            return request?.Type switch
            {
                RequestType.IsFreeLogin => storage.IsFreeLogin(
                    request.Payload.ValueKind == JsonValueKind.String
                        ? request.Payload.Deserialize<string>()
                        : request.Payload.Deserialize<IsFreeLoginRequest>()?.Login),
                RequestType.Login => request.Payload.Deserialize<LoginRequest>() is { } login
                    && storage.CheckPassword(login.Login, login.Password),
                RequestType.SignUp => request.Payload.Deserialize<SignupRequest>() is { } signup
                    && storage.SignUp(signup.Login, signup.Password),
                _ => false
            };
        }
        catch (JsonException)
        {
            return new { Ok = false, Error = "invalid_request" };
        }
        catch (SqliteException exception)
        {
            logger.LogError(exception, "Failed to execute a user storage operation.");
            return new { Ok = false, Error = "storage_error" };
        }
    }

    private static async Task CloseAsync(WebSocket socket, WebSocketCloseStatus status,
        string? description, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(2));

        try
        {
            await socket.CloseAsync(status, description, timeout.Token);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            // The socket is disposed by the request handler.
        }
    }
}
