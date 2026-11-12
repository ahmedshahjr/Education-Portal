using System.Net;
using System.Net.Http.Json;
using EducationPortal.Application.Localization.Queries.GetLocalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.FunctionalTests.Localization;

/// <summary>
/// Each school answers in one of its country's languages, chosen by the visitor or their browser, and nothing else
/// (TenantRequestLocalization). Pakistan offers English and Urdu, the UAE English and Arabic.
/// </summary>
public class LocalizationTests : TestBase
{
    private const string PakistanDomain = "pk.localhost";
    private const string UaeDomain = "ae.localhost";
    private const string NoCountryDomain = "plain.localhost";

    private const string TeacherEmail = "teacher@pk.local";

    private int _pakistanSchoolId;

    [SetUp]
    public async Task AddSchools()
    {
        var pakistan = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");
        var uae = await TestApp.AddCountryAsync("AE", "United Arab Emirates", ["en", "ar"], "ar", "Asia/Dubai", "AED");
        _pakistanSchoolId = (await TestApp.AddSchoolAsync("Lahore School", PakistanDomain, pakistan.Id)).Id;
        await TestApp.AddSchoolAsync("Dubai School", UaeDomain, uae.Id);
        await TestApp.AddSchoolAsync("Plain School", NoCountryDomain);
    }

    [Test]
    public async Task ShouldOfferOnlyEnglishWithoutACountry()
    {
        using var session = TestHttp.CreateDevelopmentSession(NoCountryDomain);
        session.SetHeader("Accept-Language", "ar-AE,ar;q=0.9");

        var localization = await ReadAsync(session);

        localization.Language.ShouldBe("en");
        localization.IsRightToLeft.ShouldBeFalse();
        localization.Languages.Select(l => l.Code).ShouldBe(["en"]);
        localization.TimeZone.ShouldBe("UTC");
        localization.CurrencyCode.ShouldBeNull();
    }

    [Test]
    public async Task ShouldOfferTheCountrysLanguagesTimeZoneAndCurrency()
    {
        using var pakistan = TestHttp.CreateDevelopmentSession(PakistanDomain);
        using var uae = TestHttp.CreateDevelopmentSession(UaeDomain);

        var pk = await ReadAsync(pakistan);
        var ae = await ReadAsync(uae);

        pk.Languages.Select(l => l.Code).ShouldBe(["en", "ur"]);
        pk.Language.ShouldBe("en");
        pk.TimeZone.ShouldBe("Asia/Karachi");
        pk.CurrencyCode.ShouldBe("PKR");

        ae.Languages.Select(l => l.Code).ShouldBe(["en", "ar"]);
        ae.Language.ShouldBe("ar"); // the UAE school's default
        ae.Culture.ShouldBe("ar-AE");
        ae.IsRightToLeft.ShouldBeTrue();
        ae.CurrencyCode.ShouldBe("AED");
    }

    [Test]
    public async Task ShouldFollowTheBrowsersLanguageWhenTheSchoolOffersIt()
    {
        using var session = TestHttp.CreateDevelopmentSession(PakistanDomain);
        session.SetHeader("Accept-Language", "ur-PK,ur;q=0.9,en;q=0.8");

        using var response = await session.GetAsync("/api/Localization");
        var localization = (await response.Content.ReadFromJsonAsync<LocalizationDto>())!;

        localization.Language.ShouldBe("ur");
        localization.Culture.ShouldBe("ur-PK");
        localization.IsRightToLeft.ShouldBeTrue();
        response.Content.Headers.ContentLanguage.ShouldContain("ur-PK");
    }

    [Test]
    public async Task ShouldIgnoreABrowserLanguageTheSchoolDoesNotOffer()
    {
        using var session = TestHttp.CreateDevelopmentSession(PakistanDomain);
        session.SetHeader("Accept-Language", "ar-AE,ar;q=0.9");

        (await ReadAsync(session)).Language.ShouldBe("en");
    }

