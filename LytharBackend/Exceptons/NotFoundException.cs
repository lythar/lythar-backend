using System.Net;

namespace LytharBackend.Exceptons;

public class NotFoundException : BaseHttpException
{
    public NotFoundException(string path) : base("NotFoundException", $"Nie znaleziono ścieźki '{path}'.", HttpStatusCode.NotFound) { }
}