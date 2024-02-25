using LytharBackend.Files;
using LytharBackend.Session;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost, Route("upload")]
    public async Task UploadFile()
    { }
}
