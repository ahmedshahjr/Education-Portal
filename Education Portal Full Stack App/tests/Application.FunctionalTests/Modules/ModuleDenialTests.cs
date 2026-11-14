using System.Text.Json;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Services;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.FunctionalTests.Modules;

/// <summary>
/// A paid module's use case is refused with 403 MODULE_NOT_ENABLED when the school's subscription does not give it the
/// module now (SEC-AUTHZ-01, SEC-AUTHZ-09). No paid module has an endpoint yet, so the test sends
/// <see cref="FeesModuleProbe"/>, a test-only use case of the Fees module, through the real MediatR pipeline with the
/// school's tenant and entitlements from the database, and turns the refusal into a response with the real exception
/// handler.
/// </summary>
public class ModuleDenialTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";

    [Test]
    public async Task ShouldRefuseAModuleTheSchoolHasNotBoughtWith403AndItsCode()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await RunAsSchoolAdministratorAsync(school.Id);

        var refused = await Should.ThrowAsync<ModuleNotEnabledException>(() => TestApp.SendAsync(new FeesModuleProbe()));

        refused.ModuleCode.ShouldBe(ModuleCodes.Fees);
        var (status, body) = await ToResponseAsync(refused);
        status.ShouldBe(StatusCodes.Status403Forbidden);
        body.GetProperty("code").GetString().ShouldBe(ModuleNotEnabledException.ErrorCode);
        body.GetProperty("module").GetString().ShouldBe(ModuleCodes.Fees);
    }

    [Test]
    public async Task ShouldRefuseAModuleWhoseGrantHasExpired()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees], start: DateTimeOffset.UtcNow.AddMonths(-2)); // ended a month ago
        await RunAsSchoolAdministratorAsync(school.Id);

        await Should.ThrowAsync<ModuleNotEnabledException>(() => TestApp.SendAsync(new FeesModuleProbe()));
    }

    [Test]
    public async Task ShouldRefuseAModuleWhileTheSubscriptionIsSuspended()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var subscription = await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees]);
        await SuspendAsync(subscription.PublicId, school.Id);
        await RunAsSchoolAdministratorAsync(school.Id);

        await Should.ThrowAsync<ModuleNotEnabledException>(() => TestApp.SendAsync(new FeesModuleProbe()));
    }

    [Test]
    public async Task ShouldRefuseAModuleGrantedToOneBranchWhenNoBranchIsKnown()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var branch = Branch.Create(school.Id, "North", "NORTH");
        await TestApp.AddAsync(branch);
        await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees], branchId: branch.Id);
        await RunAsSchoolAdministratorAsync(school.Id);

        // A branch-only grant counts only for that branch; the pipeline knows no branch, so it fails closed.
        await Should.ThrowAsync<ModuleNotEnabledException>(() => TestApp.SendAsync(new FeesModuleProbe()));
    }

    [Test]
    public async Task ShouldRefuseOnlyTheSchoolWithoutTheModule()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var other = await TestApp.AddSchoolAsync("School Two", "school2.localhost");
        await TestApp.SubscribeAsync(other.Id, [ModuleCodes.Fees]);
        await RunAsSchoolAdministratorAsync(school.Id);

        await Should.ThrowAsync<ModuleNotEnabledException>(() => TestApp.SendAsync(new FeesModuleProbe()));
    }

    [Test]
    public async Task ShouldAllowTheModuleWhileAnActiveGrantCoversNow()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees]);
        await RunAsSchoolAdministratorAsync(school.Id);

        (await TestApp.SendAsync(new FeesModuleProbe())).ShouldBe(FeesModuleProbe.Allowed);
    }

    [Test]
    public async Task ShouldNeverLimitTheMasterTenant()
    {
        await TestApp.RunAsAdministratorAsync();

        (await TestApp.SendAsync(new FeesModuleProbe())).ShouldBe(FeesModuleProbe.Allowed);
    }

    /// <summary>Suspends a subscription and its modules as the master would, then drops the school's cached entitlements.</summary>
    private static async Task SuspendAsync(Guid subscriptionId, int schoolId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var subscription = await context.Subscriptions.Include(s => s.Modules).SingleAsync(s => s.PublicId == subscriptionId);
        subscription.Suspend();
        await context.SaveChangesAsync();

        scope.ServiceProvider.GetRequiredService<IModuleEntitlementStore>().Invalidate(schoolId);
    }

    private static Task RunAsSchoolAdministratorAsync(int schoolId) =>
        TestApp.RunAsUserAsync(schoolId, "admin@school1.local", TestApp.AdministratorPassword, [Roles.Administrator]);

    private static async Task<(int Status, JsonElement Body)> ToResponseAsync(Exception exception)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        context.Response.Body = new MemoryStream();

        var handled = false;
        foreach (var handler in scope.ServiceProvider.GetServices<IExceptionHandler>())
        {
            if (await handler.TryHandleAsync(context, exception, CancellationToken.None))
            {
                handled = true;
                break;
            }
        }

        handled.ShouldBeTrue();
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        return (context.Response.StatusCode, document.RootElement.Clone());
    }
}

/// <summary>A use case of the paid Fees module, only for tests: it answers <see cref="Allowed"/> once the pipeline lets it through.</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Fees)]
public sealed record FeesModuleProbe : IRequest<string>
{
    public const string Allowed = "allowed";
}

public sealed class FeesModuleProbeHandler : IRequestHandler<FeesModuleProbe, string>
{
    public Task<string> Handle(FeesModuleProbe request, CancellationToken cancellationToken) => Task.FromResult(FeesModuleProbe.Allowed);
}
