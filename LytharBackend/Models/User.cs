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

    public string? AvatarId { get; set; }
    public string? AvatarUrl { get; set; }

    public List<Channel> Channels { get; set; } = null!;


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
