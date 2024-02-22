using Isopoh.Cryptography.Argon2;
using LytharBackend.Exceptons;
using LytharBackend.Files;
using LytharBackend.Ldap;
using LytharBackend.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LytharBackend.Controllers;

[Route("account/api")]
public class AccountController : Controller
{
    private LdapService LdapService;
    private ISessionService SessionService;
    private IFileService FileService;
    private DatabaseContext DatabaseContext;

    public AccountController(LdapService ldapService, ISessionService sessionService, DatabaseContext databaseContext, IFileService fileService)
    {
        LdapService = ldapService;
        SessionService = sessionService;
        DatabaseContext = databaseContext;
        FileService = fileService;
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
        [MaxLength(32)]
        public string? NewPassword { get; set; }
    }

    [HttpPost]
    [Route("login")]
    [SwaggerResponse(200, typeof(LoginResponse))]
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

        HttpContext.Response.Cookies.Append("token", token, new CookieOptions
        {
            Expires = DateTime.UtcNow.AddHours(1)
        });

        return new LoginResponse
        {
            Status = ResponseStatus.Success,
            Token = token
        };
    }

    [HttpDelete]
    [Route("logout")]
    public IActionResult Logout()
    {
        HttpContext.Response.Cookies.Delete("token");

        return Ok();
    }

    public class UserAccountResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
    }

    [HttpGet]
    [Route("account")]
    [SwaggerResponse(200, typeof(UserAccountResponse))]
    public async Task<UserAccountResponse> GetAccount()
    {
        var token = await SessionService.VerifyRequest(HttpContext);
        var account = await DatabaseContext.Users.Where(x => x.Id == token.AccountId).FirstOrDefaultAsync();

        if (account == null)
        {
            throw new AccountNotFoundException(token.AccountId.ToString());
        }

        return new UserAccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            LastName = account.LastName,
            Email = account.Email,
            AvatarUrl = account.AvatarUrl
        };
    }

    [HttpGet]
    [Route("accounts")]
    [SwaggerResponse(200, typeof(List<UserAccountResponse>))]
    public async Task<List<UserAccountResponse>> GetAccounts([FromQuery] List<int> accountIds)
    {
        await SessionService.VerifyRequest(HttpContext);

        var accounts = await DatabaseContext.Users.Where(x => accountIds.Contains(x.Id)).ToListAsync();

        return accounts.ConvertAll(x => new UserAccountResponse
        {
            Id = x.Id,
            Name = x.Name,
            LastName = x.LastName,
            Email = x.Email,
            AvatarUrl = x.AvatarUrl
        });
    }

    public class ListAccountsQuery
    {
        public int? Before { get; set; }
        public int? After { get; set; }
        [Range(1, 1000)]
        public int? Limit { get; set; }
    }

    [HttpGet]
    [Route("accounts/list")]
    [SwaggerResponse(200, typeof(List<int>))]
    public async Task<List<int>> GetAccountsList([FromQuery] ListAccountsQuery query)
    {
        await SessionService.VerifyRequest(HttpContext);

        return await DatabaseContext.Users
            .OrderBy(x => x.Id)
            .Where(x => query.Before == null || x.Id < query.Before)
            .Where(x => query.After == null || x.Id > query.After)
            .Take(query.Limit ?? 1000)
            .Select(x => x.Id)
            .ToListAsync();
    }

    public class UpdateAccountForm
    {
        [MaxLength(32)]
        public string? FirstName { get; set; }
        [MaxLength(32)]
        public string? LastName { get; set; }
        [MaxLength(64)]
        public string? Email { get; set; }
    }

    [HttpPatch]
    [Route("account")]
    [SwaggerResponse(200, typeof(UserAccountResponse))]
    public async Task<UserAccountResponse> UpdateAccount([FromBody] UpdateAccountForm updateAccount)
    {
        var token = await SessionService.VerifyRequest(HttpContext);
        var account = await DatabaseContext.Users.Where(x => x.Id == token.AccountId).FirstOrDefaultAsync();

        if (account == null)
        {
            throw new AccountNotFoundException(token.AccountId.ToString());
        }

        if (updateAccount.FirstName != null) account.Name = updateAccount.FirstName.Trim();
        if (updateAccount.LastName != null) account.LastName = updateAccount.LastName.Trim();
        if (updateAccount.Email != null) account.Email = updateAccount.Email.Trim();

        await DatabaseContext.SaveChangesAsync();

        return new UserAccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            LastName = account.LastName,
            Email = account.Email,
            AvatarUrl = account.AvatarUrl
        };
    }

    [HttpPost]
    [Route("account/avatar")]
    [SwaggerResponse(200, typeof(UserAccountResponse))]
    public async Task<UserAccountResponse> UpdateAvatar([FromBody] MemoryStream avatarData)
    {
        var token = await SessionService.VerifyRequest(HttpContext);
        var account = await DatabaseContext.Users.Where(x => x.Id == token.AccountId).FirstOrDefaultAsync();

        if (account == null)
        {
            throw new AccountNotFoundException(token.AccountId.ToString());
        }

        var avatarId = await FileService.UploadFile(avatarData, "avatars", account.Id.ToString());
        var avatarUrl = await FileService.GetFileUrl("avatars", avatarId);

        account.AvatarUrl = avatarUrl;

        await DatabaseContext.SaveChangesAsync();

        return new UserAccountResponse
        {
            Id = account.Id,
            Name = account.Name,
            LastName = account.LastName,
            Email = account.Email,
            AvatarUrl = account.AvatarUrl
        };
    }
}
