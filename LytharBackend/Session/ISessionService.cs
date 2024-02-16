using LytharBackend.Exceptons;

namespace LytharBackend.Session;

public class SessionData
{
    public required int AccountId;
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

public static class SessionMethods
{
    public static async Task<SessionData> VerifyRequest(this ISessionService service, HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()
                ?? context.Request.Cookies["token"];

        if (token == null)
        {
            throw new UnauthorizedException();
        }

        return await service.VerifySession(token);
    }
}