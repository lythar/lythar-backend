using System.Net;

namespace LytharBackend.Exceptons;

public class InvalidLoginException : BaseHttpException
{
    public InvalidLoginException() : base("InvalidLogin", "Podany login jest niepoprawny.", HttpStatusCode.Unauthorized) { }
}
