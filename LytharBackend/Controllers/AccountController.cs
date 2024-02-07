using LytharBackend.Ldap;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LytharBackend.Controllers;

[Route("account/api")]
public class AccountController : Controller
{
    private LdapService LdapService;
    private DatabaseContext DatabaseContext;

    public AccountController(LdapService ldapService, DatabaseContext databaseContext)
    {
        LdapService = ldapService;
        DatabaseContext = databaseContext;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ResponseStatus
    {
        Success,
        SetNewPassword
    }

    public class LoginResponse
    {
        [Required]
        [JsonPropertyName("status")]
        public ResponseStatus Status { get; set; }
    }

    public class LoginForm
    {
        [Required]
        [JsonPropertyName("username")]
        public string Username { get; set; } = null!;
        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; } = null!;
    }

    [HttpPost]
    [Route("login")]
    public LoginResponse Login([FromBody] LoginForm loginForm)
    {
        return new LoginResponse
        {
            Status = ResponseStatus.Success
        };
    }
}
