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
        return LdapService.ValidateLogin("wkijek", "54321") ? "true" : "false";
    }
}
