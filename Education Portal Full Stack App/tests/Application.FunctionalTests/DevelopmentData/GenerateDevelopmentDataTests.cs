using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.DevelopmentData.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Infrastructure.Identity;
using EducationPortal.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.FunctionalTests.DevelopmentData;

/// <summary>
/// The Development seed creates schools that can be signed in to: each generated school gets an administrator with the
/// default Development email on its own domain and the default Development password (SEC-SECRET-04).
/// </summary>
public class GenerateDevelopmentDataTests : TestBase
{
    [SetUp]
    public async Task SetUpPricedCountry()
    {
        // Creates the Administrator role that each school's first administrator is given (the seeder does this at start-up).
        await TestApp.RunAsAdministratorAsync();

        // Registration gives each school a trial priced from its country's price book, so the country needs one.
        await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "ur", "Asia/Karachi", "PKR");

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        TestApp.SetTenant(scope.ServiceProvider, TestApp.Resolve(TestApp.MasterSchoolId, isMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var plan = Plan.Create(PricingCatalog.Full, "Full", "");
        context.Plans.Add(plan);
        await context.SaveChangesAsync();

        var book = PriceBook.Create("PK", "PKR");
        context.PriceBooks.Add(book);
        await context.SaveChangesAsync();

        context.PriceBookItems.Add(PriceBookItem.ForPlan(book, plan, BillingCycle.Monthly, 30000));
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task ShouldCreateAnAdministratorWithTheDefaultEmailAndPasswordForEachGeneratedSchool()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        TestApp.SetTenant(scope.ServiceProvider, TestApp.Resolve(TestApp.MasterSchoolId, isMaster: true));

        var summary = await scope.ServiceProvider.GetRequiredService<IDevelopmentDataGenerator>()
            .GenerateAsync(new DevelopmentDataRequest(Schools: 2, CampusesPerSchool: 1, PeoplePerCampus: 0, CountryCode: "PK", Seed: 42), CancellationToken.None);

        summary.Schools.ShouldBe(2);

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var schools = await context.Schools.Where(s => !s.IsMaster && s.Domain != null).ToListAsync();
        schools.Count.ShouldBe(2);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var school in schools)
        {
            var administrator = await userManager.FindByNameAsync($"admin@{school.Domain}");

            administrator.ShouldNotBeNull();
            administrator.SchoolId.ShouldBe(school.Id);
            administrator.Email.ShouldBe($"admin@{school.Domain}");
            (await userManager.CheckPasswordAsync(administrator, DevelopmentDataSeeder.DevelopmentPassword)).ShouldBeTrue();
        }
    }
}
