namespace LytharBackend.Files;

public class LocalFileService : IFileService
{
    private readonly IConfiguration Configuration;
    private readonly string StoragePath;

    public LocalFileService(IConfiguration configuration)
    {
        Configuration = configuration;

        string? storagePath = Configuration["LocalFileService:RootPath"];

        if (storagePath == null)
        {
            throw new Exception("'LocalFileService:RootPath' not found in configuration.");
        }

        StoragePath = storagePath;
    }

    private string GetFilePath(string fileNamespace, string fileName)
    {
        return Path.Combine(StoragePath, Path.GetFileName(fileNamespace), Path.GetFileName(fileName));
    }

    public async Task<string> UploadFile(Stream fileStream, string fileNamespace, string fileName)
    {
        string filePath = GetFilePath(fileNamespace, fileName);

        Directory.CreateDirectory(filePath);

        using var file = File.Create(filePath);
        
        await fileStream.CopyToAsync(file);

        return fileName;
    }

    public Task DeleteFile(string fileNamespace, string fileName)
    {
        string filePath = GetFilePath(fileNamespace, fileName);

        File.Delete(filePath);

        return Task.CompletedTask;
    }

    public Task<string> GetFileUrl(string fileNamespace, string fileName)
    {
        return Task.FromResult($"/uploaded/{fileNamespace}/{fileName}");
    }
}
