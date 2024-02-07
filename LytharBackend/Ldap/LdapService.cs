using AntiLdapInjection;
using System.DirectoryServices.Protocols;
using System.Net;

namespace LytharBackend.Ldap;

public class LdapService
{
    private readonly ILogger Logger;
    private readonly IConfiguration Configuration;
    private readonly LdapConfig LdapConfig;

    private LdapConnection Connection;
    private LdapDirectoryIdentifier RootEntry;

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

    public bool ValidateLogin(string login, string password)
    {
        var request = new SearchRequest(
            LdapConfig.SearchDn,
            string.Format(LdapConfig.SearchFilter, LdapEncoder.FilterEncode(login)),
            System.DirectoryServices.Protocols.SearchScope.Subtree,
            null
        );
        var response = (SearchResponse)Connection.SendRequest(request);

        if (response.Entries.Count == 0) return false;

        return ValidateDn(response.Entries[0].DistinguishedName, password);
    }
}
