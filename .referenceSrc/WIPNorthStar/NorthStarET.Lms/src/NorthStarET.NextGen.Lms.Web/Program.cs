using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using NorthStarET.NextGen.Lms.Web.Authentication;
using NorthStarET.NextGen.Lms.Web.Services;
using NorthStarET.NextGen.Lms.Web.Testing;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var useTestAuth = TestEnvironment.IsTestAuthEnabled(builder.Configuration);

if (useTestAuth)
{
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = TestAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = TestAuthenticationDefaults.AuthenticationScheme;
        })
        .AddTestAuthentication();

    builder.Services.AddAuthorization(options =>
    {
        var policy = new AuthorizationPolicyBuilder(TestAuthenticationDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();

        options.DefaultPolicy = policy;
        options.FallbackPolicy = policy;
    });
}
else
{
    builder.Services
        .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("EntraId"))
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddDownstreamApi("LmsApi", builder.Configuration.GetSection("DownstreamApi"))
        .AddDistributedTokenCaches();

    // Configure OIDC events for LMS token exchange using PostConfigure to ensure it runs after AddMicrosoftIdentityWebApp
    builder.Services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.ConfigureLmsTokenExchange();
    });

    builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = options.DefaultPolicy;
    });
}

var mvcBuilder = builder.Services.AddControllersWithViews();

if (!useTestAuth)
{
    mvcBuilder.AddMicrosoftIdentityUI();
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IPlaywrightStubStore, PlaywrightStubStore>();

// Register LMS session services
builder.Services.AddScoped<ILmsSessionAccessor, LmsSessionAccessor>();
builder.Services.AddScoped<ITokenExchangeService, TokenExchangeService>();
builder.Services.AddTransient<LmsSessionHandler>();

// Configure HttpClients to use Aspire service discovery
// When running under Aspire AppHost, the service name "northstaret-nextgen-lms-api" 
// will be resolved automatically via service discovery
builder.Services.AddHttpClient<UserContextClient>(client =>
{
    // In Aspire, use the service name with http:// or https:// scheme
    // Service discovery will resolve this to the actual endpoint
    client.BaseAddress = new Uri("https+http://northstaret-nextgen-lms-api");
})
.AddHttpMessageHandler<LmsSessionHandler>() // Add session header handler
.AddServiceDiscovery(); // Enable Aspire service discovery

builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://northstaret-nextgen-lms-api");
})
.AddHttpMessageHandler<LmsSessionHandler>() // Add session header handler
.AddServiceDiscovery(); // Enable Aspire service discovery

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.MapDefaultEndpoints();

app.Run();
