namespace LytharBackend.WebSocket;

using LytharBackend.Session;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class WebSocketClient
{
    public static WebSocketClientManager Manager { get; } = new();

    public UserStatus Status { get; set; } = new();
    private readonly SessionData Session;
    private HttpContext HttpContext;
    private WebSocket Socket;
    private Guid SessionId;

    public WebSocketClient(HttpContext httpContext, WebSocket socket, SessionData sessionData)
    {
        HttpContext = httpContext;
        Socket = socket;
        Session = sessionData;
    }

    public async Task Listen()
    {
        SessionId = Manager.AddSocket(this);

        await ReceiveStatus();
        await BroadcastStatus();

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

        Status.IsOnline = false;

        await BroadcastStatus();

        Manager.RemoveSocket(SessionId);

        await Socket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None
        );
    }

    public async Task Send<T>(T message)
    {
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var serialized = JsonSerializer.Serialize(message, serializeOptions);

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

    private async Task BroadcastStatus()
    {
        await Manager.BroadcastFilter(
            x => x.Session.AccountId != Session.AccountId,
            new {
                Type = "UserStatus",
                Data = new UserStatusMessage(Session.AccountId, Status)
            }
        );
    }

    private async Task ReceiveStatus()
    {
        List<UserStatusMessage> userStatuses = new();

        foreach (var socket in Manager.GetAllSockets())
        {
            userStatuses.Add(new UserStatusMessage(socket.Session.AccountId, socket.Status));
        }

        await Send(new
        {
            Type = "UserStatusBulk",
            Data = userStatuses
        });
    }
}