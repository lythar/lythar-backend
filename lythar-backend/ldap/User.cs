using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Net;
using static System.String;

namespace lythar_backend.ldap
{
    public partial class UserData
    {
        public string Username { get; set; }
        public string Description { get; set; }

        public string UserId { get; set; }
        public string UserPassword { get; set; }

        public UserData(string username, string description, string userId, string userPassword)
        {
            Username = username;
            Description = description;
            UserId = userId;
            UserPassword = userPassword;
        }
    }

    internal class User
    {
        public static void CreateUser(LdapConnection connection, UserData userData)
        {
            AddRequest newUser = new AddRequest(Format(@"cn={0},ou=Users,dc=mufaro,dc=com", userData.Username, userData.UserId));
                newUser.Attributes.Add(new DirectoryAttribute("objectclass", new object[] { "inetorgPerson" }));
                newUser.Attributes.Add(new DirectoryAttribute("givenName", userData.Username));
                newUser.Attributes.Add(new DirectoryAttribute("sn", "user"));
                //newUser.Attributes.Add(new DirectoryAttribute("userid", id.ToString()));
                newUser.Attributes.Add(new DirectoryAttribute("description", userData.Description));
                newUser.Attributes.Add(new DirectoryAttribute("userPassword", "password"));
            try
            {
                var response = connection.SendRequest(newUser);
                Console.WriteLine($"[LDAP] : Created User {userData.Username}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static bool Authenticate(LdapConnection connection, string username, string password)
        {
            string userDN = $"cn={username},ou=Users,dc=mufaro,dc=com";

            NetworkCredential credential = new NetworkCredential(userDN, password);

            try
            {
                connection.Bind(credential);
                Console.WriteLine($"[LDAP] : User {username} authenticated");
                return true;
            }
            catch (LdapException e)
            {
                Console.WriteLine($"[LDAP] : Authentication failed for user {username}. {e.Message}");
                return false;
            }
        }

        public static string FindOne(LdapConnection connection, string query) {
            var request = new SearchRequest($"dc=mufaro,dc=com", query, System.DirectoryServices.Protocols.SearchScope.Subtree, null);
            var response = (SearchResponse)connection.SendRequest(request);

            Console.WriteLine("[LDAP] : Searching for user");
            if (response.Entries.Count == 0)
            {
                Console.WriteLine("[LDAP] : User not found");
                return "";
            }

            var entry = response.Entries[0];
            var username = entry.Attributes["givenName"][0].ToString();

            Console.WriteLine($"[LDAP] : User {username} found");

            return username;
        }
    }
}
