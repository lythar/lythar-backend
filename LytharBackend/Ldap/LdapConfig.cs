namespace LytharBackend.Ldap;

public class LdapConfig
{
    public const string Position = "Ldap";

    public string Host {  get; set; } = "localhost";
    public int Port { get; set; } = 389;
    public string AdminDn { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminGroup { get; set; } = "ou=admin";
    public string SearchDn { get; set; } = "dc=lythar,dc=org";
    public string SearchFilter { get; set; } = "(uid={0})";
    public bool LoginSync { get; set; } = false;
    public int Timeout { get; set; } = 10;
}
