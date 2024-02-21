using System.Text.Json.Serialization;

namespace LytharBackend.WebSocket;

public class WebSocketMessage<T>
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    [JsonPropertyName("data")]
    public required T Data { get; set; }
}