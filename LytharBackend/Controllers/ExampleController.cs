using LytharBackend.Ldap;
using Microsoft.AspNetCore.Mvc;

namespace LytharBackend.Controllers;

[Route("example/api")]
public class ExampleController : Controller
{
    private LdapService LdapService;

    public ExampleController(LdapService ldapService)
    {
        LdapService = ldapService;
    }

    [HttpGet]
    [Route("")]
    public string Index()
    {
        LdapService.Test();
        return "Hello, World!";
    }
}
