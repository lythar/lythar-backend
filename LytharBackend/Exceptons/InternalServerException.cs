namespace LytharBackend.Exceptons;

public class InternalServerException : BaseHttpException
{
    public InternalServerException() : base("InternalServerException", "Internal server error.", System.Net.HttpStatusCode.InternalServerError) { }
}
