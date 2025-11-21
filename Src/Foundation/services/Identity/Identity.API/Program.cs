using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using NorthStarET.Foundation.Identity.API.Middleware;
using NorthStarET.Foundation.Identity.Application;
using NorthStarET.Foundation.Identity.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults (observability, health checks, etc.)
builder.AddServiceDefaults();

// Add Identity Application layer
builder.Services.AddIdentityApplication();

// Add Identity Infrastructure layer
builder.Services.AddIdentityInfrastructure(builder.Configuration);

// Add Microsoft Identity Web for Entra ID authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Add authorization
builder.Services.AddAuthorization();

// Add controllers
builder.Services.AddControllers();

// Add API documentation
builder.Services.AddEndpointsApiExplorer();

// Add CORS (configure as needed)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Map service defaults (health checks, etc.)
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Enable developer exception page in development
    app.UseDeveloperExceptionPage();
}

// Add global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors();

// Add authentication middleware
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
