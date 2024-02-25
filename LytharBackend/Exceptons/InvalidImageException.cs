using System.Net;

namespace LytharBackend.Exceptons;

public class InvalidImageException : BaseHttpException
{
    public InvalidImageException() : base("InvalidImage", "Podany obraz jest nieprawidłowy.", HttpStatusCode.BadRequest) { }
}
