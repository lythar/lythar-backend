using System.Net;

namespace LytharBackend.Exceptons;

public class InvalidFileTypeException : BaseHttpException
{
    public InvalidFileTypeException(string contentType) : base("InvalidFileType", $"Pliki rozszerzenia {contentType} nie są dozwolone.", HttpStatusCode.BadRequest) { }
}