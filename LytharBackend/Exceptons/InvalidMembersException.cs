using System.Net;

namespace LytharBackend.Exceptons;

public class InvalidMembersException : BaseHttpException
{
    public InvalidMembersException() : base ("InvalidMembers", "Nie można stworzyć grupy bez udziału udziału tego członka.", HttpStatusCode.BadRequest)
    {
    }
}
