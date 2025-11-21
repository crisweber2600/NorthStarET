using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using NorthStarET.Foundation.Identity.Application.Interfaces;
using NorthStarET.Foundation.Identity.Domain.Entities;
using NorthStarET.Foundation.Identity.Domain.ValueObjects;

namespace NorthStarET.Foundation.Identity.Application.Commands;

public class ExchangeTokenCommandHandler : IRequestHandler<ExchangeTokenCommand, ExchangeTokenResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionManager _sessionManager;
    private readonly IAuditRepository _auditRepository;
    private readonly ILogger<ExchangeTokenCommandHandler> _logger;
    
    public ExchangeTokenCommandHandler(
        IUserRepository userRepository,
        ISessionManager sessionManager,
        IAuditRepository auditRepository,
        ILogger<ExchangeTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _sessionManager = sessionManager;
        _auditRepository = auditRepository;
        _logger = logger;
    }
    
    public async Task<ExchangeTokenResult> Handle(ExchangeTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Validate Entra ID token using Microsoft.Identity.Web
            // For now, we'll extract mock claims from the token
            var mockClaims = ExtractMockClaimsFromToken(request.AccessToken);
            
            var entraSubjectId = new EntraSubjectId(mockClaims.SubjectId);
            var tenantId = mockClaims.TenantId;
            var email = mockClaims.Email;
            var firstName = mockClaims.FirstName;
            var lastName = mockClaims.LastName;
            var userPrincipalName = mockClaims.UserPrincipalName;
            var roles = mockClaims.Roles;
            
            // Get or create user
            var user = await _userRepository.GetByExternalProviderAsync("EntraID", entraSubjectId, cancellationToken);
            
            if (user == null)
            {
                user = new User(tenantId, email, firstName, lastName);
                user.LinkExternalProvider("EntraID", entraSubjectId, userPrincipalName);
                await _userRepository.AddAsync(user, cancellationToken);
                await _userRepository.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Created new user {Email} for tenant {TenantId}", email, tenantId);
            }
            else
            {
                user.RecordLogin();
                await _userRepository.UpdateAsync(user, cancellationToken);
                await _userRepository.SaveChangesAsync(cancellationToken);
            }
            
            var isAdmin = roles.Any(r => r.Contains("Admin", StringComparison.OrdinalIgnoreCase));
            
            var claimsJson = JsonSerializer.Serialize(new
            {
                sub = entraSubjectId.Value,
                email,
                name = $"{firstName} {lastName}",
                preferred_username = userPrincipalName,
                roles,
                tenant_id = tenantId
            });
            
            var session = await _sessionManager.CreateSessionAsync(
                tenantId: tenantId,
                userId: user.Id,
                userPrincipalName: userPrincipalName,
                isAdmin: isAdmin,
                ipAddress: request.IpAddress,
                userAgent: request.UserAgent,
                claimsJson: claimsJson,
                cancellationToken: cancellationToken
            );
            
            await _auditRepository.LogAuthenticationAsync(
                tenantId: tenantId,
                userId: user.Id,
                sessionId: session.Id,
                eventType: "UserAuthenticated",
                isSuccess: true,
                eventData: JsonSerializer.Serialize(new { provider = "EntraID", isAdmin }),
                ipAddress: request.IpAddress,
                userAgent: request.UserAgent,
                cancellationToken: cancellationToken
            );
            
            _logger.LogInformation("User {Email} authenticated successfully. Session: {SessionId}", email, session.SessionId);
            
            return new ExchangeTokenResult(
                SessionId: session.SessionId,
                UserId: user.Id,
                UserPrincipalName: userPrincipalName,
                ExpiresAt: session.ExpiresAt,
                Roles: roles
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token exchange failed");
            
            await _auditRepository.LogAuthenticationAsync(
                tenantId: Guid.Empty,
                userId: null,
                sessionId: null,
                eventType: "AuthenticationFailed",
                isSuccess: false,
                errorMessage: ex.Message,
                ipAddress: request.IpAddress,
                userAgent: request.UserAgent,
                cancellationToken: cancellationToken
            );
            
            throw;
        }
    }
    
    private MockClaimsData ExtractMockClaimsFromToken(string accessToken)
    {
        // TODO: Replace with actual Microsoft.Identity.Web token validation
        return new MockClaimsData
        {
            SubjectId = "00000000-0000-0000-0000-000000000001",
            TenantId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Email = "user@example.com",
            FirstName = "Test",
            LastName = "User",
            UserPrincipalName = "user@example.com",
            Roles = new[] { "Staff" }
        };
    }
    
    private class MockClaimsData
    {
        public string SubjectId { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserPrincipalName { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
