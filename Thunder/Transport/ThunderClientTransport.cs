using System.Net.WebSockets;
using System.Text.Json;
using Thunder.Client.Interfaces;
using Thunder.Contracts.Requests;

namespace Thunder.Client.Transport;

internal sealed class ThunderClientTransport(Uri endpoint) : ITransport
{
    public async Task<bool> RequestAsync(RequestEnvelope request)
    {
        using var socket = new ClientWebSocket();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var token = timeout.Token;

        await socket.ConnectAsync(endpoint, token);

        var json = JsonSerializer.SerializeToUtf8Bytes(request);
        await socket.SendAsync(json.AsMemory(), WebSocketMessageType.Text, true, token);

        var buffer = new byte[4097];
        var length = 0;

        while (true)
        {
            var part = await socket.ReceiveAsync(buffer.AsMemory(length), token);

            if (part.MessageType == WebSocketMessageType.Close)
            {
                var status = socket.CloseStatus;
                await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, token);
                throw new WebSocketException($"Сервер закрыл соединение: {status}.");
            }

            if (part.MessageType != WebSocketMessageType.Text)
                throw new InvalidDataException("Ожидался текстовый ответ сервера.");

            length += part.Count;
            if (length > 4096)
                throw new InvalidDataException("Ответ сервера превышает 4 КиБ.");

            if (part.EndOfMessage)
                break;
        }

        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, token);

        using var reply = JsonDocument.Parse(buffer.AsMemory(0, length));
        return reply.RootElement.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Object when reply.RootElement.TryGetProperty("Error", out var error)
                && error.ValueKind == JsonValueKind.String
                => throw new InvalidDataException($"Ошибка сервера: {error.GetString()}."),
            _ => throw new InvalidDataException("Сервер вернул ошибку вместо true или false.")
        };
    }
}
