using System.Security.Claims;
using System.Security.Cryptography;
using LytharBackend.Exceptons;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LytharBackend.Session;

public static class JwtUtils
{
    public static DateTime GetExpiry(this TokenValidationResult token)
    {
        var expiresAt = (long)token.Claims.First(x => x.Key == "exp").Value;
        return DateTimeOffset.FromUnixTimeSeconds(expiresAt).DateTime;
    }

    public static DateTime GetCreatedAt(this TokenValidationResult token)
    {
        var createdAt = (long)token.Claims.First(x => x.Key == "iat").Value;
        return DateTimeOffset.FromUnixTimeSeconds(createdAt).DateTime;
    }
}

public class JwtSessionsService : ISessionService
{
    private readonly IConfiguration Configuration;
    private readonly ILogger Logger;

    private readonly RsaSecurityKey SecurityKey;
    private readonly JsonWebTokenHandler TokenHandler = new();

    public JwtSessionsService(IConfiguration configuration, ILogger<JwtSessionsService> logger)
    {
        Configuration = configuration;
        Logger = logger;

        string? privateKey = Configuration["Jwt:PrivateKey"];

        if (privateKey == null)
        {
            throw new Exception("'Jwt:PrivateKey' not found in configuration.");
        }

        privateKey = privateKey.Contains("-----") ? privateKey : File.ReadAllText(privateKey);

        privateKey = privateKey
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .ReplaceLineEndings("")
            .Replace("-----END PRIVATE KEY-----", "");

        byte[] privateKeyBytes = Convert.FromBase64String(privateKey);
        var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

        SecurityKey = new RsaSecurityKey(rsa);
    }

    public Task<string> CreateSession(CreateSessionOptions options)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.RsaSha256),
            Issuer = "Lythar",
            Expires = DateTime.UtcNow.AddHours(1),
            IssuedAt = DateTime.UtcNow,
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", options.AccountId),
                new Claim("name", options.Username),
                new Claim("p", Guid.NewGuid().ToString())
            })
        };

        return Task.FromResult(TokenHandler.CreateToken(tokenDescriptor));
    }

    public async Task<SessionData> VerifySession(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "Lythar",
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = SecurityKey
        };

        var validated = await TokenHandler.ValidateTokenAsync(token, tokenValidationParameters);

        if (!validated.IsValid)
        {
            throw new UnauthorizedException();
        }

        var accountId = (string?)validated.Claims.First(x => x.Key == "sub").Value;
        var username = (string?)validated.Claims.First(x => x.Key == "name").Value;

        if (accountId == null || username == null)
        {
            throw new UnauthorizedException();
        }

        return new SessionData
        {
            AccountId = int.Parse(accountId),
            Username = username,
            ExpiresAt = validated.GetExpiry(),
            CreatedAt = validated.GetCreatedAt(),
            SessionId = Guid.NewGuid().ToString()
        };
    }
}
