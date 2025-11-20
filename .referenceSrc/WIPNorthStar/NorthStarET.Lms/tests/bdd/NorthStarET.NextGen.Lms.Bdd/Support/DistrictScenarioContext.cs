using System;
using System.Collections.Generic;
using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.CreateDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.DeleteDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.UpdateDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Queries.GetDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Queries.ListDistricts;
using NorthStarET.NextGen.Lms.Contracts.Districts;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.Districts;
using NorthStarET.NextGen.Lms.Infrastructure.Idempotency;

namespace NorthStarET.NextGen.Lms.Bdd.Support;

/// <summary>
/// Wiring hub for district BDD scenarios. Manages DI container, mediator, repository, audit sink, and captured results.
/// </summary>
public sealed class DistrictScenarioContext : IDisposable
{
    private readonly ServiceProvider _provider;

    public DistrictScenarioContext()
    {
        var services = new ServiceCollection();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateDistrictCommand).Assembly));
        services.AddValidatorsFromAssembly(typeof(CreateDistrictCommand).Assembly);
        services.AddSingleton<TestClock>();
        services.AddSingleton<IDateTimeProvider>(sp => sp.GetRequiredService<TestClock>());
        services.AddSingleton<FakeIdempotencyService>();
        services.AddSingleton<IIdempotencyService>(sp => sp.GetRequiredService<FakeIdempotencyService>());
        services.AddSingleton<TestAuditSink>();
        services.AddSingleton<TestCurrentUserService>();
        services.AddSingleton<InMemoryDistrictRepository>();
        services.AddSingleton<IDistrictRepository>(sp => sp.GetRequiredService<InMemoryDistrictRepository>());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TestValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TestIdempotencyBehavior<,>));

        _provider = services.BuildServiceProvider();

        Mediator = _provider.GetRequiredService<IMediator>();
        Repository = _provider.GetRequiredService<InMemoryDistrictRepository>();
        IdempotencyService = _provider.GetRequiredService<FakeIdempotencyService>();
        AuditSink = _provider.GetRequiredService<TestAuditSink>();
        CurrentUser = _provider.GetRequiredService<TestCurrentUserService>();
        Clock = _provider.GetRequiredService<TestClock>();
    }

    public IMediator Mediator { get; }
    public InMemoryDistrictRepository Repository { get; }
    public FakeIdempotencyService IdempotencyService { get; }
    public TestAuditSink AuditSink { get; }
    public TestCurrentUserService CurrentUser { get; }
    public TestClock Clock { get; }

    public Result<CreateDistrictResponse>? LastCreateResult { get; set; }
    public Result? LastResult { get; set; }
    public Result<DistrictResponse>? LastDistrictResult { get; set; }
    public Result<PagedResult<DistrictSummaryResponse>>? LastListResult { get; set; }
    public Exception? LastException { get; set; }
    public Guid? LastDistrictId { get; set; }
    public Dictionary<string, string>? LastDistrictPayload { get; set; }
    public Dictionary<string, string>? PreviousDistrictPayload { get; set; }
    public int DistrictCountBeforeCommand { get; set; }
    public int AuditCountBeforeCommand { get; set; }

    public void ResetOutcome()
    {
        LastCreateResult = null;
        LastResult = null;
        LastDistrictResult = null;
        LastListResult = null;
        LastException = null;
        DistrictCountBeforeCommand = Repository.Districts.Count;
        AuditCountBeforeCommand = AuditSink.Records.Count;
    }

    public void CaptureAuditIfSuccessful(object request, object response)
    {
        if (request is not IAuditableCommand auditable)
        {
            return;
        }

        if (request is IIdempotentCommand && IdempotencyService.LastInvocationUsedCachedResult)
        {
            return;
        }

        var isSuccess = response switch
        {
            Result result => result.IsSuccess,
            { } r when r.GetType().IsGenericType && r.GetType().GetGenericTypeDefinition() == typeof(Result<>)
                => (bool)r.GetType().GetProperty("IsSuccess", BindingFlags.Public | BindingFlags.Instance)!
                    .GetValue(r)!,
            _ => false
        };

        if (isSuccess)
        {
            AuditSink.Capture(auditable);
        }
    }

    public void Dispose()
    {
        _provider.Dispose();
    }
}
