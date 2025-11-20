using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Services;

/// <summary>
/// Generates JWT-based LMS access tokens with session metadata and cryptographic signatures.
/// </summary>
internal sealed class LmsTokenGenerator : ILmsTokenGenerator
{
    private readonly IdentityModuleSettings settings;
    private readonly JwtSecurityTokenHandler tokenHandler;

    public LmsTokenGenerator(IOptions<IdentityModuleSettings> settings)
    {
        this.settings = settings.Value;
        tokenHandler = new JwtSecurityTokenHandler();
    }

    public string GenerateAccessToken(Guid userId, Guid sessionId, DateTimeOffset expiresAt)
    {
        if (string.IsNullOrWhiteSpace(settings.JwtSigningKey))
        {
            throw new InvalidOperationException(
                "JWT signing key is not configured. Please set IdentityModuleSettings:JwtSigningKey in configuration.");
        }

        if (settings.JwtSigningKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT signing key must be at least 32 characters for HS256 algorithm.");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("session_id", sessionId.ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.JwtIssuer,
            audience: settings.JwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials
        );

        return tokenHandler.WriteToken(token);
    }
}
