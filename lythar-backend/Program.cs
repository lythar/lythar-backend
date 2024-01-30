using lythar_backend.ldap;

namespace lythar_backend
{
    class Program
    {
        static void Main(string[] args)
        {
            LdapConnect ldap = new LdapConnect();

            string username = "mufaro";
            string password = "mufaro";
            bool isCorrect = User.Authenticate(ldap.ldapConnection, username, password);

            Console.WriteLine(isCorrect);

            ldap.CloseConnection();
        }
    }
}
