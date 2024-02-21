namespace LytharBackend.WebSocket;

public class WebSocketMessage<T>
{
    public required string Type { get; set; }
    public required T Data { get; set; }
}