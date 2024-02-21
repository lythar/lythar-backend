using System.Collections.Concurrent;

namespace LytharBackend.WebSocket;

public class WebSocketClientManager
{
    private readonly ConcurrentDictionary<Guid, WebSocketClient> Sockets = new();

    public Guid AddSocket(WebSocketClient socket)
    {
        var socketId = Guid.NewGuid();
        Sockets.TryAdd(socketId, socket);
        return socketId;
    }

    public WebSocketClient? GetSocket(Guid socketId)
    {
        Sockets.TryGetValue(socketId, out var socket);
        return socket;
    }

    public ICollection<WebSocketClient> GetAllSockets()
    {
        return Sockets.Values;
    }

    public async Task BroadcastFilter<T>(Func<WebSocketClient, bool> filter, T message)
    {
        foreach (var socket in Sockets.Values)
        {
            if (filter(socket))
            {
                await socket.Send(message);
            }
        }
    }

    public async Task Broadcast<T>(T message)
    {
        foreach (var socket in Sockets.Values)
        {
            await socket.Send(message);
        }
    }

    public WebSocketClient? FirstOrDefault(Func<WebSocketClient, bool> filter)
    {
        return Sockets.Values.FirstOrDefault(filter);
    }

    public void RemoveSocket(Guid socketId)
    {
        Sockets.TryRemove(socketId, out _);
    }
}