using System.Net;
using System.DirectoryServices.Protocols;

namespace Program
{
    public class LdapConnect
    {

        LdapConnection ldapConnection;

        public LdapConnect()
        {
            try
            {
                LdapDirectoryIdentifier ldi = new LdapDirectoryIdentifier("192.168.1.59", 389);
                ldapConnection = new LdapConnection(ldi);
                Console.WriteLine("[LDAP] : Created Connetion");
                ldapConnection.AuthType = AuthType.Basic;
                ldapConnection.SessionOptions.ProtocolVersion = 3;

                NetworkCredential nc = new NetworkCredential("cn=admin,dc=mufaro,dc=com",
                    "admin_pass"); //password

                ldapConnection.Bind(nc);
                Console.WriteLine("[LDAP] : Authenticated successfully");
            }
            catch (LdapException e)
            {
                Console.WriteLine("\r\nUnable to login:\r\n\t" + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("\r\nUnexpected exception occured:\r\n\t" + e.GetType() + ":" + e.Message);
            }
        }

        public void CreateUser(string username)
        {
            int id = 1;

            AddRequest addme = new AddRequest(String.Format(@"cn={0}, ou=Users,dc=mufaro,dc=com", username));
            addme.Attributes.Add(new DirectoryAttribute("objectclass", new object[] { "inetorgPerson" }));
            addme.Attributes.Add(new DirectoryAttribute("givenName", "new"));
            addme.Attributes.Add(new DirectoryAttribute("sn", "user"));
            addme.Attributes.Add(new DirectoryAttribute("userid", id.ToString()));
            addme.Attributes.Add(new DirectoryAttribute("description", "A test user"));
            addme.Attributes.Add(new DirectoryAttribute("userPassword", "password"));
            try
            {
                var response = ldapConnection.SendRequest(addme);
                Console.WriteLine($"[LDAP] Created User {username}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            LdapConnect ldap = new LdapConnect();
            ldap.CreateUser("TestUser");
        }
    }
}
