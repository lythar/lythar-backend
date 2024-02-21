namespace LytharBackend.WebSocket;

using LytharBackend.Session;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class WebSocketClient
{
    private HttpContext HttpContext;
    private WebSocket Socket;
    private readonly SessionData Session;
    private Guid SessionId;
    public static WebSocketClientManager Manager { get; } = new();

    public WebSocketClient(HttpContext httpContext, WebSocket socket, SessionData sessionData)
    {
        HttpContext = httpContext;
        Socket = socket;
        Session = sessionData;
    }

    public async Task Listen()
    {
        SessionId = Manager.AddSocket(this);

        var buffer = new byte[1024 * 4];
        var receiveResult = await Socket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None
        );

        while (!receiveResult.CloseStatus.HasValue)
        {
            await OnMessage(buffer, receiveResult);

            receiveResult = await Socket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None
            );
        }

        Manager.RemoveSocket(SessionId);

        await Socket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None
        );
    }

    public async Task Send<T>(T message)
    {
        var serialized = JsonSerializer.Serialize(message);

        await Socket.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(serialized)),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    private async Task OnMessage(byte[] buffer, WebSocketReceiveResult receiveResult)
    {
        await Send(new { Type = "pong" });
    }
}