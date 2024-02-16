using System.Net;

namespace LytharBackend.Exceptons;

public class UnauthorizedException : BaseHttpException
{
    public UnauthorizedException() : base("Unauthorized", "Nie masz dostępu do tej usługi.", HttpStatusCode.Unauthorized) { }
}
