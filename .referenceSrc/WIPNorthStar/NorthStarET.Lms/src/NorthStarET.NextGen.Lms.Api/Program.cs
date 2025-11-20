using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using NorthStarET.NextGen.Lms.Application;
using NorthStarET.NextGen.Lms.Api.Authentication;
using NorthStarET.NextGen.Lms.Api.Configuration;
using NorthStarET.NextGen.Lms.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using NorthStarET.NextGen.Lms.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApplication();
builder.AddRedisClient("identity-redis");
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<DatabaseMigrationHostedService>();

builder.Services.Configure<SecurityHeadersOptions>(builder.Configuration.GetSection(SecurityHeadersOptions.SectionName));

// Add MVC services (required for ValidateAntiForgeryToken attribute)
builder.Services.AddControllersWithViews(options =>
{
    options.CacheProfiles.Add("AuthorizationNoStore", new CacheProfile
    {
        NoStore = true,
        Location = ResponseCacheLocation.None
    });
});

// Add antiforgery services for API endpoints
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string SmartScheme = "LmsSmart";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = SmartScheme;
        options.DefaultAuthenticateScheme = SmartScheme;
        options.DefaultChallengeScheme = SmartScheme;
    })
    // Policy scheme decides whether to use Bearer (for token exchange) or LmsSession (everywhere else)
    .AddPolicyScheme(SmartScheme, "LMS smart scheme", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var path = context.Request.Path;
            if (path.StartsWithSegments("/api/auth/exchange-token"))
            {
                return JwtBearerDefaults.AuthenticationScheme;
            }
            return SessionAuthenticationDefaults.AuthenticationScheme;
        };
    })
    .AddLmsSession()
    // Configure Microsoft Identity Web API with explicit JwtBearer events for diagnostics
    .AddMicrosoftIdentityWebApi(jwtOptions =>
    {
        builder.Configuration.Bind("EntraId", jwtOptions);
        jwtOptions.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.HttpContext.Request.Path.StartsWithSegments("/api/auth/exchange-token"))
                {
                    var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearerDiagnostics");
                    logger.LogInformation("[JWT] Incoming Authorization header present: {HasAuth} Length: {Length}",
                        !string.IsNullOrEmpty(authHeader), authHeader?.Length ?? 0);
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (context.HttpContext.Request.Path.StartsWithSegments("/api/auth/exchange-token"))
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearerDiagnostics");
                    logger.LogWarning(context.Exception, "[JWT] Authentication failed for token exchange. Message: {Message}", context.Exception.Message);
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                if (context.HttpContext.Request.Path.StartsWithSegments("/api/auth/exchange-token"))
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearerDiagnostics");
                    logger.LogWarning("[JWT] Challenge issued for token exchange endpoint. Error: {Error} Description: {Description}",
                        context.Error, context.ErrorDescription);
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                if (context.HttpContext.Request.Path.StartsWithSegments("/api/auth/exchange-token"))
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearerDiagnostics");
                    var subject = context.Principal?.Identity?.Name ?? "<no-name>";
                    logger.LogInformation("[JWT] Token validated for token exchange. Subject: {Subject}. Claims count: {Claims}",
                        subject, context.Principal?.Claims?.Count() ?? 0);
                }
                return Task.CompletedTask;
            }
        };
    }, appOptions =>
    {
        builder.Configuration.Bind("EntraId", appOptions);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BearerOnly", policy =>
    {
        policy.AuthenticationSchemes.Clear();
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });

    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(SessionAuthenticationDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebFrontend", policy =>
    {
        var webOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "https://localhost:7064", "http://localhost:5143", "https://localhost:7002" };

        policy.WithOrigins(webOrigins)
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.GlobalLimiter = PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));

    options.AddPolicy("authentication", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }));

    options.AddPolicy("district-mutations", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }));

    options.AddPolicy("admin-invitations", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }));
});

builder.Logging.AddFilter("NorthStarET.NextGen.Lms.Application.Authorization.Services.AuthorizationService", LogLevel.Information);

var app = builder.Build();

var securityHeadersOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecurityHeadersOptions>>().Value;

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

    if (!context.Request.Path.StartsWithSegments("/swagger"))
    {
        context.Response.Headers.Append("Content-Security-Policy", securityHeadersOptions.ContentSecurityPolicy);
    }

    await next();
});

app.UseHttpsRedirection();
app.UseCors("AllowWebFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();
app.Run();
