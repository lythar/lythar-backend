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
        [JsonPropertyName("login")]
        public string Login { get; set; } = null!;
        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; } = null!;
        [JsonPropertyName("newPassword")]
        public string? NewPassword { get; set; }
    }

    [HttpPost]
    [Route("login")]
    public async Task<LoginResponse> Login([FromBody] LoginForm loginForm)
    {
        var auth = LdapService.ValidateLogin(loginForm.Login, loginForm.Password);
        var user = Models.User.FromLdap(auth.Attributes);

        DatabaseContext.Users.Add(user);
        await DatabaseContext.SaveChangesAsync();

        return new LoginResponse
        {
            Status = ResponseStatus.Success
        };
    }
}
