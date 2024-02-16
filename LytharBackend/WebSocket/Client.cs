namespace LytharBackend.WebSocket;

using LytharBackend.Session;
using System.Net.WebSockets;

public class WebSocketClient
{
    private HttpContext HttpContext;
    private WebSocket Socket;
    private readonly SessionData Session;

    public WebSocketClient(HttpContext httpContext, WebSocket socket, SessionData sessionData)
    {
        HttpContext = httpContext;
        Socket = socket;
        Session = sessionData;
    }

    public async Task Listen()
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await Socket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None
        );

        while (!receiveResult.CloseStatus.HasValue)
        {
            await Socket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None
            );

            receiveResult = await Socket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None
            );
        }

        await Socket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None
        );
    }
}