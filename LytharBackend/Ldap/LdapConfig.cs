namespace LytharBackend.Ldap;

public class LdapConfig
{
    public const string Position = "Ldap";

    public string Host {  get; set; } = "localhost";
    public int Port { get; set; } = 389;
    public string AdminDn { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string SearchDn { get; set; } = "dc=lythar,dc=org";
    public string SearchFilter { get; set; } = "(cn={0})";
    public int Timeout { get; set; } = 10;
}
