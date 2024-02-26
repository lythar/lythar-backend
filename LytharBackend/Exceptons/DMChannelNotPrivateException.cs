using System.Net;

namespace LytharBackend.Exceptons;

public class DMChannelNotPrivateException : BaseHttpException
{
    public DMChannelNotPrivateException() : base("DMChannelNotPrivate", "Nie można stworzyć publicznego kanału wiadomości bezpośrednich.", HttpStatusCode.BadRequest)
    {
    }
}
