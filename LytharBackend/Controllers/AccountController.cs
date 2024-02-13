using Isopoh.Cryptography.Argon2;
using LytharBackend.Exceptons;
using LytharBackend.Ldap;
using LytharBackend.Session;
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
    private ISessionService SessionService;
    private DatabaseContext DatabaseContext;

    public AccountController(LdapService ldapService, ISessionService sessionService, DatabaseContext databaseContext)
    {
        LdapService = ldapService;
        SessionService = sessionService;
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
        public ResponseStatus Status { get; set; }
        public string? Token { get; set; }
    }

    public class LoginForm
    {
        [Required]
        public string Login { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        public string? NewPassword { get; set; }
    }

    [HttpPost]
    [Route("login")]
    [SwaggerResponse(HttpStatusCode.OK, typeof(LoginResponse))]
    [SwaggerResponse(HttpStatusCode.BadRequest, typeof(BaseHttpExceptionOptions))]
    [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(BaseHttpExceptionOptions))]
    [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(BaseHttpExceptionOptions))]
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
            else if (loginForm.NewPassword == null)
            {
                return new LoginResponse { Status = ResponseStatus.SetNewPassword };
            }

            var newUser = Models.User.FromLdap(loginAttempt.Attributes);

            newUser.Password = Argon2.Hash(loginForm.NewPassword);

            var newUserResult = await DatabaseContext.Users.AddAsync(newUser);
            await DatabaseContext.SaveChangesAsync();

            user = newUserResult.Entity;
        }
        else if (loginForm.NewPassword != null)
        {
            throw new AccountExistsException(loginForm.Login);
        }
        else if (!Argon2.Verify(user.Password, loginForm.Password))
        {
            throw new InvalidLoginException();
        }

        var token = await SessionService.CreateSession(new CreateSessionOptions
        {
            AccountId = user.Id.ToString(),
            Username = user.Login
        });

        return new LoginResponse
        {
            Status = ResponseStatus.Success,
            Token = token
        };
    }
}
