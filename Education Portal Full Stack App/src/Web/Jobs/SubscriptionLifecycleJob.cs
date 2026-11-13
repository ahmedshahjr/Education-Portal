using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Subscriptions.Lifecycle;
using EducationPortal.Web.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace EducationPortal.Web.Jobs;

/// <summary>
/// The daily subscription lifecycle (ADR-015), scheduled by Quartz.NET at <c>SubscriptionLifecycle:Schedule</c> (UTC).
/// There is no request, so the job opens its own scope (ENG-EF-02) and sets the master tenant on it: only the master
/// tenant manages subscriptions (SEC-AUTHZ-07), and the normal tenant query filters stay on (SEC-AUTHZ-08). One run at a
/// time in this process; a second run, here or on another instance, changes nothing (the rules are idempotent).
/// </summary>
[DisallowConcurrentExecution]
public sealed class SubscriptionLifecycleJob : IJob
{
    public static readonly JobKey Key = new("subscription-lifecycle");

    public static readonly TriggerKey DailyTrigger = new("subscription-lifecycle-daily");

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SubscriptionLifecycleJob> _logger;

    public SubscriptionLifecycleJob(IServiceScopeFactory scopeFactory, TimeProvider timeProvider, ILogger<SubscriptionLifecycleJob> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await RunAsMasterAsync(_scopeFactory, _timeProvider.GetUtcNow(), cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Logged and handed to Quartz, which records the failure; nothing was saved, and the next run tries again.
            _logger.LogError(exception, "Subscription lifecycle run failed");
            throw new JobExecutionException(exception);
        }
    }

    /// <summary>Runs the lifecycle once, at <paramref name="now"/>, in a new scope that acts as the master tenant.</summary>
    public static async Task<SubscriptionLifecycleResult?> RunAsMasterAsync(IServiceScopeFactory scopeFactory, DateTimeOffset now, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);

        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;

        var masterId = await services.GetRequiredService<IApplicationDbContext>().Schools
            .Where(s => s.IsMaster)
            .Select(s => (int?)s.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (masterId is null)
        {
            // No master tenant yet (an empty database): nothing may manage subscriptions, so do nothing (fail closed).
            services.GetRequiredService<ILogger<SubscriptionLifecycleJob>>().LogWarning("Subscription lifecycle skipped: there is no master tenant");
            return null;
        }

        services.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(masterId.Value, IsMaster: true));

        return await services.GetRequiredService<SubscriptionLifecycle>().RunAsync(now, cancellationToken);
    }
}
