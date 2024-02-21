using System.Net;

namespace LytharBackend.Exceptons;

public class MessageNotFoundException : BaseHttpException
{
    public MessageNotFoundException(string messageId) : base("MessageNotFound", $"Wiadomośc '{messageId}' nie istnieje.", HttpStatusCode.NotFound) { }
}
