namespace LytharBackend.Exceptons;

public class InternalServerException : BaseHttpException
{
    public InternalServerException() : base("InternalServerException", "Błąd serwera.", System.Net.HttpStatusCode.InternalServerError) { }
}
