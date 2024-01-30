using System.Net;
using System.DirectoryServices.Protocols;
using static System.String;

namespace lythar_backend.ldap
{
    public class LdapConnect
    {

        public LdapConnection ldapConnection;
        public LdapDirectoryIdentifier RootEntry = new LdapDirectoryIdentifier("192.168.1.59", 389);

        public LdapConnect()
        {
            try
            {
                ldapConnection = new LdapConnection(RootEntry);
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

        public void CloseConnection()
        {
            ldapConnection.Dispose();
        }
    }

}
