using System.Net;

namespace LytharBackend.Exceptons;

public class InvalidLoginException : BaseHttpException
{
    public InvalidLoginException() : base("InvalidLogin", "Provided login is invalid.", HttpStatusCode.Unauthorized) { }
}
