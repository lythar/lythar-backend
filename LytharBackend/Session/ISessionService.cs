namespace LytharBackend.Session;

public class SessionData
{
    public required string AccountId;
    public required string Username;
    public required string SessionId;
    public required DateTime CreatedAt;
    public required DateTime ExpiresAt;
}

public class CreateSessionOptions
{
    public required string AccountId;
    public required string Username;
}

public interface ISessionService
{
    Task<string> CreateSession(CreateSessionOptions options);
    Task<SessionData> VerifySession(string token);
}
