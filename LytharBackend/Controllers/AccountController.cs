﻿using Isopoh.Cryptography.Argon2;
using LytharBackend.Exceptons;
using LytharBackend.Files;
using LytharBackend.ImageGeneration;
using LytharBackend.Ldap;
using LytharBackend.Models;
using LytharBackend.Session;
using LytharBackend.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
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

    [HttpPost, Route("login")]
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

            var newUser = Models.User.FromLdap(
                loginAttempt.Attributes,
                loginAttempt.DistinguishedName.Contains($",{LdapService.AdminGroup},{LdapService.SearchDn}")
            );

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

        if (!LdapService.UserExists(user.Login))
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
            Expires = DateTime.UtcNow.AddHours(6)
        });

        return new LoginResponse
        {
            Status = ResponseStatus.Success,
            Token = token
        };
    }

    [HttpDelete, Route("logout")]
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
        public required bool IsAdmin { get; set; }

        public static UserAccountResponse FromDatabase(Models.User user)
        {
            return new UserAccountResponse
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                IsAdmin = user.IsAdmin
            };
        }
    }

    [HttpGet, Route("account")]
    [SwaggerResponse(200, typeof(UserAccountResponse))]
    public async Task<UserAccountResponse> GetAccount()
    {
        var token = await SessionService.VerifyRequest(HttpContext);
        var account = await DatabaseContext.GetUserById(token.AccountId);

        return UserAccountResponse.FromDatabase(account);
    }

    [HttpGet, Route("accounts")]
    [SwaggerResponse(200, typeof(List<UserAccountResponse>))]
    public async Task<List<UserAccountResponse>> GetAccounts([FromQuery] List<int> accountIds)
    {
        await SessionService.VerifyRequest(HttpContext);

        var accounts = await DatabaseContext.Users.Where(x => accountIds.Contains(x.Id)).ToListAsync();

        return accounts.ConvertAll(UserAccountResponse.FromDatabase);
    }

    public class ListAccountsQuery
    {
        public int? Before { get; set; }
        public int? After { get; set; }
        [Range(1, 1000)]
        public int? Limit { get; set; }
    }

    [HttpGet, Route("accounts/list")]
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

    [HttpPatch, Route("account")]
    [SwaggerResponse(200, typeof(UserAccountResponse))]
    public async Task<UserAccountResponse> UpdateAccount([FromBody] UpdateAccountForm updateAccount)
    {
        var token = await SessionService.VerifyRequest(HttpContext);
        var account = await DatabaseContext.GetUserById(token.AccountId);

        if (updateAccount.FirstName != null) account.Name = updateAccount.FirstName.Trim();
        if (updateAccount.LastName != null) account.LastName = updateAccount.LastName.Trim();
        if (updateAccount.Email != null) account.Email = updateAccount.Email.Trim();

        await DatabaseContext.SaveChangesAsync();

        return UserAccountResponse.FromDatabase(account);
    }

    [HttpPost, Route("account/avatar")]
    [SwaggerResponse(200, typeof(UserAccountResponse))]
    [OpenApiBodyParameter(["image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"])]
    public async Task<UserAccountResponse> UpdateAvatar()
    {
        var token = await SessionService.VerifyRequest(HttpContext);
        var account = await DatabaseContext.GetUserById(token.AccountId);

        long? length = HttpContext.Request.ContentLength;
        var avatarData = HttpContext.Request.Body;

        if (length == null || avatarData == null)
        {
            throw new FileSizeException(0, 1024 * 1024);
        }

        if (length > 32 * 1000 * 1024)
        {
            throw new FileSizeException((long)length, 1000 * 1024);
        }

        if (account.AvatarId != null)
        {
            await FileService.DeleteFile("avatars", account.AvatarId);
        }

        using var memoryStream = await IconCreator.Generate(avatarData, 512, 512);

        var fileName = $"{account.Id}.{Guid.NewGuid()}.webp";

        var avatarId = await FileService.UploadFile(memoryStream, "avatars", fileName);
        var avatarUrl = await FileService.GetFileUrl("avatars", avatarId);

        account.AvatarId = avatarId;
        account.AvatarUrl = avatarUrl;

        await DatabaseContext.SaveChangesAsync();

        var updatedUser = UserAccountResponse.FromDatabase(account);

        await WebSocketClient.Manager.Broadcast(new WebSocketMessage<UserAccountResponse>
        {
            Type = "UserUpdated",
            Data = updatedUser
        });

        return updatedUser;
    }
}
