using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Constants;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.AcademicCalendar;

/// <summary>
/// The school calendar shows its holidays, calendar events and school events together. The school events are for the whole
/// school, so a campus filter keeps them, and a school never sees another school's events (SEC-AUTHZ-03).
/// </summary>
public class CombinedCalendarTests : TestBase
{
    private const string SchoolDomain = "combined-calendar.localhost";
    private const string OtherSchoolDomain = "combined-calendar-other.localhost";
    private const string AdminEmail = "admin@combined-calendar.local";

    [Test]
    public async Task ShouldShowSchoolEventsBesideHolidaysAndFilterThemOut()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Combined One", SchoolDomain);
        var other = await TestApp.AddSchoolAsync("Combined Two", OtherSchoolDomain);
        using var admin = await SignInAsync(school.Id, SchoolDomain, AdminEmail);
        using var otherAdmin = await SignInAsync(other.Id, OtherSchoolDomain, "admin@combined-calendar-other.local");
        var campus = await MainCampusAsync(admin);

        (await admin.PostAsync("/api/SchoolCalendar/holidays", new
        {
            campusId = (Guid?)null, title = "Independence Day", startDate = "2026-08-14", endDate = "2026-08-14",
            type = "Public", appliesTo = "Both", isTentative = false,
        })).StatusCode.ShouldBe(HttpStatusCode.OK);

        (await admin.PostAsync("/api/SchoolEvents", new
        {
            title = "Sports day", description = (string?)null,
            startsAt = "2026-08-20T09:00:00Z", endsAt = "2026-08-20T13:00:00Z", audience = "All",
        })).StatusCode.ShouldBe(HttpStatusCode.OK);

        (await otherAdmin.PostAsync("/api/SchoolEvents", new
        {
            title = "Other school's fair", description = (string?)null,
            startsAt = "2026-08-21T09:00:00Z", endsAt = "2026-08-21T13:00:00Z", audience = "All",
        })).StatusCode.ShouldBe(HttpStatusCode.OK);

        var all = await ReadAsync(admin, "/api/SchoolCalendar?from=2026-08-01&to=2026-08-31");
        all.GetProperty("holidays").GetArrayLength().ShouldBe(1);
        all.GetProperty("schoolEvents").EnumerateArray().Select(e => e.GetProperty("title").GetString()).ShouldBe(["Sports day"]);

        // Leaving school events out, and a campus filter that keeps them.
        var without = await ReadAsync(admin, "/api/SchoolCalendar?from=2026-08-01&to=2026-08-31&includeSchoolEvents=false");
        without.GetProperty("schoolEvents").GetArrayLength().ShouldBe(0);
        var forCampus = await ReadAsync(admin, $"/api/SchoolCalendar?from=2026-08-01&to=2026-08-31&campusId={campus}");
        forCampus.GetProperty("schoolEvents").GetArrayLength().ShouldBe(1);

        // A range that does not touch the event leaves it out.
        var later = await ReadAsync(admin, "/api/SchoolCalendar?from=2026-09-01&to=2026-09-30");
        later.GetProperty("schoolEvents").GetArrayLength().ShouldBe(0);
    }

    private static async Task<JsonElement> ReadAsync(HttpSession session, string path)
    {
        using var response = await session.GetAsync(path);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, path);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<Guid> MainCampusAsync(HttpSession admin) =>
        (await ReadAsync(admin, "/api/Campuses")).EnumerateArray()
            .Single(c => c.GetProperty("kind").GetString() == "Main")
            .GetProperty("id").GetGuid();

    private static async Task<HttpSession> SignInAsync(int schoolId, string domain, string email)
    {
        await TestApp.RunAsUserAsync(schoolId, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }
}
