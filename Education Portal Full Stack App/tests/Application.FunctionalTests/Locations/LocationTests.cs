using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.Locations.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Infrastructure.Data.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.FunctionalTests.Locations;

/// <summary>
/// The location data import (countries, states and cities from the Countries States Cities Database) and the read-only
/// dropdown endpoints. The import must be safe to run again and never change rows administrators added.
/// </summary>
public class LocationTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";
    private const string SchoolUserEmail = "staff@school1.local";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    [Test]
    public async Task ShouldImportTheFiveCountriesAndPakistansProvinces()
    {
        var results = await ImportAsync();

        results.Select(r => r.CountryCode).ShouldBe(["SA", "AE", "US", "JP", "PK"]);
        (await TestApp.CountAsync<Country>()).ShouldBe(5);
        (await TestApp.CountAsync<State>()).ShouldBe(results.Sum(r => r.StatesAdded));
        (await TestApp.CountAsync<City>()).ShouldBe(results.Sum(r => r.CitiesAdded));

        var pakistan = await QueryAsync(db => db.Countries.SingleAsync(c => c.Code == "PK"));
        pakistan.Iso3.ShouldBe("PAK");
        pakistan.PhoneCode.ShouldBe("92");
        pakistan.CurrencyCode.ShouldBe("PKR");
        pakistan.Languages.ShouldBe(["en", "ur"]);
        pakistan.NativeName.ShouldNotBeNullOrWhiteSpace();

        var provinces = await QueryAsync(db => db.States.Where(s => s.CountryId == pakistan.Id).OrderBy(s => s.IsoCode).ToListAsync());
        provinces.Select(s => (s.IsoCode, s.Name, s.Type)).ShouldBe(
        [
            ("PK-BA", "Balochistan", StateType.Province),
            ("PK-GB", "Gilgit-Baltistan", StateType.AdministeredArea),
            ("PK-IS", "Islamabad Capital Territory", StateType.FederalCapitalTerritory),
            ("PK-JK", "Azad Jammu and Kashmir", StateType.AdministeredArea),
            ("PK-KP", "Khyber Pakhtunkhwa", StateType.Province),
            ("PK-PB", "Punjab", StateType.Province),
            ("PK-SD", "Sindh", StateType.Province),
        ]);

        // The three US military postal regions have no ISO code and are left out; DC and the outlying areas stay.
        var usStates = results.Single(r => r.CountryCode == "US").StatesAdded;
        usStates.ShouldBe(57);
    }

    [Test]
    public async Task ShouldChangeNothingWhenRunAgain()
    {
        await ImportAsync();
        var states = await TestApp.CountAsync<State>();
        var cities = await TestApp.CountAsync<City>();

        var again = await ImportAsync();

        again.ShouldAllBe(r => r.StatesAdded == 0 && r.StatesUpdated == 0 && r.CitiesAdded == 0 && r.CitiesUpdated == 0);
        (await TestApp.CountAsync<Country>()).ShouldBe(5);
        (await TestApp.CountAsync<State>()).ShouldBe(states);
        (await TestApp.CountAsync<City>()).ShouldBe(cities);
    }

    [Test]
    public async Task ShouldRestoreDatasetRowsButNeverTouchCustomOnes()
    {
        await ImportAsync();
        var (sindhId, punjabId) = await QueryAsync(async db => (
            await db.States.Where(s => s.IsoCode == "PK-SD").Select(s => s.Id).SingleAsync(),
            await db.States.Where(s => s.IsoCode == "PK-PB").Select(s => s.Id).SingleAsync()));

        // Someone changed dataset rows by hand; an administrator added a missing town, and replaced a dataset city with
        // their own (custom) row of the same name.
        await QueryAsync(db => db.States.Where(s => s.Id == sindhId).ExecuteUpdateAsync(u => u.SetProperty(s => s.Name, "Changed by hand")));
        await QueryAsync(db => db.Cities.Where(c => c.StateId == sindhId && c.Name == "Karachi").ExecuteUpdateAsync(u => u.SetProperty(c => c.Latitude, 1m)));
        var hyderabad = await QueryAsync(db => db.Cities.SingleAsync(c => c.StateId == sindhId && c.Name == "Hyderabad"));
        await QueryAsync(db => db.Cities.Where(c => c.Id == hyderabad.Id).ExecuteDeleteAsync());
        var customHyderabad = City.CreateCustom(sindhId, "Hyderabad", "حیدرآباد", 10m, 20m);
        var customTown = City.CreateCustom(punjabId, "Chak 45", null, null, null);
        var customState = State.CreateCustom((await QueryAsync(db => db.Countries.SingleAsync(c => c.Code == "PK"))).Id, "Test Region", null, StateType.Region);
        await TestApp.AddAsync(customHyderabad);
        await TestApp.AddAsync(customTown);
        await TestApp.AddAsync(customState);

        var results = await ImportAsync();

        var pk = results.Single(r => r.CountryCode == "PK");
        pk.StatesUpdated.ShouldBe(1);
        pk.CitiesUpdated.ShouldBe(1);
        pk.CitiesSkipped.ShouldBe(1);
        pk.CitiesAdded.ShouldBe(0);

        (await QueryAsync(db => db.States.SingleAsync(s => s.Id == sindhId))).Name.ShouldBe("Sindh");
        (await QueryAsync(db => db.Cities.SingleAsync(c => c.StateId == sindhId && c.Name == "Karachi"))).Latitude.ShouldNotBe(1m);

        var keptHyderabad = await QueryAsync(db => db.Cities.SingleAsync(c => c.Id == customHyderabad.Id));
        keptHyderabad.IsCustom.ShouldBeTrue();
        keptHyderabad.Latitude.ShouldBe(10m);
        keptHyderabad.NativeName.ShouldBe("حیدرآباد");
        (await QueryAsync(db => db.Cities.CountAsync(c => c.StateId == sindhId && c.Name == "Hyderabad"))).ShouldBe(1);
        (await QueryAsync(db => db.Cities.AnyAsync(c => c.Id == customTown.Id && c.IsCustom))).ShouldBeTrue();
        (await QueryAsync(db => db.States.SingleAsync(s => s.Id == customState.Id))).Name.ShouldBe("Test Region");
    }

    [Test]
    public async Task ShouldKeepTheMasterTenantsOwnCountrySettings()
    {
        var existing = await TestApp.AddCountryAsync("PK", "Pakistan (master's name)", ["ur"], "ur", "Asia/Karachi", "PKR");

        await ImportAsync();

        var pakistan = await QueryAsync(db => db.Countries.SingleAsync(c => c.Code == "PK"));
        pakistan.Id.ShouldBe(existing.Id);
        pakistan.Name.ShouldBe("Pakistan (master's name)");
        pakistan.Languages.ShouldBe(["ur"]);
        pakistan.Iso3.ShouldBe("PAK");
        (await TestApp.CountAsync<Country>()).ShouldBe(5);
    }

    [Test]
    public async Task ShouldServeTheCascadingDropdownsToASignedInSchoolUser()
    {
        await ImportAsync();
        using var session = await SignInAsSchoolUserAsync();

        var countries = await ReadAsync<List<LocationCountryDto>>(session, "/api/Locations/countries");
        countries.Count.ShouldBe(5);
        countries.Select(c => c.Name).ShouldBe(countries.Select(c => c.Name).OrderBy(n => n, StringComparer.Ordinal));
        var pakistan = countries.Single(c => c.Code == "PK");
        pakistan.Iso3.ShouldBe("PAK");
        pakistan.PhoneCode.ShouldBe("92");

        var states = await ReadAsync<List<StateDto>>(session, $"/api/Locations/countries/{pakistan.Id}/states");
        states.Count.ShouldBe(7);
        var islamabad = states.Single(s => s.IsoCode == "PK-IS");
        islamabad.Name.ShouldBe("Islamabad Capital Territory");
        islamabad.Type.ShouldBe(StateType.FederalCapitalTerritory);

        var cities = await ReadAsync<List<CityDto>>(session, $"/api/Locations/states/{islamabad.Id}/cities");
        cities.ShouldNotBeEmpty();
        cities.ShouldAllBe(c => c.Latitude != null && c.Longitude != null);

        // Enums travel by name (ENG-ENUM-02).
        using var raw = await session.GetAsync($"/api/Locations/countries/{pakistan.Id}/states");
        (await raw.Content.ReadAsStringAsync()).ShouldContain("\"type\":\"Province\"");
    }

    [Test]
    public async Task ShouldHideInactiveStatesAndCities()
    {
        await ImportAsync();
        await QueryAsync(db => db.States.Where(s => s.IsoCode == "PK-GB").ExecuteUpdateAsync(u => u.SetProperty(s => s.IsActive, false)));
        var sindh = await QueryAsync(db => db.States.SingleAsync(s => s.IsoCode == "PK-SD"));
        await QueryAsync(db => db.Cities.Where(c => c.StateId == sindh.Id && c.Name == "Karachi").ExecuteUpdateAsync(u => u.SetProperty(c => c.IsActive, false)));
        var gilgit = await QueryAsync(db => db.States.SingleAsync(s => s.IsoCode == "PK-GB"));
        var pakistan = await QueryAsync(db => db.Countries.SingleAsync(c => c.Code == "PK"));
        using var session = await SignInAsSchoolUserAsync();

        var states = await ReadAsync<List<StateDto>>(session, $"/api/Locations/countries/{pakistan.PublicId}/states");
        var cities = await ReadAsync<List<CityDto>>(session, $"/api/Locations/states/{sindh.PublicId}/cities");
        using var gilgitCities = await session.GetAsync($"/api/Locations/states/{gilgit.PublicId}/cities");

        states.ShouldNotContain(s => s.IsoCode == "PK-GB");
        cities.ShouldNotContain(c => c.Name == "Karachi");
        gilgitCities.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldAnswer404ForAnUnknownCountryOrState()
    {
        await ImportAsync();
        using var session = await SignInAsSchoolUserAsync();

        using var states = await session.GetAsync($"/api/Locations/countries/{Guid.NewGuid()}/states");
        using var cities = await session.GetAsync($"/api/Locations/states/{Guid.NewGuid()}/cities");
        using var emptyId = await session.GetAsync($"/api/Locations/countries/{Guid.Empty}/states");
        using var notAGuid = await session.GetAsync("/api/Locations/countries/1/states");

        states.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        cities.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        emptyId.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        notAGuid.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldRequireSignIn()
    {
        await ImportAsync();
        var pakistan = await QueryAsync(db => db.Countries.SingleAsync(c => c.Code == "PK"));
        var sindh = await QueryAsync(db => db.States.SingleAsync(s => s.IsoCode == "PK-SD"));
        await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var anonymous = TestHttp.CreateDevelopmentSession(SchoolDomain);

        using var countries = await anonymous.GetAsync("/api/Locations/countries");
        using var states = await anonymous.GetAsync($"/api/Locations/countries/{pakistan.PublicId}/states");
        using var cities = await anonymous.GetAsync($"/api/Locations/states/{sindh.PublicId}/cities");

        countries.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        states.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        cities.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldRefuseAnUnknownDomain()
    {
        await ImportAsync();
        using var session = TestHttp.CreateDevelopmentSession("unknown.localhost");

        using var countries = await session.GetAsync("/api/Locations/countries");

        countries.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static async Task<IReadOnlyList<LocationImportResult>> ImportAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<LocationDataImporter>().ImportAsync();
    }

    private static async Task<T> QueryAsync<T>(Func<ApplicationDbContext, Task<T>> query)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        return await query(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    private static async Task<HttpSession> SignInAsSchoolUserAsync()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsUserAsync(school.Id, SchoolUserEmail, TestApp.DefaultUserPassword, []);
        var session = TestHttp.CreateDevelopmentSession(SchoolDomain);
        using var response = await session.SignInAsync(SchoolUserEmail, TestApp.DefaultUserPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }

    private static async Task<T> ReadAsync<T>(HttpSession session, string url)
    {
        using var response = await session.GetAsync(url);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }
}
