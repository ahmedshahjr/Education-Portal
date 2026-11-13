using EducationPortal.Web.Jobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace EducationPortal.Application.FunctionalTests.Hosting;

/// <summary>The API schedules the lifecycle job with Quartz.NET on the configured UTC cron, and not at all when disabled (ADR-015).</summary>
public class SubscriptionLifecycleSchedulingTests : TestBase
{
    // A schedule that never fires during the test.
    private const string FarFuture = "0 0 1 1 1 ? 2099";

    [Test]
    public async Task ShouldScheduleTheJobDailyInUtcWhenEnabled()
    {
        await using var factory = new ProductionWebApiFactory(FunctionalTestSetup.ConnectionString)
            .WithWebHostBuilder(builder => builder
                .UseSetting("SubscriptionLifecycle:Enabled", "true")
                .UseSetting("SubscriptionLifecycle:Schedule", FarFuture));
        using var _ = factory.CreateClient();

        var scheduler = await factory.Services.GetRequiredService<ISchedulerFactory>().GetScheduler();

        var trigger = (await scheduler.GetTrigger(SubscriptionLifecycleJob.DailyTrigger)).ShouldBeAssignableTo<ICronTrigger>()!;
        trigger.ShouldSatisfyAllConditions(
            t => t.JobKey.ShouldBe(SubscriptionLifecycleJob.Key),
            t => t.CronExpressionString.ShouldBe(FarFuture),
            t => t.TimeZone.ShouldBe(TimeZoneInfo.Utc));
    }

    [Test]
    public async Task ShouldNotScheduleTheJobWhenDisabled()
    {
        await using var factory = new ProductionWebApiFactory(FunctionalTestSetup.ConnectionString);
        using var _ = factory.CreateClient();

        var scheduler = await factory.Services.GetRequiredService<ISchedulerFactory>().GetScheduler();

        (await scheduler.GetJobDetail(SubscriptionLifecycleJob.Key)).ShouldBeNull();
    }
}
