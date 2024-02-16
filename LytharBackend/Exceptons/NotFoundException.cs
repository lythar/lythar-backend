using System.Net;

namespace LytharBackend.Exceptons;

public class NotFoundException : BaseHttpException
{
    public NotFoundException(string path) : base("NotFoundException", $"Endpoint '{path}' not found.", HttpStatusCode.NotFound) { }
}