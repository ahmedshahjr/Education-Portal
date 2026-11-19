using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using EducationPortal.Application.Branding.Commands.UpdateBranding;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Users.Commands.SignIn;
using EducationPortal.Domain.Localization;
using FluentValidation;
using FluentValidation.Resources;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Common.Localization;

/// <summary>
/// Validation errors reach Arabic and Urdu users in their language (ADR-005): every server text has a translation with
/// the same placeholders, and FluentValidation's built-in messages exist in Urdu, which FluentValidation does not ship.
/// </summary>
public partial class ValidationLocalizationTests
{
    private static readonly string[] ResourceNames =
    [
        "EducationPortal.Application.Common.Localization.ValidationMessages",
        "EducationPortal.Application.Common.Localization.PropertyNames",
        "EducationPortal.Application.Common.Localization.NotificationMessages",
    ];

    private static IEnumerable<string> TranslatedLanguages => Language.All.Where(l => l != Language.English).Select(l => l.Code);

    private static readonly UpdateBrandingCommandValidator SharedBrandingValidator = new();

    [OneTimeSetUp]
    public void ConfigureValidation() => ValidationLocalization.Configure();

    [Test]
    public void EveryServerTextShouldBeTranslatedWithTheSamePlaceholders(
        [ValueSource(nameof(ResourceNames))] string resource,
        [ValueSource(nameof(TranslatedLanguages))] string language)
    {
        var manager = new ResourceManager(resource, typeof(ValidationLocalization).Assembly);
        var english = Entries(manager, CultureInfo.InvariantCulture);
        var translated = Entries(manager, CultureInfo.GetCultureInfo(language));

        english.ShouldNotBeEmpty();
        translated.Keys.ShouldBe(english.Keys, ignoreOrder: true);

        foreach (var (key, text) in english)
        {
            translated[key].ShouldNotBeNullOrWhiteSpace(key);
            Placeholders(translated[key]).ShouldBe(Placeholders(text), ignoreOrder: true, key);
        }
    }

    [Test]
    public void UrduShouldCoverFluentValidationsBuiltInMessagesWithTheSamePlaceholders()
    {
        var english = new LanguageManager();

        foreach (var (key, urdu) in UrduValidatorMessages.All)
        {
            var source = english.GetString(key, CultureInfo.GetCultureInfo("en"));
            source.ShouldNotBeNullOrEmpty($"FluentValidation has no message '{key}'.");
            Placeholders(urdu).ShouldBe(Placeholders(source), ignoreOrder: true, key);
        }

        UrduValidatorMessages.All.Keys.ShouldContain("NotEmptyValidator");
        UrduValidatorMessages.All.Keys.ShouldContain("MaximumLengthValidator");
        UrduValidatorMessages.All.Keys.ShouldContain("EmailValidator");
    }

    [TestCase("ur-PK", "'ای میل' خالی نہیں ہونا چاہیے۔")]
    [TestCase("ar-AE", "'البريد الإلكتروني' لا يجب أن يكون فارغاً.")]
    [TestCase("en", "'Email' must not be empty.")]
    public async Task ShouldWriteBuiltInMessagesAndPropertyNamesInTheCurrentLanguage(string culture, string expected)
    {
        var errors = await ValidateInAsync(culture, new SignInCommandValidator(), new SignInCommand { Email = string.Empty, Password = "x" });

        errors.ShouldContain(expected);
    }

    [TestCase("ur-PK", "نمایاں رنگ #rrggbb کی شکل میں ہونا چاہیے۔", "لوگو PNG، JPEG یا WebP تصویر ہونا چاہیے جس کا سائز زیادہ سے زیادہ 512 KB ہو۔")]
    [TestCase("ar-AE", "يجب أن يكون لون التمييز بالصيغة #rrggbb.", "يجب أن يكون الشعار صورة PNG أو JPEG أو WebP بحجم 512 كيلوبايت على الأكثر.")]
    [TestCase("en", "The accent colour must have the form #rrggbb.", "The logo must be a PNG, JPEG or WebP image of at most 512 KB.")]
    public async Task ShouldWriteOwnMessagesInTheLanguageOfEachValidation(string culture, string colourMessage, string logoMessage)
    {
        // One validator instance serves requests in different languages, so the message is read on each validation.
        var invalid = new UpdateBrandingCommand { AccentColor = "red", Logo = [1, 2, 3] };

        var errors = await ValidateInAsync(culture, SharedBrandingValidator, invalid);

        errors.ShouldContain(colourMessage);
        errors.ShouldContain(logoMessage);
    }

    private static async Task<IReadOnlyList<string>> ValidateInAsync<T>(string culture, IValidator<T> validator, T instance)
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            var result = await validator.ValidateAsync(instance);
            return result.Errors.Select(e => e.ErrorMessage).ToList();
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    private static Dictionary<string, string> Entries(ResourceManager manager, CultureInfo culture)
    {
        var set = manager.GetResourceSet(culture, createIfNotExists: true, tryParents: false)
            ?? throw new InvalidOperationException($"No resources for '{culture.Name}'.");

        return set.Cast<DictionaryEntry>().ToDictionary(e => (string)e.Key, e => (string)e.Value!);
    }

    private static List<string> Placeholders(string text) =>
        PlaceholderPattern().Matches(text).Select(m => m.Value).ToList();

    [GeneratedRegex(@"\{[A-Za-z0-9]+\}")]
    private static partial Regex PlaceholderPattern();
}
