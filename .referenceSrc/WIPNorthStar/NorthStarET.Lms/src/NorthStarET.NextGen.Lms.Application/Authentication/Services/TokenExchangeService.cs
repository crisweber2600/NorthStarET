using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Services;

public sealed class TokenExchangeService : ITokenExchangeService
{
    private const string EmailClaim = ClaimTypes.Email;
    private const string NameClaim = ClaimTypes.Name;
    private const string GivenNameClaim = ClaimTypes.GivenName;
    private const string FamilyNameClaim = ClaimTypes.Surname;
    private const string SubjectClaim = ClaimTypes.NameIdentifier;
    private const string ObjectIdClaim = "oid";
    private const string PlainEmailClaim = "email";
    private const string PreferredUsernameClaim = "preferred_username";
    private const string EmailsClaim = "emails";

    private static readonly string[] EmailClaimPriority =
    {
        EmailClaim,
        PlainEmailClaim,
        PreferredUsernameClaim,
        EmailsClaim,
        ClaimTypes.Upn
    };

    private readonly ILogger<TokenExchangeService> logger;
    private readonly IEntraTokenValidator tokenValidator;
    private readonly IUserRepository userRepository;
    private readonly ITenantRepository tenantRepository;
    private readonly IMembershipRepository membershipRepository;
    private readonly IRoleRepository roleRepository;
    private readonly ISessionRepository sessionRepository;
    private readonly ISessionStore sessionStore;
    private readonly ILmsTokenGenerator lmsTokenGenerator;
    private readonly IdentityModuleSettings options;

    public TokenExchangeService(
        ILogger<TokenExchangeService> logger,
        IEntraTokenValidator tokenValidator,
        IUserRepository userRepository,
        ITenantRepository tenantRepository,
        IMembershipRepository membershipRepository,
        IRoleRepository roleRepository,
        ISessionRepository sessionRepository,
        ISessionStore sessionStore,
        ILmsTokenGenerator lmsTokenGenerator,
        IOptions<IdentityModuleSettings> options)
    {
        this.logger = logger;
        this.tokenValidator = tokenValidator;
        this.userRepository = userRepository;
        this.tenantRepository = tenantRepository;
        this.membershipRepository = membershipRepository;
        this.roleRepository = roleRepository;
        this.sessionRepository = sessionRepository;
        this.sessionStore = sessionStore;
        this.lmsTokenGenerator = lmsTokenGenerator;
        this.options = options.Value;
    }

