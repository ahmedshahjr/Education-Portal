using EducationPortal.Infrastructure.Data;
using EducationPortal.Infrastructure.Data.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Seeding;

/// <summary>
/// The development seed gives the Pakistan and UAE demo schools campuses with their full details (province, city, phones and
/// place on the map), and running the seed again changes nothing. The campus screens show these rows.
/// </summary>
public class DemoCampusSeedTests : TestBase
{
    [Test]
    public async Task ShouldGiveTheDemoSchoolsTheirCampusesWithFullDetailsOnceOnly()
    {
        await ImportLocationsAsync();
        await SeedDemoDataAsync();
        await SeedDemoDataAsync();

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var karachi = await context.Campuses.IgnoreQueryFilters()
            .Include(c => c.State).Include(c => c.City).Include(c => c.Phones)
            .SingleAsync(c => c.Name == "Karachi Clifton");
        karachi.State!.Name.ShouldBe("Sindh");
        karachi.City!.Name.ShouldBe("Karachi");
        karachi.Phones.Count.ShouldBe(1);
        karachi.Latitude.ShouldBe(24.8138m);
        karachi.Longitude.ShouldBe(67.0300m);

        var lahore = await context.Campuses.IgnoreQueryFilters()
            .Include(c => c.Phones)
            .SingleAsync(c => c.Name == "Lahore");
        lahore.Phones.Count.ShouldBe(2);

        var dubai = await context.Campuses.IgnoreQueryFilters()
            .Include(c => c.State).Include(c => c.City)
            .SingleAsync(c => c.Name == "Dubai Al Barsha");
        dubai.State!.Name.ShouldBe("Dubai");
        dubai.City!.Name.ShouldBe("Al Barsha");

        var mainCampuses = await context.Campuses.IgnoreQueryFilters()
            .Where(c => c.Kind == EducationPortal.Domain.Enums.CampusKind.Main && c.Address != null)
            .CountAsync();
        mainCampuses.ShouldBe(2);

        // The same campus codes are kept: one row per code per school.
        (await context.Campuses.IgnoreQueryFilters().CountAsync(c => c.Name == "Karachi Clifton")).ShouldBe(1);
    }

    private static async Task ImportLocationsAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        await scope.ServiceProvider.GetRequiredService<LocationDataImporter>().ImportAsync();
    }

    private static async Task SeedDemoDataAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>().SeedAsync();
    }
}
