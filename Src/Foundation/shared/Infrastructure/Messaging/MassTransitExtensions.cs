using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace NorthStarET.Foundation.Infrastructure.Messaging;

/// <summary>
/// Extension methods for configuring MassTransit with RabbitMQ
/// </summary>
public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransitWithRabbitMq(
        this IServiceCollection services,
        string connectionString,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        services.AddMassTransit(busConfig =>
        {
            // Allow custom configuration (e.g., adding consumers)
            configure?.Invoke(busConfig);

            busConfig.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(connectionString);

                // Configure retry policy
                cfg.UseMessageRetry(retry =>
                {
                    retry.Exponential(
                        retryLimit: 3,
                        minInterval: TimeSpan.FromSeconds(1),
                        maxInterval: TimeSpan.FromSeconds(30),
                        intervalDelta: TimeSpan.FromSeconds(5));
                });

                // Configure circuit breaker
                cfg.UseCircuitBreaker(cb =>
                {
                    cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                    cb.TripThreshold = 5;
                    cb.ActiveThreshold = 10;
                    cb.ResetInterval = TimeSpan.FromSeconds(30);
                });

                // Configure dead-letter queue
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
