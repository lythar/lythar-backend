using AntiLdapInjection;
using System.DirectoryServices.Protocols;
using System.Net;

namespace LytharBackend.Ldap;

// credit to github.com/Ender-0001 for being a skidder
public static class LdapUtils
{
    public static T First<T>(this DirectoryAttribute collection)
    {
        var values = collection.GetValues(typeof(string));
        return values.OfType<T>().First();
    }

    public static T? FirstOrDefault<T>(this DirectoryAttribute collection)
    {
        if (collection == null) return default(T?);

        var values = collection.GetValues(typeof(string));
        return values.OfType<T>().FirstOrDefault();
    }
}

public class LdapService
{
    private readonly ILogger Logger;
    private readonly IConfiguration Configuration;
    private readonly LdapConfig LdapConfig;

    private LdapConnection Connection;
    private LdapDirectoryIdentifier RootEntry;

    public string AdminGroup => LdapConfig.AdminGroup;
    public string SearchDn => LdapConfig.SearchDn;

    public LdapService(ILogger<LdapService> logger, IConfiguration configuration)
    {
        Logger = logger;
        Configuration = configuration;
        LdapConfig = new LdapConfig();

        Configuration.GetSection("Ldap").Bind(LdapConfig);

        RootEntry = new LdapDirectoryIdentifier(LdapConfig.Host, LdapConfig.Port);
        Connection = new LdapConnection(RootEntry);

        Connection.AuthType = AuthType.Basic;
        Connection.SessionOptions.ProtocolVersion = 3;
        Connection.Timeout = TimeSpan.FromSeconds(LdapConfig.Timeout);

        var networkCredential = new NetworkCredential(LdapConfig.AdminDn, LdapConfig.AdminPassword);

        Connection.Bind(networkCredential);

        Logger.LogInformation("Connected to LDAP!");
    }

    private bool ValidateDn(string dn, string password)
    {
        try
        {
            using var connection = new LdapConnection(RootEntry);

            connection.AuthType = AuthType.Basic;
            connection.SessionOptions.ProtocolVersion = 3;

            var networkCredential = new NetworkCredential(dn, password);

            connection.Bind(networkCredential);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public SearchResponse? Find(string ldapFilter)
    {
        var request = new SearchRequest(
            LdapConfig.SearchDn,
            ldapFilter,
            SearchScope.Subtree,
            null
        );

        return (SearchResponse)Connection.SendRequest(request);
    }

    public SearchResultEntry? ValidateLogin(string login, string password)
    {
        var response = Find(string.Format(LdapConfig.SearchFilter, LdapEncoder.FilterEncode(login)));

        if (response == null || response.Entries.Count == 0) return null;

        var dn = response.Entries[0].DistinguishedName;

        if (ValidateDn(dn, password)) return response.Entries[0];
        else return null;
    }
}
