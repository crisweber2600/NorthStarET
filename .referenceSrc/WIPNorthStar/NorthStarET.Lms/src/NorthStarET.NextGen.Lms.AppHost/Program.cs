using System;

var builder = DistributedApplication.CreateBuilder(args);

static bool UseEphemeralStorage()
    => string.Equals(
        Environment.GetEnvironmentVariable("ASPIRE_EPHEMERAL_STORAGE"),
        "true",
        StringComparison.OrdinalIgnoreCase);

var identityPostgresBuilder = builder.AddPostgres("identity-postgres");

if (!UseEphemeralStorage())
{
    identityPostgresBuilder = identityPostgresBuilder.WithDataVolume();
}

var identityPostgres = identityPostgresBuilder.AddDatabase("identity-db");

// Redis Stack for session caching (feature 001) and idempotency tokens (feature 002)
var identityRedis = builder.AddRedis("identity-redis");

// TODO: Azure Event Grid emulator/mock for local development (feature 002)
// For production, Event Grid publisher will be configured via Azure.Messaging.EventGrid client in Infrastructure layer
// Local development options:
//   1. Use Azure Event Grid simulator/emulator (if available)
//   2. Mock event publishing for local dev (log events without actual publishing)
//   3. Connect to a development Event Grid topic in Azure
// Configuration will be added to appsettings.Development.json with EventGrid:Endpoint and EventGrid:TopicKey

var api = builder
    .AddProject<Projects.NorthStarET_NextGen_Lms_Api>("northstaret-nextgen-lms-api")
    .WithReference(identityPostgres)
    .WithReference(identityRedis)
    .WaitFor(identityPostgres)
    .WaitFor(identityRedis);

var web = builder
    .AddProject<Projects.NorthStarET_NextGen_Lms_Web>("northstaret-nextgen-lms-web")
    .WithReference(api)
    .WithReference(identityRedis)
    .WaitFor(api)
    .WaitFor(identityRedis);

builder.Build().Run();
