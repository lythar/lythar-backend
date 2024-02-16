using System.Net;

namespace LytharBackend.Exceptons;

public class AccountExistsException : BaseHttpException
{
    public AccountExistsException(string login) : base("AccountExists", $"Konto '{login}' już istnieje.", HttpStatusCode.BadRequest) { }
}