    public async Task<TokenExchangeResult> ExchangeAsync(TokenExchangeCommandContext context, CancellationToken cancellationToken = default)
    {
        var principal = await tokenValidator.ValidateAsync(context.EntraToken, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTimeOffset.UtcNow;

        var entraSubjectId = ExtractSubject(principal);
        var email = ResolveEmail(principal, entraSubjectId)
            ?? throw new InvalidOperationException("Email claim missing from Entra token.");
        var firstName = ExtractClaim(principal, GivenNameClaim) ?? ExtractNameFragment(principal, NameFragment.First);
        var lastName = ExtractClaim(principal, FamilyNameClaim) ?? ExtractNameFragment(principal, NameFragment.Last);

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            throw new InvalidOperationException("Unable to determine user first and last name from Entra token claims.");
        }

        var user = await userRepository.GetByEntraSubjectIdAsync(entraSubjectId, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        bool userCreated = false;
        bool isFirstUser = false;

        if (user is null)
        {
            // Check if this is the first user in the system
            var userCount = await userRepository.GetUserCountAsync(cancellationToken);
            isFirstUser = userCount == 0;

            user = new User(Guid.NewGuid(), entraSubjectId, email, firstName, lastName, now, true);
            user.RegisterLogin(now);
            await userRepository.AddAsync(user, cancellationToken);
            userCreated = true;

            if (isFirstUser)
            {
                logger.LogInformation("First user {UserId} detected, creating system admin access", user.Id);
            }
        }
        else
        {
            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation("Updating email for user {UserId} from {OldEmail} to {NewEmail}", user.Id, user.Email, email);
            }

            user.UpdateName(firstName, lastName);
            user.RegisterLogin(now);
            await userRepository.UpdateAsync(user, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Get all active memberships first
        var authorizationTenants = await membershipRepository.GetActiveMembershipsForUserAsync(user.Id, cancellationToken);

        // If this is the first user and they have no memberships, create system admin access
        if (isFirstUser && authorizationTenants.Count == 0)
        {
            await CreateSystemAdminAccessAsync(user.Id, now, cancellationToken);
            authorizationTenants = await membershipRepository.GetActiveMembershipsForUserAsync(user.Id, cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (authorizationTenants.Count == 0)
        {
            logger.LogWarning("User {UserId} attempted to authenticate without any active memberships", user.Id);
            throw new InvalidOperationException("Active membership required for token exchange.");
        }

        // If no specific tenant requested, select the first available
        TenantId activeTenantId;
        Membership membership;
        if (context.ActiveTenantId == Guid.Empty)
        {
            membership = authorizationTenants.FirstOrDefault()
                ?? throw new InvalidOperationException("Active membership required for token exchange.");
            activeTenantId = membership.TenantId;
            logger.LogInformation("No tenant specified, auto-selected first available tenant {TenantId} for user {UserId}", activeTenantId.Value, user.Id);
        }
        else
        {
            activeTenantId = new TenantId(context.ActiveTenantId);
            membership = authorizationTenants.FirstOrDefault(m => m.TenantId == activeTenantId)
                ?? throw new InvalidOperationException($"User does not have active membership for tenant {activeTenantId.Value}.");
        }

        var role = await roleRepository.GetAsync(membership.RoleId, cancellationToken);
        if (role is null)
        {
            throw new InvalidOperationException($"Role {membership.RoleId} referenced by membership is missing.");
        }

        var tenantMap = await tenantRepository.GetByIdsAsync(authorizationTenants.Select(m => m.TenantId), cancellationToken);

        var activeTenant = tenantMap.SingleOrDefault(tenant => tenant.Id == activeTenantId)
            ?? throw new InvalidOperationException("Active tenant not found in membership list.");

        var entraTokenHash = ComputeHash(context.EntraToken);
        var sessionTtl = TimeSpan.FromMinutes(Math.Max(1, options.SessionSlidingExpirationMinutes));
        var expiresAt = now.Add(sessionTtl);

        var existingSession = await sessionRepository.GetByTokenHashAsync(entraTokenHash, cancellationToken);
        Session session;
        string lmsAccessToken;

        if (existingSession is not null)
        {
            // Generate JWT token for existing session
            lmsAccessToken = lmsTokenGenerator.GenerateAccessToken(user.Id, existingSession.Id, expiresAt);
            existingSession.Refresh(lmsAccessToken, expiresAt, now);
            existingSession.UpdateEntraTokenHash(entraTokenHash);
            session = existingSession;
            await sessionRepository.UpdateAsync(existingSession, cancellationToken);
        }
        else
        {
            // Create new session ID first, then generate JWT token with that ID
            var newSessionId = Guid.NewGuid();
            lmsAccessToken = lmsTokenGenerator.GenerateAccessToken(user.Id, newSessionId, expiresAt);

            session = Session.CreateWithId(
                newSessionId,
                user.Id,
                entraTokenHash,
                lmsAccessToken,
                activeTenantId,
                now,
                expiresAt,
                context.IpAddress,
                context.UserAgent);

            user.AttachSession(session);
            await sessionRepository.AddAsync(session, cancellationToken);
        }

        await sessionStore.CacheSessionAsync(session, session.ExpiresAt - now, cancellationToken);

        var availableTenants = BuildTenantSummaries(authorizationTenants, tenantMap);

        var userContext = new UserContextModel(
            user.Id,
            user.Email,
            user.DisplayName,
            activeTenant.Id.Value,
            activeTenant.Name,
            activeTenant.Type.ToString(),
            role.DisplayName);

        logger.LogInformation(
            "Token exchange succeeded for user {UserId} in tenant {TenantId} (newUser={NewUser})",
            user.Id,
            activeTenant.Id.Value,
            userCreated);

        return new TokenExchangeResult(
            session.Id,
            session.ExpiresAt,
            lmsAccessToken,
            userContext,
            availableTenants);
    }

    private static EntraSubjectId ExtractSubject(ClaimsPrincipal principal)
    {
        var subject = principal.FindFirst(SubjectClaim)?.Value
            ?? principal.FindFirst(ObjectIdClaim)?.Value
            ?? principal.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new InvalidOperationException("Subject claim missing from Entra token.");
        }

        return new EntraSubjectId(subject);
    }

    private static string? ExtractClaim(ClaimsPrincipal principal, string claimType)
    {
        return principal.FindFirst(claimType)?.Value;
    }

    private string? ResolveEmail(ClaimsPrincipal principal, EntraSubjectId subjectId)
    {
        foreach (var claimType in EmailClaimPriority)
        {
            var rawValue = ExtractClaim(principal, claimType);
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                continue;
            }

            var normalized = NormalizeEmailClaim(claimType, rawValue);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                continue;
            }

            if (!string.Equals(claimType, EmailClaim, StringComparison.Ordinal))
            {
                logger.LogInformation(
                    "Email claim missing; using {ClaimType} claim for Entra subject {Subject}",
                    claimType,
                    subjectId.Value);
            }

            return normalized;
        }

        return null;
    }

    private static string? NormalizeEmailClaim(string claimType, string value)
    {
        var trimmed = value.Trim();

        if (claimType.Equals(EmailsClaim, StringComparison.OrdinalIgnoreCase))
        {
            var candidates = trimmed.Trim('[', ']')
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(segment => segment.Trim().Trim('"'));

            foreach (var candidate in candidates)
            {
                if (IsLikelyEmail(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        return IsLikelyEmail(trimmed) ? trimmed : null;
    }

    private static bool IsLikelyEmail(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();
        if (candidate.Contains(' '))
        {
            return false;
        }

        var atIndex = candidate.IndexOf('@');
        if (atIndex <= 0 || atIndex == candidate.Length - 1)
        {
            return false;
        }

        return true;
    }

    private enum NameFragment
    {
        First,
        Last
    }

    private static string? ExtractNameFragment(ClaimsPrincipal principal, NameFragment fragment)
    {
        var name = ExtractClaim(principal, NameClaim);
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return null;
        }

        return fragment switch
        {
            NameFragment.First => parts[0],
            NameFragment.Last => parts[^1],
            _ => null
        };
    }

    private static string ComputeHash(string entraToken)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(entraToken));
        return Convert.ToHexString(bytes);
    }

    private static IReadOnlyCollection<TenantSummaryModel> BuildTenantSummaries(
        IEnumerable<Membership> memberships,
        IReadOnlyCollection<Tenant> tenants)
    {
        var tenantDictionary = tenants.ToDictionary(tenant => tenant.Id, tenant => tenant);

        var summaries = new List<TenantSummaryModel>();
        foreach (var membership in memberships)
        {
            if (!tenantDictionary.TryGetValue(membership.TenantId, out var tenant))
            {
                continue;
            }

            summaries.Add(new TenantSummaryModel(tenant.Id.Value, tenant.Name, tenant.Type.ToString()));
        }

        return summaries;
    }

    private async Task CreateSystemAdminAccessAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        const string PlatformTenantName = "Platform";
        const string PlatformAdminRoleName = "PlatformAdmin";

        // Get or create Platform tenant
        var platformTenant = await tenantRepository.GetByExternalIdAsync(PlatformTenantName, cancellationToken);
        if (platformTenant is null)
        {
            var tenantId = new TenantId(Guid.NewGuid());
            platformTenant = new Tenant(
                tenantId,
                PlatformTenantName,
                TenantType.District,
                now,
                isActive: true,
                parentTenantId: null,
                externalId: PlatformTenantName);

            await tenantRepository.AddAsync(platformTenant, cancellationToken);
            logger.LogInformation("Created platform tenant {TenantId} for first system admin", tenantId.Value);
        }

        // Get or create PlatformAdmin role
        var platformAdminRole = await roleRepository.GetByNameAsync(PlatformAdminRoleName, cancellationToken);
        if (platformAdminRole is null)
        {
            platformAdminRole = new Role(
                Guid.NewGuid(),
                PlatformAdminRoleName,
                "Platform Administrator",
                now,
                isSystemRole: true,
                initialPermissions: null,
                description: "System administrator with platform-wide access");

            await roleRepository.AddAsync(platformAdminRole, cancellationToken);
            logger.LogInformation("Created PlatformAdmin role {RoleId} for first system admin", platformAdminRole.Id);
        }

        // Create membership linking first user to platform tenant with PlatformAdmin role
        var membership = new Membership(
            Guid.NewGuid(),
            userId,
            platformTenant.Id,
            platformAdminRole.Id,
            now,
            grantedBy: null, // First user, so no granter
            expiresAt: null, // Never expires
            isActive: true);

        await membershipRepository.AddAsync(membership, cancellationToken);
        logger.LogInformation("Created membership {MembershipId} for first system admin user {UserId}", membership.Id, userId);
    }
}
