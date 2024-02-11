using Isopoh.Cryptography.Argon2;
using LytharBackend.Exceptons;
using LytharBackend.Ldap;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;
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
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }

    public class LoginForm
    {
        [Required]
        [JsonPropertyName("login")]
        public string Login { get; set; } = null!;
        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; } = null!;
    }

    public class RegisterForm : LoginForm
    {
        [JsonPropertyName("newPassword")]
        public string? NewPassword { get; set; }
    }

    [HttpPost]
    [Route("login")]
    [SwaggerResponse(HttpStatusCode.OK, typeof(LoginResponse))]
    [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(BaseHttpExceptionOptions))]
    public async Task<LoginResponse> Login([FromBody] LoginForm loginForm)
    {
        var user = await DatabaseContext.Users.Where(x => x.Login == loginForm.Login).FirstOrDefaultAsync();

        if (user == null)
        {
            var loginAttempt = LdapService.ValidateLogin(loginForm.Login, loginForm.Password);

            if (loginAttempt == null)
            {
                throw new InvalidLoginException();
            }

            return new LoginResponse { Status = ResponseStatus.SetNewPassword };
        }

        if (!Argon2.Verify(user.Password, loginForm.Password))
        {
            throw new InvalidLoginException();
        }

        return new LoginResponse
        {
            Status = ResponseStatus.Success
        };
    }
}
