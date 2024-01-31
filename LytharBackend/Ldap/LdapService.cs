namespace LytharBackend.Ldap;

public class LdapService
{
    private readonly ILogger Logger;

    public LdapService(ILogger<LdapService> logger)
    {
        Logger = logger;
    }

    public void Test()
    {
        Logger.LogInformation("Haloo");
    }
}
