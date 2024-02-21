namespace LytharBackend.WebSocket;

public class WebSocketMessage<T>
{
    public required string Type { get; set; }
    public required T Data { get; set; }
}

public class UserStatusMessage
{
    public int AccountId { get; set; }
    public UserStatus Status { get; set; }

    public UserStatusMessage(int accountId, UserStatus status)
    {
        AccountId = accountId;
        Status = status;
    }
}

public class UserStatus
{
    public bool IsOnline { get; set; } = true;
}