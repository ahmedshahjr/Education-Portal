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

    [SetUp]
    public async Task AddSchools()
    {
        var pakistan = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");
        var uae = await TestApp.AddCountryAsync("AE", "United Arab Emirates", ["en", "ar"], "ar", "Asia/Dubai", "AED");
        await TestApp.AddSchoolAsync("Lahore School", PakistanDomain, pakistan.Id);
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

    private static async Task<LocalizationDto> ReadAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/Localization");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<LocalizationDto>())!;
    }
}
