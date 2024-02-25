using LytharBackend.Files;
using LytharBackend.Session;
using Microsoft.AspNetCore.Mvc;

namespace LytharBackend.Controllers;

[Route("channels/api")]
public class AttachmentsController : Controller
{
    private ISessionService SessionService;
    private IFileService FileService;
    private DatabaseContext DatabaseContext;
}
