using System.Net;

namespace LytharBackend.Exceptons;

public class InvalidFileTypeException : BaseHttpException
{
    public InvalidFileTypeException(string contentType) : base("InvalidFileType", $"Plik jest rozszerzenia {contentType}, który nie jest dozwolony.", HttpStatusCode.BadRequest) { }
}