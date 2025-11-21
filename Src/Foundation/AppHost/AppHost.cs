var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL resource
var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent);

// Add Redis resource
var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);

// Add RabbitMQ resource
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithLifetime(ContainerLifetime.Persistent);

builder.Build().Run();
