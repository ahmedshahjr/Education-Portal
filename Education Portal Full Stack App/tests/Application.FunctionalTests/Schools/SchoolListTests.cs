using System.Net;
using System.Net.Http.Json;
using EducationPortal.Application.Schools.Queries.GetSchools;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.FunctionalTests.Schools;

/// <summary>
/// The master portal's Schools table can be searched by name and limited to one country. The filters run on the
/// server, so paging counts only matching schools; input is validated, and only master tenant administrators can
/// list schools at all (SEC-AUTHZ-07, SEC-INPUT-01, SEC-INPUT-02).
/// </summary>
public class SchoolListTests : TestBase
{
    private EntityIds _pakistanId = null!;
    private EntityIds _uaeId = null!;

    [SetUp]
    public async Task AddSchools()
    {
        _pakistanId = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");
        _uaeId = await TestApp.AddCountryAsync("AE", "United Arab Emirates", ["en", "ar"], "en", "Asia/Dubai", "AED");

        await TestApp.AddSchoolAsync("Alpha Academy", "alpha.localhost", _pakistanId.Id);
        await TestApp.AddSchoolAsync("Beta Academy", "beta.localhost", _uaeId.Id);
        await TestApp.AddSchoolAsync("Alpha Grammar", "grammar.localhost", _uaeId.Id);
        await TestApp.AddSchoolAsync("Gamma School", "gamma.localhost");
    }

    [Test]
    public async Task ShouldListEverySchoolWithoutFilters()
    {
        using var master = await SignInAsMasterAsync();

        var page = await ReadSchoolsAsync(master, "");

        page.TotalCount.ShouldBe(5); // the master tenant and the four schools
    }

    [TestCase("alpha")]
    [TestCase("ALPHA")]
    [TestCase("  Alpha ")]
    public async Task ShouldFindSchoolsWhoseNameContainsTheSearchIgnoringCase(string search)
    {
        using var master = await SignInAsMasterAsync();

        var page = await ReadSchoolsAsync(master, $"?search={Uri.EscapeDataString(search)}");

        page.Items.Select(s => s.Name).ShouldBe(["Alpha Academy", "Alpha Grammar"]);
        page.TotalCount.ShouldBe(2);
    }

    [Test]
    public async Task ShouldFindTextInTheMiddleOfAName()
    {
        using var master = await SignInAsMasterAsync();

        var page = await ReadSchoolsAsync(master, "?search=cadem");

        page.Items.Select(s => s.Name).ShouldBe(["Alpha Academy", "Beta Academy"]);
    }

    [TestCase("%")]
    [TestCase("_")]
    public async Task ShouldTreatWildcardsAsPlainText(string search)
    {
        using var master = await SignInAsMasterAsync();

        var page = await ReadSchoolsAsync(master, $"?search={Uri.EscapeDataString(search)}");

        page.Items.ShouldBeEmpty();
        page.TotalCount.ShouldBe(0);
    }

    [Test]
    public async Task ShouldLimitTheTableToOneCountry()
    {
        using var master = await SignInAsMasterAsync();

        var page = await ReadSchoolsAsync(master, $"?countryId={_uaeId.PublicId}");

        page.Items.Select(s => s.Name).ShouldBe(["Alpha Grammar", "Beta Academy"]);
        page.Items.ShouldAllBe(s => s.CountryId == _uaeId.PublicId);
    }

    [Test]
    public async Task ShouldCombineTheSearchAndTheCountry()
    {
        using var master = await SignInAsMasterAsync();

        var page = await ReadSchoolsAsync(master, $"?search=alpha&countryId={_pakistanId.PublicId}");

        page.Items.ShouldHaveSingleItem().Name.ShouldBe("Alpha Academy");
    }

    [Test]
    public async Task ShouldPageOnlyTheMatchingSchools()
    {
        using var master = await SignInAsMasterAsync();

        var first = await ReadSchoolsAsync(master, "?search=a&pageNumber=1&pageSize=2");
        var second = await ReadSchoolsAsync(master, "?search=a&pageNumber=2&pageSize=2");

        // Every name holds an "a", including the master tenant's ("Education Portal"), which comes first.
        first.TotalCount.ShouldBe(5);
        first.Items.Select(s => s.Name).ShouldBe(["Education Portal", "Alpha Academy"]);
        second.Items.Select(s => s.Name).ShouldBe(["Alpha Grammar", "Beta Academy"]);
    }

    [Test]
    public async Task ShouldRefuseAnInvalidFilter()
    {
        using var master = await SignInAsMasterAsync();

        using var tooLong = await master.GetAsync($"/api/Schools?search={new string('a', GetSchoolsQuery.MaxSearchLength + 1)}");
        using var emptyCountry = await master.GetAsync($"/api/Schools?countryId={Guid.Empty}");
        using var internalCountryId = await master.GetAsync($"/api/Schools?countryId={_pakistanId.Id}");

        tooLong.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        emptyCountry.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        internalCountryId.StatusCode.ShouldBe(HttpStatusCode.BadRequest); // ENG-ID-02: the internal key is never accepted
    }

    [Test]
    public async Task ShouldKeepTheSchoolListAwayFromSchoolAdministrators()
    {
        const string adminEmail = "admin@alpha.local";
        var schoolId = (await TestApp.FindSchoolIdAsync("alpha.localhost"))!;
        await TestApp.RunAsUserAsync(schoolId.Id, adminEmail, TestApp.AdministratorPassword, [Roles.Administrator]);
        using var session = TestHttp.CreateDevelopmentSession("alpha.localhost");
        (await session.SignInAsync(adminEmail, TestApp.AdministratorPassword)).Dispose();

        using var response = await session.GetAsync($"/api/Schools?search=alpha&countryId={_pakistanId.PublicId}");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldRequireSignIn()
    {
        using var session = TestHttp.CreateDevelopmentSession();

        using var response = await session.GetAsync("/api/Schools?search=alpha");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static async Task<HttpSession> SignInAsMasterAsync()
    {
        await TestApp.RunAsAdministratorAsync();
        var master = TestHttp.CreateDevelopmentSession();
        using var response = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return master;
    }

    private static async Task<SchoolPage> ReadSchoolsAsync(HttpSession session, string query)
    {
        using var response = await session.GetAsync($"/api/Schools{query}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<SchoolPage>())!;
    }

    /// <summary>The JSON shape of one page of schools.</summary>
    private sealed record SchoolPage(List<SchoolSummaryDto> Items, int TotalCount);
}
