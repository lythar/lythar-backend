﻿using System.Security.Claims;
using System.Security.Cryptography;
using LytharBackend.Exceptons;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LytharBackend.Session;

public class JwtSessionsService : ISessionService
{
    private readonly IConfiguration Configuration;

    private readonly RsaSecurityKey SecurityKey;
    private readonly JsonWebTokenHandler TokenHandler = new();

    public JwtSessionsService(IConfiguration configuration)
    {
        Configuration = configuration;

        string? privateKey = Configuration["Jwt:PrivateKey"];

        if (privateKey == null)
        {
            throw new Exception("'Jwt:PrivateKey' not found in configuration.");
        }

        privateKey = privateKey
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .ReplaceLineEndings("")
            .Replace("-----END PRIVATE KEY-----", "");

        var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);

        SecurityKey = new(rsa);
    }

    public Task<string> CreateSession(CreateSessionOptions options)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.RsaSha256Signature),
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
            AccountId = accountId,
            Username = username,
            ExpiresAt = DateTime.Now,
            CreatedAt = DateTime.Now,
            SessionId = Guid.NewGuid().ToString()
        };
    }
}
