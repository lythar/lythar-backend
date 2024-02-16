using System.Net;

namespace LytharBackend.Exceptons;

public class AccountNotFoundException : BaseHttpException
{
    public AccountNotFoundException(string accountId) : base("AccountNotFound", $"Konto '{accountId}' nie istnieje.", HttpStatusCode.NotFound) { }
}
