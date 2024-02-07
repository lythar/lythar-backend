using LytharBackend.Ldap;
using System.DirectoryServices.Protocols;

namespace LytharBackend.Models;

public class User
{
    public int Id { get; set; }
    public required string Login { get; set; }
    public required string Name { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }

    /// <summary>
    /// Any URL that fits in HTML img src attribute, so can be a base64 blob or an actual URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    public static User FromLdap(SearchResultEntry entry)
    {
        var uid = entry.Attributes["uid"].FirstOrDefault<string>() ?? entry.Attributes["cn"].First<string>();
        var givenName = entry.Attributes["givenName"].FirstOrDefault<string>() ?? uid;
        var sn = entry.Attributes["sn"].FirstOrDefault<string>();
        var email = entry.Attributes["mail"].FirstOrDefault<string>();

        return new User {
            Login = uid,
            Name = givenName,
            LastName = sn,
            Email = email
        };
    }
}
