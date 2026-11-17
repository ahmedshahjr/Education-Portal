using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.SchoolPublicPage;

/// <summary>
/// A school's public page is edited by its administrator (School module) on its own domain; the master tenant edits any
/// school by its public id. One school never reads or changes another school's page, curricula, facilities, photos or
/// class offerings, and a user without the administrator role or with the module off is refused (AGENTS.md section 2).
/// </summary>
public class SchoolPublicPageTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";
    private const string OtherSchoolDomain = "school2.localhost";
    private const string AdminEmail = "admin@school1.local";
    private const string OtherAdminEmail = "admin@school2.local";
    private const string StaffEmail = "staff@school1.local";
    private const string SchoolModuleDescription = "School profile, settings and the school overview dashboard.";

    private static readonly object ValidProfile = new
    {
        tagline = "Learning with care",
        about = "A school for boys and girls in Karachi.",
        establishedYear = 1990,
        genderPolicy = "CoEducation",
        visitingHours = "Monday to Friday, 8:00 to 14:00",
        virtualTourUrl = "https://tour.example.com/school1",
    };

    [Test]
    public async Task ShouldLetTheSchoolSaveAndPublishItsOwnPage()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, AdminEmail);
        var adminUserId = TestApp.GetUserId()!; // the school administrator who signed in, not the master

        var draft = await ReadJsonAsync(admin, "/api/SchoolProfile");
        draft.GetProperty("tagline").GetString().ShouldBe(string.Empty);
        draft.GetProperty("isPublished").GetBoolean().ShouldBeFalse();

        (await admin.PutAsync("/api/SchoolProfile", ValidProfile)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await admin.PostAsync("/api/SchoolProfile/publish")).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var published = await ReadJsonAsync(admin, "/api/SchoolProfile");
        published.GetProperty("tagline").GetString().ShouldBe("Learning with care");
        published.GetProperty("establishedYear").GetInt32().ShouldBe(1990);
        published.GetProperty("isPublished").GetBoolean().ShouldBeTrue();
        published.GetProperty("publishedAt").ValueKind.ShouldNotBe(JsonValueKind.Null);

        (await admin.PostAsync("/api/SchoolProfile/unpublish")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadJsonAsync(admin, "/api/SchoolProfile")).GetProperty("isPublished").GetBoolean().ShouldBeFalse();

        var audit = await TestApp.FindAuditEventsAsync(adminUserId);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.PublicPageProfileChanged);
    }

    [Test]
    public async Task ShouldNeverLetAnotherSchoolReadOrChangeThePage()
    {
        await TestApp.RunAsAdministratorAsync();
        var one = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var two = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        using var adminOne = await SignInToSchoolAsync(one.Id, SchoolDomain, AdminEmail);
        using var adminTwo = await SignInToSchoolAsync(two.Id, OtherSchoolDomain, OtherAdminEmail);
        (await adminOne.PutAsync("/api/SchoolProfile", ValidProfile)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var otherSchool = $"?schoolId={one.PublicId}";

        (await adminTwo.GetAsync($"/api/SchoolProfile{otherSchool}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.PutAsync($"/api/SchoolProfile{otherSchool}", new { tagline = "Stolen", about = "Stolen" })).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.PostAsync($"/api/SchoolProfile/publish{otherSchool}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // School two's own page is its empty draft, not school one's text.
        (await ReadJsonAsync(adminTwo, "/api/SchoolProfile")).GetProperty("tagline").GetString().ShouldBe(string.Empty);
        (await ReadJsonAsync(adminOne, "/api/SchoolProfile")).GetProperty("tagline").GetString().ShouldBe("Learning with care");
    }

    [Test]
    public async Task ShouldLetTheMasterTenantEditAnySchoolByItsPublicId()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, AdminEmail);
        using var master = TestHttp.CreateDevelopmentSession();
        (await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword)).StatusCode.ShouldBe(HttpStatusCode.OK);
        var forSchool = $"?schoolId={school.PublicId}";

        // The master tenant owns no school, so it must name one.
        (await master.GetAsync("/api/SchoolProfile")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await master.PutAsync($"/api/SchoolProfile{forSchool}", ValidProfile)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await master.PutAsync($"/api/SchoolCurricula{forSchool}", new { kinds = new[] { "Cambridge" } })).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await ReadJsonAsync(admin, "/api/SchoolProfile")).GetProperty("tagline").GetString().ShouldBe("Learning with care");
        (await ReadJsonAsync(admin, "/api/SchoolCurricula")).EnumerateArray().Select(e => e.GetString()).ShouldBe(["Cambridge"]);
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

        (await staff.PutAsync("/api/SchoolProfile", ValidProfile)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldRefuseEveryPageReadWhenTheSchoolModuleIsOff()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, AdminEmail);
        using var master = TestHttp.CreateDevelopmentSession();
        (await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword)).StatusCode.ShouldBe(HttpStatusCode.OK);

        using (var off = await master.PutAsync($"/api/Modules/{ModuleCode.School}", new { name = "School", description = SchoolModuleDescription, isCore = true, isActive = false, sortOrder = 10 }))
        {
            off.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        foreach (var path in ReadPaths)
        {
            using var refused = await admin.GetAsync(path);
            refused.StatusCode.ShouldBe(HttpStatusCode.Forbidden, path);
            (await refused.Content.ReadAsStringAsync()).ShouldContain("MODULE_NOT_ENABLED");
        }

        (await master.PutAsync($"/api/Modules/{ModuleCode.School}", new { name = "School", description = SchoolModuleDescription, isCore = true, isActive = true, sortOrder = 10 })).Dispose();
        (await admin.GetAsync("/api/SchoolProfile")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldSetCurriculaAndFacilitiesOncePerKindAndRemoveThem()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, AdminEmail);

        (await admin.PutAsync("/api/SchoolCurricula", new { kinds = new[] { "Matric", "Cambridge" } })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await admin.PutAsync("/api/SchoolCurricula", new { kinds = new[] { "Cambridge" } })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadJsonAsync(admin, "/api/SchoolCurricula")).EnumerateArray().Select(e => e.GetString()).ShouldBe(["Cambridge"]);
        (await admin.PutAsync("/api/SchoolCurricula", new { kinds = new[] { "Cambridge", "Cambridge" } })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        (await admin.PutAsync("/api/SchoolFacilities/Transport", new { description = "Buses cover 12 routes" })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await admin.PutAsync("/api/SchoolFacilities/Transport", new { description = "   " })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var facilities = await ReadJsonAsync(admin, "/api/SchoolFacilities");
        facilities.GetArrayLength().ShouldBe(1);
        facilities.EnumerateArray().First().GetProperty("kind").GetString().ShouldBe("Transport");
        facilities.EnumerateArray().First().GetProperty("description").ValueKind.ShouldBe(JsonValueKind.Null);

        (await admin.DeleteAsync("/api/SchoolFacilities/Transport")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await admin.DeleteAsync("/api/SchoolFacilities/Transport")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadJsonAsync(admin, "/api/SchoolFacilities")).GetArrayLength().ShouldBe(0);
    }

    [Test]
    public async Task ShouldRefuseAnotherSchoolsFacilityAndCurriculumChanges()
    {
        await TestApp.RunAsAdministratorAsync();
        var one = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var two = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        using var adminOne = await SignInToSchoolAsync(one.Id, SchoolDomain, AdminEmail);
        using var adminTwo = await SignInToSchoolAsync(two.Id, OtherSchoolDomain, OtherAdminEmail);
        (await adminOne.PutAsync("/api/SchoolFacilities/Hostel", new { description = "Rooms for 40" })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var other = $"?schoolId={one.PublicId}";

        (await adminTwo.DeleteAsync($"/api/SchoolFacilities/Hostel{other}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.PutAsync($"/api/SchoolCurricula{other}", new { kinds = new[] { "Montessori" } })).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadJsonAsync(adminTwo, "/api/SchoolCurricula")).GetArrayLength().ShouldBe(0);
        (await ReadJsonAsync(adminOne, "/api/SchoolFacilities")).GetArrayLength().ShouldBe(1);
    }

    [Test]
    public async Task ShouldStoreOnlyPhotosUnderTwoMegabytesThatAreImagesByTheirBytes()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, AdminEmail);
        var adminUserId = TestApp.GetUserId()!;

        var photo = await UploadAsync(admin, TestImages.Png, "Front gate");
        var listed = await ReadJsonAsync(admin, "/api/SchoolPhotos");
        listed.GetArrayLength().ShouldBe(1);
        listed.EnumerateArray().First().TryGetProperty("content", out _).ShouldBeFalse("the list never carries the image bytes");
        listed.EnumerateArray().First().GetProperty("contentType").GetString().ShouldBe("image/png");

        using var content = await admin.GetAsync($"/api/SchoolPhotos/{photo}/content");
        content.StatusCode.ShouldBe(HttpStatusCode.OK);
        content.Content.Headers.ContentType!.MediaType.ShouldBe("image/png");
        string.Join(",", content.Headers.GetValues("Cache-Control")).ShouldContain("no-store"); // personal data is never cached (SecurityHeaders)
        content.Headers.ETag.ShouldNotBeNull();
        (await content.Content.ReadAsByteArrayAsync()).ShouldBe(TestImages.Png);

        (await UploadResponseAsync(admin, TestImages.Svg)).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await UploadResponseAsync(admin, TestImages.OversizedPng(SchoolPhoto.MaxBytes + 1))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await UploadResponseAsync(admin, TestImages.Png)).StatusCode.ShouldBe(HttpStatusCode.OK);

        using var replaceWithSvg = await PutPhotoAsync(admin, photo, "Front gate", TestImages.Svg);
        replaceWithSvg.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        (await DeletePhotoAsync(admin, photo)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await admin.GetAsync($"/api/SchoolPhotos/{photo}/content")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var audit = await TestApp.FindAuditEventsAsync(adminUserId);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.SchoolPhotoUploaded);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.SchoolPhotoDeleted);
    }

    [Test]
    public async Task ShouldNeverServeAnotherSchoolsPhoto()
    {
        await TestApp.RunAsAdministratorAsync();
        var one = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var two = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        using var adminOne = await SignInToSchoolAsync(one.Id, SchoolDomain, AdminEmail);
        using var adminTwo = await SignInToSchoolAsync(two.Id, OtherSchoolDomain, OtherAdminEmail);
        var photo = await UploadAsync(adminOne, TestImages.Png, "Front gate");

        (await adminTwo.GetAsync($"/api/SchoolPhotos/{photo}/content")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.GetAsync($"/api/SchoolPhotos/{photo}/content?schoolId={one.PublicId}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await DeletePhotoAsync(adminTwo, photo)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadJsonAsync(adminTwo, "/api/SchoolPhotos")).GetArrayLength().ShouldBe(0);
    }

    [Test]
    public async Task ShouldOfferClassesOnlyForItsOwnCampusesAndSchool()
    {
        await TestApp.RunAsAdministratorAsync();
        var one = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var two = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        using var adminOne = await SignInToSchoolAsync(one.Id, SchoolDomain, AdminEmail);
        using var adminTwo = await SignInToSchoolAsync(two.Id, OtherSchoolDomain, OtherAdminEmail);
        var campus = (await ReadJsonAsync(adminOne, "/api/Campuses")).EnumerateArray().First().GetProperty("id").GetGuid();

        var offering = await CreateOfferingAsync(adminOne, campus, seatsAvailable: 30);
        var listed = await ReadJsonAsync(adminOne, "/api/ClassOfferings");
        listed.GetArrayLength().ShouldBe(1);
        listed.EnumerateArray().First().GetProperty("campusId").GetGuid().ShouldBe(campus);
        listed.EnumerateArray().First().GetProperty("currencyCode").GetString().ShouldBe("PKR");

        (await adminOne.PutAsync($"/api/ClassOfferings/{offering}", OfferingBody(monthlyFee: 5500, seatsAvailable: 35))).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await adminOne.PutAsync($"/api/ClassOfferings/{offering}", OfferingBody(monthlyFee: 5500, seatsAvailable: 41))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadJsonAsync(adminOne, "/api/ClassOfferings")).EnumerateArray().First().GetProperty("monthlyFee").GetDecimal().ShouldBe(5500m);

        // School two can neither read, change, delete nor place an offering on school one's campus or offering.
        (await ReadJsonAsync(adminTwo, "/api/ClassOfferings")).GetArrayLength().ShouldBe(0);
        (await adminTwo.PutAsync($"/api/ClassOfferings/{offering}", OfferingBody(monthlyFee: 1, seatsAvailable: 1))).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.DeleteAsync($"/api/ClassOfferings/{offering}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.PostAsync("/api/ClassOfferings", OfferingBody(campusId: campus))).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await adminOne.DeleteAsync($"/api/ClassOfferings/{offering}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadJsonAsync(adminOne, "/api/ClassOfferings")).GetArrayLength().ShouldBe(0);
    }

    // The read paths of the five endpoint groups, each taking the school as a query parameter.
    private static readonly string[] ReadPaths =
    [
        "/api/SchoolProfile",
        "/api/SchoolCurricula",
        "/api/SchoolFacilities",
        "/api/SchoolPhotos",
        "/api/ClassOfferings",
    ];

    private static object OfferingBody(Guid? campusId = null, decimal monthlyFee = 5000, int seatsAvailable = 30) => new
    {
        campusId,
        level = "Nursery",
        academicYear = "2026-27",
        monthlyFee,
        admissionFee = 1000m,
        currencyCode = "pkr",
        seatsTotal = 40,
        seatsAvailable,
        status = "Open",
        showFee = true,
    };

    private static async Task<Guid> CreateOfferingAsync(HttpSession admin, Guid campus, int seatsAvailable)
    {
        using var response = await admin.PostAsync("/api/ClassOfferings", OfferingBody(campus, seatsAvailable: seatsAvailable));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetGuid();
    }

    private static async Task<Guid> UploadAsync(HttpSession admin, byte[] image, string caption)
    {
        using var response = await UploadResponseAsync(admin, image, caption);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetGuid();
    }

    private static async Task<HttpResponseMessage> UploadResponseAsync(HttpSession admin, byte[] image, string caption = "Photo")
    {
        using var form = PhotoForm(image, caption);
        return await admin.PostFormAsync("/api/SchoolPhotos", form);
    }

    private static async Task<HttpResponseMessage> PutPhotoAsync(HttpSession admin, Guid photo, string caption, byte[] image)
    {
        using var form = PhotoForm(image, caption);
        return await admin.PutFormAsync($"/api/SchoolPhotos/{photo}", form);
    }

    private static Task<HttpResponseMessage> DeletePhotoAsync(HttpSession admin, Guid photo) => admin.DeleteAsync($"/api/SchoolPhotos/{photo}");

    private static MultipartFormDataContent PhotoForm(byte[] image, string caption)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent("Campus"), "kind" },
            { new StringContent("1"), "sortOrder" },
            { new StringContent(caption), "caption" },
        };
        var file = new ByteArrayContent(image);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(file, "file", "photo.png");
        return form;
    }

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
