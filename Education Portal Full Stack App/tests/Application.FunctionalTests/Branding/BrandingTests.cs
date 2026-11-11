using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.Branding.Common;
using EducationPortal.Application.Branding.Queries.GetBrandingImage;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.FunctionalTests.Branding;

/// <summary>
/// Per-school branding: public before sign-in, changed only by the school's administrators or the master tenant, and
/// never visible to or changeable by another school (SEC-AUTHZ-06, SEC-AUTHZ-09, SEC-INPUT-04).
/// </summary>
public class BrandingTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";
    private const string OtherSchoolDomain = "school2.localhost";
    private const string SchoolAdminEmail = "admin@school1.local";

    // The API sends enums by name.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    [Test]
    public async Task ShouldReturnTheBrandingOfTheHostsSchoolBeforeSignIn()
    {
        await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var master = TestHttp.CreateDevelopmentSession();
        using var school = TestHttp.CreateDevelopmentSession(SchoolDomain);

        var masterBranding = await ReadBrandingAsync(master);
        var schoolBranding = await ReadBrandingAsync(school);

        masterBranding.IsMaster.ShouldBeTrue();
        schoolBranding.IsMaster.ShouldBeFalse();
        schoolBranding.SchoolName.ShouldBe("School One");
        schoolBranding.AccentColor.ShouldBe("#00a1ff");
        schoolBranding.SidebarColor.ShouldBeNull();
        schoolBranding.LogoVersion.ShouldBeNull();
    }

    [Test]
    public async Task ShouldLetASchoolAdministratorChangeTheirOwnBranding()
    {
        await SignInAsSchoolAdministratorAsync(SchoolDomain, out var session);
        using var _ = session;

        using var response = await session.PutFormAsync("/api/Branding", Form("#763ebd", "#1F2A3D", logo: TestImages.Png, favicon: TestImages.Icon));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var branding = await ReadBrandingAsync(session);
        branding.AccentColor.ShouldBe("#763ebd");
        branding.SidebarColor.ShouldBe("#1f2a3d");
        branding.LogoVersion.ShouldNotBeNullOrEmpty();
        branding.FaviconVersion.ShouldNotBeNullOrEmpty();

        using var logo = await session.GetAsync("/api/Branding/images/Logo");
        logo.StatusCode.ShouldBe(HttpStatusCode.OK);
        logo.Content.Headers.ContentType!.MediaType.ShouldBe("image/png");
        logo.Content.Headers.ContentDisposition!.DispositionType.ShouldBe("attachment");
        (await logo.Content.ReadAsByteArrayAsync()).ShouldBe(TestImages.Png);

        using var favicon = await session.GetAsync("/api/Branding/images/Favicon");
        favicon.Content.Headers.ContentType!.MediaType.ShouldBe("image/x-icon");
    }

    [Test]
    public async Task ShouldAuditABrandingChange()
    {
        var userId = await SignInAsSchoolAdministratorAsync(SchoolDomain, out var session);
        using var _ = session;

        (await session.PutFormAsync("/api/Branding", Form("#0074ba"))).Dispose();

        (await TestApp.FindAuditEventsAsync(userId))
            .ShouldContain(e => e.EventType == SecurityAuditEventType.BrandingChanged && e.Outcome == SecurityAuditOutcome.Success);
    }

    [Test]
    public async Task ShouldRemoveALogo()
    {
        await SignInAsSchoolAdministratorAsync(SchoolDomain, out var session);
        using var _ = session;
        (await session.PutFormAsync("/api/Branding", Form("#00a1ff", logo: TestImages.Png))).Dispose();

        using var response = await session.PutFormAsync("/api/Branding", Form("#00a1ff", removeLogo: true));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadBrandingAsync(session)).LogoVersion.ShouldBeNull();
        using var logo = await session.GetAsync("/api/Branding/images/Logo");
        logo.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [TestCase("svg")]
    [TestCase("renamed-text")]
    [TestCase("too-large")]
    public async Task ShouldRefuseAnUnsafeOrOversizedLogo(string kind)
    {
        await SignInAsSchoolAdministratorAsync(SchoolDomain, out var session);
        using var _ = session;
        var content = kind switch
        {
            "svg" => TestImages.Svg,
            "renamed-text" => "just text with a .png name"u8.ToArray(),
            _ => TestImages.OversizedPng(BrandingImageRules.MaxLogoBytes + 1),
        };

        using var response = await session.PutFormAsync("/api/Branding", Form("#00a1ff", logo: content));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await CountImagesAsync()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldRefuseAJpegFavicon()
    {
        await SignInAsSchoolAdministratorAsync(SchoolDomain, out var session);
        using var _ = session;

        using var response = await session.PutFormAsync("/api/Branding", Form("#00a1ff", favicon: [0xFF, 0xD8, 0xFF, 0xE0, 0x00]));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [TestCase("red")]
    [TestCase("#12345")]
    [TestCase("#123456; background: url(x)")]
    public async Task ShouldRefuseASidebarColourThatIsNotHex(string colour)
    {
        await SignInAsSchoolAdministratorAsync(SchoolDomain, out var session);
        using var _ = session;

        using var response = await session.PutFormAsync("/api/Branding", Form("#00a1ff", colour));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRequireSignInToChangeBranding()
    {
        await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = TestHttp.CreateDevelopmentSession(SchoolDomain);

        using var response = await session.PutFormAsync("/api/Branding", Form("#0074ba"));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldRequireTheAdministratorRoleToChangeBranding()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsUserAsync(schoolId.Id, "teacher@school1.local", TestApp.DefaultUserPassword, []);
        using var session = TestHttp.CreateDevelopmentSession(SchoolDomain);
        (await session.SignInAsync("teacher@school1.local", TestApp.DefaultUserPassword)).Dispose();

        using var response = await session.PutFormAsync("/api/Branding", Form("#0074ba"));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldRequireTheAntiforgeryTokenToChangeBranding()
    {
        await SignInAsSchoolAdministratorAsync(SchoolDomain, out var session);
        using var _ = session;
        session.SetCookie("__Host-XSRF-TOKEN", "not-the-token");

        using var response = await session.PutFormAsync("/api/Branding", Form("#0074ba"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadBrandingAsync(session)).AccentColor.ShouldBe("#00a1ff");
    }

    [Test]
    public async Task ShouldNotShowOneSchoolsImagesOnAnotherSchoolsDomain()
    {
        await SignInAsSchoolAdministratorAsync(SchoolDomain, out var session);
        using var _ = session;
        (await session.PutFormAsync("/api/Branding", Form("#00a1ff", logo: TestImages.Png))).Dispose();
        await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        using var otherSchool = TestHttp.CreateDevelopmentSession(OtherSchoolDomain);

        using var logo = await otherSchool.GetAsync("/api/Branding/images/Logo");

        logo.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadBrandingAsync(otherSchool)).LogoVersion.ShouldBeNull();
    }

    [Test]
    public async Task ShouldHideOtherSchoolsImagesThroughTheQueryFilter()
    {
        var schoolOne = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var schoolTwo = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        await TestApp.AddAsync(new BrandingImage(schoolOne.Id, BrandingImageKind.Logo, TestImages.Png));

        TestApp.UseTenant(schoolTwo.Id);
        (await TestApp.SendAsync(new GetBrandingImageQuery(BrandingImageKind.Logo))).ShouldBeNull();

        TestApp.UseTenant(schoolOne.Id);
        (await TestApp.SendAsync(new GetBrandingImageQuery(BrandingImageKind.Logo))).ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldKeepMasterPortalEndpointsAwayFromSchoolAdministrators()
    {
        await SignInAsSchoolAdministratorAsync(SchoolDomain, out var session);
        using var _ = session;

        using var list = await session.GetAsync("/api/Schools");
        using var update = await session.PutFormAsync($"/api/Schools/{TestApp.MasterSchoolPublicId}/branding", Form("#fa896b"));

        list.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        update.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldLetTheMasterTenantChangeAnySchoolsBranding()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsAdministratorAsync();
        using var master = TestHttp.CreateDevelopmentSession();
        (await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword)).Dispose();

        using var schools = await master.GetAsync("/api/Schools");
        using var update = await master.PutFormAsync($"/api/Schools/{schoolId.PublicId}/branding", Form("#00ceb6", "#004d40", logo: TestImages.Png));

        schools.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await schools.Content.ReadAsStringAsync()).ShouldContain(SchoolDomain);
        update.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var school = TestHttp.CreateDevelopmentSession(SchoolDomain);
        var branding = await ReadBrandingAsync(school);
        branding.AccentColor.ShouldBe("#00ceb6");
        branding.SidebarColor.ShouldBe("#004d40");
        branding.LogoVersion.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldLetTheMasterTenantViewAnySchoolsImages()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.AddAsync(new BrandingImage(schoolId.Id, BrandingImageKind.Logo, TestImages.Png));
        await TestApp.RunAsAdministratorAsync();
        using var master = TestHttp.CreateDevelopmentSession();
        (await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword)).Dispose();

        using var logo = await master.GetAsync($"/api/Schools/{schoolId.PublicId}/branding/images/Logo");
        using var favicon = await master.GetAsync($"/api/Schools/{schoolId.PublicId}/branding/images/Favicon");

        logo.StatusCode.ShouldBe(HttpStatusCode.OK);
        favicon.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldKeepSchoolImagesAwayFromSchoolAdministrators()
    {
        // The administrator signs in on SchoolDomain, so the image belongs to another school.
        var otherSchoolId = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        await TestApp.AddAsync(new BrandingImage(otherSchoolId.Id, BrandingImageKind.Logo, TestImages.Png));
        await SignInAsSchoolAdministratorAsync(SchoolDomain, out var session);
        using var _ = session;

        using var response = await session.GetAsync($"/api/Schools/{otherSchoolId.PublicId}/branding/images/Logo");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldAnswer404ForAnUnknownSchoolInTheMasterPortal()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = TestHttp.CreateDevelopmentSession();
        (await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword)).Dispose();

        using var response = await master.GetAsync("/api/Schools/999999/branding");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static Task<string> SignInAsSchoolAdministratorAsync(string domain, out HttpSession session)
    {
        session = TestHttp.CreateDevelopmentSession(domain);
        return SignInAsync(domain, session);

        static async Task<string> SignInAsync(string domain, HttpSession session)
        {
            var schoolId = await TestApp.AddSchoolAsync("School One", domain);
            var userId = await TestApp.RunAsUserAsync(schoolId.Id, SchoolAdminEmail, TestApp.AdministratorPassword, [Roles.Administrator]);
            using var response = await session.SignInAsync(SchoolAdminEmail, TestApp.AdministratorPassword);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            return userId;
        }
    }

    private static async Task<BrandingDto> ReadBrandingAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/Branding");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<BrandingDto>(Json))!;
    }

    private static async Task<int> CountImagesAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EducationPortal.Infrastructure.Data.ApplicationDbContext>();
        // A scope without a request has no tenant, so the query filter would hide every row; count the table directly.
        return await context.Database.SqlQuery<int>($"SELECT COUNT(*)::int AS \"Value\" FROM \"BrandingImages\"").SingleAsync();
    }

    private static MultipartFormDataContent Form(
        string accent,
        string? sidebarColor = null,
        byte[]? logo = null,
        byte[]? favicon = null,
        bool removeLogo = false)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(accent), "AccentColor" },
            { new StringContent(removeLogo ? "true" : "false"), "RemoveLogo" },
        };

        if (sidebarColor is not null)
        {
            form.Add(new StringContent(sidebarColor), "SidebarColor");
        }

        AddFile(form, "Logo", "logo.png", logo);
        AddFile(form, "Favicon", "favicon.ico", favicon);
        return form;
    }

    private static void AddFile(MultipartFormDataContent form, string name, string fileName, byte[]? content)
    {
        if (content is null)
        {
            return;
        }

        // The browser's content type is a claim; the server checks the bytes instead.
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(file, name, fileName);
    }
}
