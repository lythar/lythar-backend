using System.Net;

namespace LytharBackend.Exceptons;

public class FileSizeException : BaseHttpException
{
    public FileSizeException(long fileSize, long maxFileSize) : base("FileSizeExceeded", $"Plik jest za duży. Maksymalny rozmiar to {maxFileSize / 1000}KB, a ten plik ma {fileSize / 1000}KB.", HttpStatusCode.BadRequest) { }
}
