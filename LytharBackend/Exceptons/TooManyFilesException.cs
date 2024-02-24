using System.Net;

namespace LytharBackend.Exceptons;

public class TooManyFilesException : BaseHttpException
{
    public TooManyFilesException(int maxFiles, int filesQuantity) : base("TooManyFiles", $"Wysłano {filesQuantity} plików, a można wysłać maksymalnie {maxFiles}.", HttpStatusCode.BadRequest) { }
}
