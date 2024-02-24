using System.Net;

namespace LytharBackend.Exceptons;

public class FileSizeException : BaseHttpException
{
    public FileSizeException(long fileSize, long maxFileSize) : base("FileSizeExceeded", $"Plik jest za duży. Maksymalny rozmiar to {maxFileSize}B, a ten plik ma {fileSize}B.", HttpStatusCode.BadRequest) { }
}
