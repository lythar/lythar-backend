using System.Net;

namespace LytharBackend.Exceptons;

public class AccountNotFoundException : BaseHttpException
{
    public AccountNotFoundException(string accountId) : base("AccountNotFound", $"Account '{accountId}' doesn't exist.", HttpStatusCode.NotFound) { }
}
