using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.FunctionalTests.AcademicCalendar;

/// <summary>
/// The school calendar (academic years, terms, shifts, holidays and events) is managed by a school's administrator on its
/// own domain (Academic setup module). Another school never reads or changes it, a user without the administrator role
/// is refused, and every request is refused while the module is off (AGENTS.md section 2).
/// </summary>
public class AcademicCalendarTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";
    private const string OtherSchoolDomain = "school2.localhost";
    private const string AdminEmail = "admin@school1.local";
    private const string OtherAdminEmail = "admin@school2.local";
    private const string StaffEmail = "staff@school1.local";

    private static readonly string[] ReadPaths =
    [
        "/api/AcademicYears",
        "/api/Shifts",
        "/api/SchoolCalendar?from=2026-08-01&to=2026-08-31",
    ];

    [Test]
    public async Task ShouldKeepOneCurrentYearAndRefuseOverlapsAndClashingTerms()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, AdminEmail);
        var adminUserId = TestApp.GetUserId()!;

        var first = await CreateYearAsync(admin, "2025-26", "2025-08-01", "2026-06-30", isCurrent: true);
        var second = await CreateYearAsync(admin, "2026-27", "2026-08-01", "2027-06-30", isCurrent: false);

        // Same name, or dates that touch another year, are refused.
        (await admin.PostAsync("/api/AcademicYears", Year("2026-27", "2027-08-01", "2028-06-30"))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await admin.PostAsync("/api/AcademicYears", Year("Overlap", "2026-06-30", "2026-07-31"))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        (await admin.PutAsync($"/api/AcademicYears/{second}/current", new { })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var years = await ReadJsonAsync(admin, "/api/AcademicYears");
        years.EnumerateArray().Single(y => y.GetProperty("isCurrent").GetBoolean()).GetProperty("id").GetGuid().ShouldBe(second);
        years.EnumerateArray().First().GetProperty("name").GetString().ShouldBe("2026-27"); // newest first

        var term = await CreateTermAsync(admin, second, "First term", 1, "2026-08-01", "2026-12-15");
        (await admin.PostAsync($"/api/AcademicYears/{second}/terms", Term("Clash", 1, "2027-01-05", "2027-03-31"))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await admin.PostAsync($"/api/AcademicYears/{second}/terms", Term("Overlap", 2, "2026-12-01", "2027-03-31"))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await admin.PostAsync($"/api/AcademicYears/{second}/terms", Term("Outside", 2, "2027-06-01", "2027-07-31"))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await CreateTermAsync(admin, second, "Second term", 2, "2027-01-05", "2027-06-30");

        // The year cannot shrink past its terms, and it cannot be deleted while it has terms.
        (await admin.PutAsync($"/api/AcademicYears/{second}", YearUpdate("2026-27", "2026-09-01", "2027-06-30", "Active"))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await admin.DeleteAsync($"/api/AcademicYears/{second}")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // A closed year cannot be changed or made current until it is reopened.
        (await admin.PutAsync($"/api/AcademicYears/{first}", YearUpdate("2025-26", "2025-08-01", "2026-06-30", "Closed"))).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await admin.PutAsync($"/api/AcademicYears/{first}", YearUpdate("Renamed", "2025-08-01", "2026-06-30", "Closed"))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await admin.PutAsync($"/api/AcademicYears/{first}/current", new { })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await admin.PutAsync($"/api/AcademicYears/{first}", YearUpdate("Renamed", "2025-08-01", "2026-06-30", "Active"))).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await admin.DeleteAsync($"/api/AcademicYears/terms/{term}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadJsonAsync(admin, "/api/AcademicYears")).EnumerateArray()
            .Single(y => y.GetProperty("id").GetGuid() == second)
            .GetProperty("terms").EnumerateArray().Select(t => t.GetProperty("name").GetString()).ShouldBe(["Second term"]);

        (await TestApp.FindAuditEventsAsync(adminUserId)).ShouldContain(e => e.EventType == SecurityAuditEventType.AcademicYearChanged);
    }

    [Test]
    public async Task ShouldManageShiftsHolidaysAndEventsPerCampus()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, AdminEmail);
        var campus = await MainCampusAsync(admin);

        var shift = await CreateAsync(admin, "/api/Shifts", new { campusId = campus, name = "Morning", startTime = "07:30:00", endTime = "13:00:00" });
        (await admin.PostAsync("/api/Shifts", new { campusId = campus, name = "Morning", startTime = "13:30:00", endTime = "18:00:00" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await admin.PutAsync($"/api/Shifts/{shift}", new { name = "Morning", startTime = "08:00:00", endTime = "13:00:00" })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadJsonAsync(admin, $"/api/Shifts?campusId={campus}")).EnumerateArray().Single().GetProperty("startTime").GetString().ShouldBe("08:00:00");

        await CreateAsync(admin, "/api/SchoolCalendar/holidays", Holiday("Independence Day", null, "2026-08-14", "2026-08-14"));
        var campusHoliday = await CreateAsync(admin, "/api/SchoolCalendar/holidays", Holiday("Campus repairs", campus, "2026-08-20", "2026-08-22"));
        await CreateAsync(admin, "/api/SchoolCalendar/holidays", Holiday("Winter break", null, "2026-12-20", "2026-12-31"));
        var meeting = await CreateAsync(admin, "/api/SchoolCalendar/events", new
        {
            title = "Parent-teacher meeting",
            eventDate = "2026-08-28",
            startTime = "09:00:00",
            endTime = "12:00:00",
            type = "ParentTeacherMeeting",
            audience = "Parents",
        });

        var august = await ReadJsonAsync(admin, $"/api/SchoolCalendar?from=2026-08-01&to=2026-08-31&campusId={campus}");
        august.GetProperty("holidays").EnumerateArray().Select(h => h.GetProperty("title").GetString()).ShouldBe(["Independence Day", "Campus repairs"]);
        august.GetProperty("events").EnumerateArray().Single().GetProperty("id").GetGuid().ShouldBe(meeting);

        (await admin.DeleteAsync($"/api/SchoolCalendar/holidays/{campusHoliday}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadJsonAsync(admin, "/api/SchoolCalendar?from=2026-08-01&to=2026-08-31")).GetProperty("holidays").GetArrayLength().ShouldBe(1);

        // A range longer than the limit, or backwards, is refused.
        (await admin.GetAsync("/api/SchoolCalendar?from=2026-01-01&to=2027-12-31")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await admin.GetAsync("/api/SchoolCalendar?from=2026-02-01&to=2026-01-01")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldNeverLetAnotherSchoolReadOrChangeTheCalendar()
    {
        await TestApp.RunAsAdministratorAsync();
        var one = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var two = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        using var adminOne = await SignInToSchoolAsync(one.Id, SchoolDomain, AdminEmail);
        using var adminTwo = await SignInToSchoolAsync(two.Id, OtherSchoolDomain, OtherAdminEmail);
        var campusOne = await MainCampusAsync(adminOne);

        var year = await CreateYearAsync(adminOne, "2026-27", "2026-08-01", "2027-06-30", isCurrent: false);
        var shift = await CreateAsync(adminOne, "/api/Shifts", new { campusId = campusOne, name = "Morning", startTime = "07:30:00", endTime = "13:00:00" });
        var holiday = await CreateAsync(adminOne, "/api/SchoolCalendar/holidays", Holiday("Founders day", null, "2026-08-14", "2026-08-14"));
        var otherSchool = $"?schoolId={one.PublicId}";

        // Naming school one is a 404; school two's own lists are empty.
        foreach (var path in ReadPaths)
        {
            var separator = path.Contains('?') ? '&' : '?';
            (await adminTwo.GetAsync($"{path}{separator}schoolId={one.PublicId}")).StatusCode.ShouldBe(HttpStatusCode.NotFound, path);
        }

        (await ReadJsonAsync(adminTwo, "/api/AcademicYears")).GetArrayLength().ShouldBe(0);
        (await ReadJsonAsync(adminTwo, "/api/Shifts")).GetArrayLength().ShouldBe(0);

        // School one's rows, and its campus, are not found from school two.
        (await adminTwo.PutAsync($"/api/AcademicYears/{year}", YearUpdate("Stolen", "2026-08-01", "2027-06-30", "Active"))).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.PutAsync($"/api/AcademicYears/{year}/current", new { })).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.PostAsync($"/api/AcademicYears/{year}/terms", Term("Stolen", 1, "2026-08-01", "2026-12-15"))).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.DeleteAsync($"/api/Shifts/{shift}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.DeleteAsync($"/api/SchoolCalendar/holidays/{holiday}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.PostAsync("/api/Shifts", new { campusId = campusOne, name = "Stolen", startTime = "07:30:00", endTime = "13:00:00" })).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.PostAsync("/api/SchoolCalendar/holidays", Holiday("Stolen", campusOne, "2026-08-14", "2026-08-14"))).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.PostAsync($"/api/AcademicYears{otherSchool}", Year("Stolen", "2030-08-01", "2031-06-30"))).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // School one's data is unchanged.
        (await ReadJsonAsync(adminOne, "/api/AcademicYears")).EnumerateArray().Single().GetProperty("name").GetString().ShouldBe("2026-27");
        (await ReadJsonAsync(adminOne, "/api/Shifts")).GetArrayLength().ShouldBe(1);
    }

    [Test]
    public async Task ShouldLetTheMasterTenantManageASchoolsCalendarByItsId()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var master = TestHttp.CreateDevelopmentSession();
        (await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword)).StatusCode.ShouldBe(HttpStatusCode.OK);

        // The master tenant owns no school, so it must name one.
        (await master.GetAsync("/api/AcademicYears")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await master.PostAsync($"/api/AcademicYears?schoolId={school.PublicId}", Year("2026-27", "2026-08-01", "2027-06-30"))).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadJsonAsync(master, $"/api/AcademicYears?schoolId={school.PublicId}")).GetArrayLength().ShouldBe(1);
    }

    [Test]
    public async Task ShouldRefuseAStaffUserAndAnAnonymousVisitor()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsUserAsync(school.Id, StaffEmail, TestApp.AdministratorPassword, []);
        using var staff = TestHttp.CreateDevelopmentSession(SchoolDomain);
        (await staff.SignInAsync(StaffEmail, TestApp.AdministratorPassword)).StatusCode.ShouldBe(HttpStatusCode.OK);
        using var anonymous = TestHttp.CreateDevelopmentSession(SchoolDomain);

        foreach (var path in ReadPaths)
        {
            (await staff.GetAsync(path)).StatusCode.ShouldBe(HttpStatusCode.Forbidden, path);
            (await anonymous.GetAsync(path)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized, path);
        }

        (await staff.PostAsync("/api/AcademicYears", Year("2026-27", "2026-08-01", "2027-06-30"))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldRefuseEveryRequestWhenTheAcademicSetupModuleIsOff()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, AdminEmail);
        using var master = TestHttp.CreateDevelopmentSession();
        (await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword)).StatusCode.ShouldBe(HttpStatusCode.OK);

        using (var off = await master.PutAsync($"/api/Modules/{ModuleCode.AcademicSetup}", AcademicSetupModule(isActive: false)))
        {
            off.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        foreach (var path in ReadPaths)
        {
            using var refused = await admin.GetAsync(path);
            refused.StatusCode.ShouldBe(HttpStatusCode.Forbidden, path);
            (await refused.Content.ReadAsStringAsync()).ShouldContain("MODULE_NOT_ENABLED");
        }

        (await admin.PostAsync("/api/AcademicYears", Year("2026-27", "2026-08-01", "2027-06-30"))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        (await master.PutAsync($"/api/Modules/{ModuleCode.AcademicSetup}", AcademicSetupModule(isActive: true))).Dispose();
        (await admin.GetAsync("/api/AcademicYears")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private static object AcademicSetupModule(bool isActive) => new
    {
        name = "Academic setup",
        description = "Academic years, classes, courses and enrolments.",
        isCore = true,
        isActive,
        sortOrder = 30,
    };

    private static object Year(string name, string start, string end, bool isCurrent = false) =>
        new { name, startDate = start, endDate = end, isCurrent };

    private static object YearUpdate(string name, string start, string end, string status) =>
        new { name, startDate = start, endDate = end, status };

    private static object Term(string name, int sequence, string start, string end) =>
        new { name, sequence, startDate = start, endDate = end };

    private static object Holiday(string title, Guid? campusId, string start, string end) => new
    {
        campusId,
        title,
        startDate = start,
        endDate = end,
        type = "Public",
        appliesTo = "Both",
        isTentative = false,
    };

    private static async Task<Guid> CreateYearAsync(HttpSession admin, string name, string start, string end, bool isCurrent) =>
        await CreateAsync(admin, "/api/AcademicYears", Year(name, start, end, isCurrent));

    private static async Task<Guid> CreateTermAsync(HttpSession admin, Guid year, string name, int sequence, string start, string end) =>
        await CreateAsync(admin, $"/api/AcademicYears/{year}/terms", Term(name, sequence, start, end));

    private static async Task<Guid> CreateAsync(HttpSession admin, string path, object body)
    {
        using var response = await admin.PostAsync(path, body);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task<Guid> MainCampusAsync(HttpSession admin) =>
        (await ReadJsonAsync(admin, "/api/Campuses")).EnumerateArray()
            .Single(c => c.GetProperty("kind").GetString() == nameof(CampusKind.Main))
            .GetProperty("id").GetGuid();

    private static async Task<JsonElement> ReadJsonAsync(HttpSession session, string path)
    {
        using var response = await session.GetAsync(path);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, path);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();
    }

    private static async Task<HttpSession> SignInToSchoolAsync(int schoolId, string domain, string email)
    {
        await TestApp.RunAsUserAsync(schoolId, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }
}
