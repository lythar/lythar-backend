namespace LytharBackend.Files;

public interface IFileService
{
    // Uploads a file to the file storage and returns the ID of the file.
    Task<string> UploadFile(Stream fileStream, string fileNamespace, string fileName);
    Task DeleteFile(string fileNamespace, string fileName);
    Task<string> GetFileUrl(string fileNamespace, string fileName);
}
