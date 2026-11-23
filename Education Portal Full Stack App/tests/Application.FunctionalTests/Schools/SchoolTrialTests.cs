using System.Net;
using System.Net.Http.Json;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Application.Schools.Registration;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Schools;

/// <summary>
/// A new school starts a free trial of three months (90 days) on the Full plan, priced from its country's price book, in the
/// same save as the school and its first administrator (ADR-011 subscriptions, SEC-AUTHZ-07, SEC-LOG-03).
/// </summary>
public class SchoolTrialTests : TestBase
{
    private const string TrialCountry = "TR";
    private const string OneTimePassword = "violet harbour lantern 47";

    [Test]
    public async Task ShouldStartAThreeMonthTrialOnTheFullPlanForASchoolWhoseCountryHasPrices()
    {
        var countryId = await TestApp.AddCountryAsync(TrialCountry, "Turkey", ["en"], "en", "Europe/Istanbul", "TRY");
        await TestApp.RunAsAdministratorAsync();
        await EnsureFullPlanPricedAsync();

        var before = DateTimeOffset.UtcNow;
        using var master = await SignInAsMasterAsync();
        using var response = await master.PostAsync("/api/Schools", new
        {
            name = "Trial School",
            domain = "trial-school.localhost",
            countryId = countryId.PublicId,
            dataRegion = "SouthAsia",
            administratorEmail = "head@trial-school.local",
            administratorPassword = OneTimePassword,
        });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var schoolPublicId = await response.Content.ReadFromJsonAsync<Guid>();
        var schoolId = await TestApp.FindIdAsync<School>(schoolPublicId);

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        TestApp.SetTenant(scope.ServiceProvider, TestApp.Resolve(TestApp.MasterSchoolId, isMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var subscription = await context.Subscriptions.SingleAsync(s => s.SchoolId == schoolId);
        var fullPlanId = await context.Plans.Where(p => p.Code == PricingCatalog.Full).Select(p => p.Id).SingleAsync();

        subscription.Status.ShouldBe(SubscriptionStatus.Trial);
        subscription.PlanId.ShouldBe(fullPlanId);
        subscription.BillingCycle.ShouldBe(BillingCycle.Monthly);
        subscription.TrialEndDate.ShouldNotBeNull();
        subscription.TrialEndDate!.Value.ShouldBeGreaterThanOrEqualTo(before.AddDays(SchoolRegistration.TrialDays));
        subscription.TrialEndDate.Value.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow.AddDays(SchoolRegistration.TrialDays));
    }

    /// <summary>Makes sure the Full plan exists and has a monthly price in the test country's price book.</summary>
    private static async Task EnsureFullPlanPricedAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        TestApp.SetTenant(scope.ServiceProvider, TestApp.Resolve(TestApp.MasterSchoolId, isMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var plan = await context.Plans.FirstOrDefaultAsync(p => p.Code == PricingCatalog.Full);
        if (plan is null)
        {
            plan = Plan.Create(PricingCatalog.Full, "Full", "Every module.");
            context.Plans.Add(plan);
            await context.SaveChangesAsync();
        }

        var book = await context.PriceBooks.FirstOrDefaultAsync(b => b.CountryCode == TrialCountry);
        if (book is null)
        {
            book = PriceBook.Create(TrialCountry, "TRY");
            context.PriceBooks.Add(book);
            await context.SaveChangesAsync();
        }

        context.PriceBookItems.Add(PriceBookItem.ForPlan(book, plan, BillingCycle.Monthly, 1_000m));
        await context.SaveChangesAsync();
    }

    private static async Task<HttpSession> SignInAsMasterAsync()
    {
        var master = TestHttp.CreateDevelopmentSession();
        using var response = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return master;
    }
}
