using LytharBackend.Ldap;
using Microsoft.EntityFrameworkCore;
using System.DirectoryServices.Protocols;

namespace LytharBackend.Models;

public class User
{
    public int Id { get; set; }
    public required string Login { get; set; }
    public required string Name { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Any URL that fits in HTML img src attribute, so can be a base64 blob or an actual URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    public static User FromLdap(SearchResultAttributeCollection attr)
    {
        var cn = attr["cn"].FirstOrDefault<string>() ?? attr["uid"].First<string>();
        var givenName = attr["givenName"].FirstOrDefault<string>() ?? cn;
        var sn = attr["sn"].FirstOrDefault<string>();
        var email = attr["mail"].FirstOrDefault<string>();

        return new User {
            Login = cn,
            Name = givenName,
            LastName = sn,
            Email = email
        };
    }
}
