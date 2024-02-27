using LytharBackend.Exceptons;
using LytharBackend.Ldap;
using Microsoft.EntityFrameworkCore;
using System.DirectoryServices.Protocols;

namespace LytharBackend.Models;

public static class UserExtensions
{
    public static async Task<User> GetUserById(this DatabaseContext context, int id)
    {
        var user = await context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();

        if (user == null)
        {
            throw new AccountNotFoundException(id.ToString());
        }

        return user;
    }

    public static async Task<User> GetAdminById(this DatabaseContext context, int id)
    {
        var user = await GetUserById(context, id);

        if (!user.IsAdmin)
        {
            throw new ForbiddenException("Nie masz uprawnień do wykonania tej operacji.");
        }

        return user;
    }
}

public class User
{
    public int Id { get; set; }
    public required string Login { get; set; }
    public required string Name { get; set; }
    public bool IsAdmin { get; set; } = false;
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;

    public string? AvatarId { get; set; }
    public string? AvatarUrl { get; set; }

    public List<Channel> Channels { get; set; } = null!;


    public static User FromLdap(SearchResultAttributeCollection attr, bool isAdmin)
    {
        var cn = attr["cn"].FirstOrDefault<string>() ?? attr["uid"].First<string>();
        var givenName = attr["givenName"].FirstOrDefault<string>() ?? cn;
        var sn = attr["sn"].FirstOrDefault<string>();
        var email = attr["mail"].FirstOrDefault<string>();

        return new User {
            Login = cn,
            Name = givenName,
            LastName = sn,
            Email = email,
            IsAdmin = isAdmin
        };
    }
}
