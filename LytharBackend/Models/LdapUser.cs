using LinqToLdap.Mapping;

namespace LytharBackend.Models;

// TO-DO: WIP LinqToLdap class, figure out how to resolve any DC or make an env variable of it
[DirectorySchema(NamingContext, ObjectCategory = "Person", ObjectClass = "User")]
public class LdapUser
{
    public const string NamingContext = "OU=Users";
}
