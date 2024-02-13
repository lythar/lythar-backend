using System.Net;

namespace LytharBackend.Exceptons;

public class AccountExistsException : BaseHttpException
{
    public AccountExistsException(string login) : base("AccountExists", $"Account '{login}' already exists.", HttpStatusCode.BadRequest) { }
}
