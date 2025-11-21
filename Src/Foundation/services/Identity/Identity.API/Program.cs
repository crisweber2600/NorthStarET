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
// TODO: Configure Microsoft.Identity.Web when Entra ID settings are available

// Add controllers
builder.Services.AddControllers();

// Add API documentation
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Map service defaults (health checks, etc.)
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Enable developer exception page in development
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// TODO: Add authentication middleware when configured
// app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
