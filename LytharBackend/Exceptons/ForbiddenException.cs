using System.Net;

namespace LytharBackend.Exceptons;

public class ForbiddenException : BaseHttpException
{
    public ForbiddenException(string message) : base("Forbidden", message, HttpStatusCode.Forbidden) { }
}
