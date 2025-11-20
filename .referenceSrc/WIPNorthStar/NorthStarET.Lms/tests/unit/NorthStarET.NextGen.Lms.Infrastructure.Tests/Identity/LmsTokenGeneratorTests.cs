using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.Services;
using Xunit;

namespace NorthStarET.NextGen.Lms.Infrastructure.Tests.Identity;

public sealed class LmsTokenGeneratorTests
{
    private const string ValidSigningKey = "ThisIsASecureSigningKeyWith32OrMoreCharacters";
    private const string ValidIssuer = "TestIssuer";
    private const string ValidAudience = "TestAudience";

    [Fact]
    public void GenerateAccessToken_WithValidParameters_ShouldGenerateValidJwtToken()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var token = generator.GenerateAccessToken(userId, sessionId, expiresAt);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue("the generated token should be a valid JWT");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeUserIdInSubClaim()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var token = generator.GenerateAccessToken(userId, sessionId, expiresAt);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

        subClaim.Should().NotBeNull("the token should contain a 'sub' claim");
        subClaim!.Value.Should().Be(userId.ToString(), "the 'sub' claim should contain the user ID");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeSessionIdInCustomClaim()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var token = generator.GenerateAccessToken(userId, sessionId, expiresAt);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var sessionIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "session_id");

        sessionIdClaim.Should().NotBeNull("the token should contain a 'session_id' claim");
        sessionIdClaim!.Value.Should().Be(sessionId.ToString(), "the 'session_id' claim should contain the session ID");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeExpirationClaim()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var token = generator.GenerateAccessToken(userId, sessionId, expiresAt);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeCloseTo(expiresAt.UtcDateTime, TimeSpan.FromSeconds(5),
            "the token expiration should match the requested expiration time");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeJtiClaim()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var token = generator.GenerateAccessToken(userId, sessionId, expiresAt);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

        jtiClaim.Should().NotBeNull("the token should contain a 'jti' claim for uniqueness");
        Guid.TryParse(jtiClaim!.Value, out _).Should().BeTrue("the 'jti' claim should be a valid GUID");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeIssuedAtClaim()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
        var beforeGeneration = DateTimeOffset.UtcNow;

        // Act
        var token = generator.GenerateAccessToken(userId, sessionId, expiresAt);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var iatClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat);

        iatClaim.Should().NotBeNull("the token should contain an 'iat' claim");
        long.TryParse(iatClaim!.Value, out var iatValue).Should().BeTrue("the 'iat' claim should be a valid Unix timestamp");

        var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iatValue);
        issuedAt.Should().BeCloseTo(beforeGeneration, TimeSpan.FromSeconds(5),
            "the 'iat' claim should reflect when the token was generated");
    }

    [Fact]
    public void GenerateAccessToken_ShouldBeSignedWithHmacSha256()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var token = generator.GenerateAccessToken(userId, sessionId, expiresAt);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.SignatureAlgorithm.Should().Be(SecurityAlgorithms.HmacSha256,
            "the token should be signed with HMAC-SHA256");
    }

    [Fact]
    public void GenerateAccessToken_ShouldBeValidatableWithCorrectKey()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var token = generator.GenerateAccessToken(userId, sessionId, expiresAt);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ValidSigningKey));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = ValidIssuer,
            ValidateAudience = true,
            ValidAudience = ValidAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var act = () => handler.ValidateToken(token, validationParameters, out _);
        act.Should().NotThrow("a valid token should pass validation with correct parameters");
    }

    [Fact]
    public void GenerateAccessToken_WithTamperedToken_ShouldFailValidation()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var token = generator.GenerateAccessToken(userId, sessionId, expiresAt);
        var tamperedToken = token.Substring(0, token.Length - 5) + "XXXXX"; // Tamper with signature

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ValidSigningKey));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = ValidIssuer,
            ValidateAudience = true,
            ValidAudience = ValidAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var act = () => handler.ValidateToken(tamperedToken, validationParameters, out _);
        act.Should().Throw<SecurityTokenException>("tampered tokens should fail validation");
    }

    [Fact]
    public void GenerateAccessToken_WithMissingSigningKey_ShouldThrowException()
    {
        // Arrange
        var settings = new IdentityModuleSettings
        {
            JwtSigningKey = string.Empty, // Missing key
            JwtIssuer = ValidIssuer,
            JwtAudience = ValidAudience
        };
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var act = () => generator.GenerateAccessToken(userId, sessionId, expiresAt);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT signing key is not configured*");
    }

    [Fact]
    public void GenerateAccessToken_WithShortSigningKey_ShouldThrowException()
    {
        // Arrange
        var settings = new IdentityModuleSettings
        {
            JwtSigningKey = "TooShortKey", // Less than 32 characters
            JwtIssuer = ValidIssuer,
            JwtAudience = ValidAudience
        };
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var act = () => generator.GenerateAccessToken(userId, sessionId, expiresAt);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be at least 32 characters*");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeIssuerClaim()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var token = generator.GenerateAccessToken(userId, sessionId, expiresAt);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be(ValidIssuer, "the token should contain the configured issuer");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeAudienceClaim()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var token = generator.GenerateAccessToken(userId, sessionId, expiresAt);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Audiences.Should().Contain(ValidAudience, "the token should contain the configured audience");
    }

    [Fact]
    public void GenerateAccessToken_CalledMultipleTimes_ShouldGenerateUniqueTokens()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var token1 = generator.GenerateAccessToken(userId, sessionId, expiresAt);
        var token2 = generator.GenerateAccessToken(userId, sessionId, expiresAt);

        // Assert
        token1.Should().NotBe(token2, "each call should generate a unique token due to unique 'jti' claim");
    }

    [Fact]
    public void GenerateAccessToken_WithDifferentUserIds_ShouldGenerateDifferentTokens()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var token1 = generator.GenerateAccessToken(userId1, sessionId, expiresAt);
        var token2 = generator.GenerateAccessToken(userId2, sessionId, expiresAt);

        // Assert
        token1.Should().NotBe(token2, "tokens for different users should be different");

        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);

        jwtToken1.Subject.Should().Be(userId1.ToString());
        jwtToken2.Subject.Should().Be(userId2.ToString());
    }

    [Fact]
    public void GenerateAccessToken_WithDifferentSessionIds_ShouldGenerateDifferentTokens()
    {
        // Arrange
        var settings = CreateValidSettings();
        var generator = new LmsTokenGenerator(Options.Create(settings));
        var userId = Guid.NewGuid();
        var sessionId1 = Guid.NewGuid();
        var sessionId2 = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);

        // Act
        var token1 = generator.GenerateAccessToken(userId, sessionId1, expiresAt);
        var token2 = generator.GenerateAccessToken(userId, sessionId2, expiresAt);

        // Assert
        token1.Should().NotBe(token2, "tokens for different sessions should be different");

        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);

        var sessionIdClaim1 = jwtToken1.Claims.First(c => c.Type == "session_id");
        var sessionIdClaim2 = jwtToken2.Claims.First(c => c.Type == "session_id");

        sessionIdClaim1.Value.Should().Be(sessionId1.ToString());
        sessionIdClaim2.Value.Should().Be(sessionId2.ToString());
    }

    private static IdentityModuleSettings CreateValidSettings()
    {
        return new IdentityModuleSettings
        {
            JwtSigningKey = ValidSigningKey,
            JwtIssuer = ValidIssuer,
            JwtAudience = ValidAudience
        };
    }
}
