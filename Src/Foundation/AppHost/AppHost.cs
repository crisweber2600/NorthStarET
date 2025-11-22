var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL resource
var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent);

// Add Identity database
var identityDb = postgres.AddDatabase("IdentityDb");

// Add Redis resource
var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);

// Add RabbitMQ resource
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithLifetime(ContainerLifetime.Persistent);

// Add Identity Service
builder.AddProject<Projects.Identity_API>("identity-api")
    .WithReference(identityDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WaitFor(identityDb)
    .WaitFor(redis)
    .WaitFor(rabbitmq);

builder.Build().Run();