    [Test]
    public async Task ShouldRememberTheChosenLanguage()
    {
        using var session = TestHttp.CreateDevelopmentSession(PakistanDomain);
        session.SetHeader("Accept-Language", "en");

        using var response = await session.PutAsync("/api/Localization/language", new { language = "ur" });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var setCookie = response.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("__Host-Culture=", StringComparison.Ordinal));
        setCookie.ShouldContain("secure", Case.Insensitive);
        setCookie.ShouldContain("httponly", Case.Insensitive);
        (await ReadAsync(session)).Language.ShouldBe("ur"); // the choice wins over the browser's language
    }

    [Test]
    public async Task ShouldRefuseALanguageTheSchoolDoesNotOffer()
    {
        using var session = TestHttp.CreateDevelopmentSession(PakistanDomain);

        using var response = await session.PutAsync("/api/Localization/language", new { language = "ar" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        session.Cookie("__Host-Culture").ShouldBeNull();
    }

    [Test]
    public async Task ShouldIgnoreACultureCookieForALanguageTheSchoolDoesNotOffer()
    {
        // A cookie copied from the UAE school, or written by hand, cannot switch a Pakistani school to Arabic.
        using var session = TestHttp.CreateDevelopmentSession(PakistanDomain);
        session.SetCookie("__Host-Culture", Uri.EscapeDataString("c=ar|uic=ar"));

        (await ReadAsync(session)).Language.ShouldBe("en");
    }

    [Test]
    public async Task ShouldRequireTheAntiforgeryTokenToChangeTheLanguage()
    {
        using var session = TestHttp.CreateDevelopmentSession(PakistanDomain);
        session.SetCookie("__Host-XSRF-TOKEN", "not-the-token");

        using var response = await session.PutAsync("/api/Localization/language", new { language = "ur" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        session.Cookie("__Host-Culture").ShouldBeNull();
    }

    [Test]
    public async Task ShouldWriteServerMessagesInTheRequestsLanguage()
    {
        using var session = TestHttp.CreateDevelopmentSession(PakistanDomain);
        session.SetHeader("Accept-Language", "ur");

        using var response = await session.PutAsync("/api/Localization/language", new { language = "ar" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
        problem.Title.ShouldBe("تصدیق میں ایک یا زیادہ غلطیاں ملیں۔");
    }

    [TestCase(PakistanDomain, "ur", "ar", "یہ اسکول یہ زبان پیش نہیں کرتا۔")]
    [TestCase(UaeDomain, "ar", "ur", "هذه المدرسة لا تقدّم هذه اللغة.")]
    [TestCase(UaeDomain, "en", "ur", "This school does not offer that language.")]
    public async Task ShouldWriteValidationErrorsInTheRequestsLanguage(string domain, string acceptLanguage, string requested, string expected)
    {
        using var session = TestHttp.CreateDevelopmentSession(domain);
        session.SetHeader("Accept-Language", acceptLanguage);

        using var response = await session.PutAsync("/api/Localization/language", new { language = requested });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!;
        problem.Errors["Language"].ShouldContain(expected);
    }

    [Test]
    public async Task ShouldWriteBuiltInValidationErrorsInUrdu()
    {
        // FluentValidation ships no Urdu; the portal adds it, with the property name translated too.
        using var session = TestHttp.CreateDevelopmentSession(PakistanDomain);
        session.SetHeader("Accept-Language", "ur");

        using var response = await session.PutAsync("/api/Localization/language", new { language = "" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!;
        problem.Errors["Language"].ShouldContain("'زبان' خالی نہیں ہونا چاہیے۔");
    }

    [Test]
    public async Task ShouldFollowAChangeToTheCountryOnTheNextRequest()
    {
        using var session = TestHttp.CreateDevelopmentSession(PakistanDomain);
        (await ReadAsync(session)).Languages.Count.ShouldBe(2);

        await using (var scope = FunctionalTestSetup.ScopeFactory.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EducationPortal.Infrastructure.Data.ApplicationDbContext>();
            var pakistan = await context.Countries.SingleAsync(c => c.Code == "PK");
            pakistan.Update("Pakistan", ["ur"], "ur", "Asia/Karachi", "PKR");
            await context.SaveChangesAsync();
        }

        var localization = await ReadAsync(session);
        localization.Languages.Select(l => l.Code).ShouldBe(["ur"]);
        localization.Language.ShouldBe("ur");
    }

    [Test]
    public async Task ShouldFollowTheSignedInUserToAnotherDevice()
    {
        await TestApp.RunAsUserAsync(_pakistanSchoolId, TeacherEmail, TestApp.AdministratorPassword, []);

        using (var laptop = TestHttp.CreateDevelopmentSession(PakistanDomain))
        {
            (await laptop.SignInAsync(TeacherEmail, TestApp.AdministratorPassword)).Dispose();
            using var changed = await laptop.PutAsync("/api/Localization/language", new { language = "ur" });
            changed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        // A new browser with no language cookie and an English browser language.
        using var phone = TestHttp.CreateDevelopmentSession(PakistanDomain);
        phone.SetHeader("Accept-Language", "en");
        (await ReadAsync(phone)).Language.ShouldBe("en"); // before sign-in the account is unknown

        (await phone.SignInAsync(TeacherEmail, TestApp.AdministratorPassword)).Dispose();

        (await ReadAsync(phone)).Language.ShouldBe("ur");
    }

    [Test]
    public async Task ShouldPreferTheUsersSavedLanguageOverThisBrowsersCookie()
    {
        await TestApp.RunAsUserAsync(_pakistanSchoolId, TeacherEmail, TestApp.AdministratorPassword, []);

        using var session = TestHttp.CreateDevelopmentSession(PakistanDomain);
        (await session.SignInAsync(TeacherEmail, TestApp.AdministratorPassword)).Dispose();
        (await session.PutAsync("/api/Localization/language", new { language = "ur" })).Dispose();

        // An older cookie from before, for example after the user changed the language on another device.
        session.SetCookie("__Host-Culture", Uri.EscapeDataString("c=en|uic=en"));

        (await ReadAsync(session)).Language.ShouldBe("ur");
    }

    [Test]
    public async Task ShouldIgnoreASavedLanguageTheSchoolNoLongerOffers()
    {
        await TestApp.RunAsUserAsync(_pakistanSchoolId, TeacherEmail, TestApp.AdministratorPassword, []);

        using var session = TestHttp.CreateDevelopmentSession(PakistanDomain);
        (await session.SignInAsync(TeacherEmail, TestApp.AdministratorPassword)).Dispose();
        (await session.PutAsync("/api/Localization/language", new { language = "ur" })).Dispose();

        await using (var scope = FunctionalTestSetup.ScopeFactory.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EducationPortal.Infrastructure.Data.ApplicationDbContext>();
            var pakistan = await context.Countries.SingleAsync(c => c.Code == "PK");
            pakistan.Update("Pakistan", ["en"], "en", "Asia/Karachi", "PKR");
            await context.SaveChangesAsync();
        }

        (await ReadAsync(session)).Language.ShouldBe("en");
    }

    [TestCase(PakistanDomain, "Asia/Dubai", "Asia/Dubai")] // the caller's own time zone wins
    [TestCase(PakistanDomain, null, "Asia/Karachi")] // else the school's country
    [TestCase(PakistanDomain, "Not/AZone", "Asia/Karachi")] // an unknown value is ignored
    [TestCase(PakistanDomain, "Pakistan Standard Time", "Asia/Karachi")] // only IANA ids are accepted
    [TestCase(NoCountryDomain, null, "UTC")] // no country: UTC
    public async Task ShouldUseTheCallersTimeZoneElseTheSchools(string domain, string? header, string expected)
    {
        using var session = TestHttp.CreateDevelopmentSession(domain);
        if (header is not null)
        {
            session.SetHeader("X-Time-Zone", header);
        }

        var localization = await ReadAsync(session);

        localization.UserTimeZone.ShouldBe(expected);
    }

    private static async Task<LocalizationDto> ReadAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/Localization");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<LocalizationDto>())!;
    }
}
