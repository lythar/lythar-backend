using System.Net;

namespace LytharBackend.Exceptons;

public class UnauthorizedException : BaseHttpException
{
    public UnauthorizedException() : base("Unauthorized", "You are unauthorized to access this service.", HttpStatusCode.Unauthorized) { }
}
