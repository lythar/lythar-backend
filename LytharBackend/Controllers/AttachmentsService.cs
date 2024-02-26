using LytharBackend.Exceptons;
using LytharBackend.Files;
using LytharBackend.Models;
using LytharBackend.Session;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace LytharBackend.Controllers;

[Route("attachments/api")]
public class AttachmentsController : Controller
{
    private ISessionService SessionService;
    private IFileService FileService;
    private DatabaseContext DatabaseContext;
    private ILogger Logger;

    public AttachmentsController(ISessionService sessionService, DatabaseContext databaseContext, IFileService fileService, ILogger<AttachmentsController> logger)
    {
        SessionService = sessionService;
        DatabaseContext = databaseContext;
        FileService = fileService;
        Logger = logger;
    }

    public class AttachmentResponse
    {
        public Guid FileId { get; set; }
        public required string Name { get; set; }
        public required string CdnUrl { get; set; }

        public static AttachmentResponse FromDatabase(Attachment attachment)
        {
            return new AttachmentResponse
            {
                FileId = attachment.Id,
                Name = attachment.Name,
                CdnUrl = attachment.CdnUrl
            };
        }
    }

    [HttpPut, Route("upload/{fileName}")]
    [OpenApiBodyParameter(["application/octet-stream"])]
    public async Task<AttachmentResponse> UploadFile([FromRoute] string fileName)
    {
        var session = await SessionService.VerifyRequest(HttpContext);

        string namespaceId = $"attachments/{Guid.NewGuid()}";
        var length = HttpContext.Request.ContentLength;
        var stream = HttpContext.Request.Body;

        if (length == null || length > 256 * 1000 * 1024)
        {
            throw new FileSizeException(length ?? 0, 256 * 1000 * 1024);
        }

        string fileId = await FileService.UploadFile(stream, namespaceId, fileName);
        string fileUrl = await FileService.GetFileUrl(namespaceId, fileId);

        var attachment = new Attachment
        {
            Name = fileName,
            CdnNamespace = namespaceId,
            CdnUrl = fileUrl
        };

        var insertedAttachment = await DatabaseContext.Attachments.AddAsync(attachment);
        await DatabaseContext.SaveChangesAsync();

        Logger.LogInformation("Uploaded file {} ({}MB) by {}", fileName, (float)length / 1000000, session.AccountId);

        return AttachmentResponse.FromDatabase(insertedAttachment.Entity);
    }
}
