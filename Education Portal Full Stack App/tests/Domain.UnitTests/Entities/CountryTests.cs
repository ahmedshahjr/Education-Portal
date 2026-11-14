using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Localization;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class CountryTests
{
    [Test]
    public void ShouldCreateACountryWithNormalisedCodes()
    {
        var country = Country.Create("pk", " Pakistan ", ["EN", "ur", "en"], "En", "Asia/Karachi", "PKR");

        country.Code.ShouldBe("PK");
        country.Name.ShouldBe("Pakistan");
        country.Languages.ShouldBe(["en", "ur"]);
        country.DefaultLanguage.ShouldBe("en");
        country.Offers("UR").ShouldBeTrue();
        country.Offers("ar").ShouldBeFalse();
    }

    [Test]
    public void ShouldReplaceTheLanguagesOnUpdate()
    {
        var country = Country.Create("AE", "United Arab Emirates", ["en"], "en", "Asia/Dubai", "AED");

        country.Update("UAE", ["ar", "en"], "ar", "Asia/Dubai", "AED");

        country.Languages.ShouldBe(["ar", "en"]);
        country.DefaultLanguage.ShouldBe("ar");
        country.Code.ShouldBe("AE");
    }

    [Test]
    public void ShouldCreateTheUnitedStatesWithEnglishOnly()
    {
        var country = Country.Create("us", "United States", ["en"], "en", "America/New_York", "USD");

        country.Code.ShouldBe("US");
        country.Languages.ShouldBe(["en"]);
        country.TimeZoneId.ShouldBe("America/New_York");
        country.CurrencyCode.ShouldBe("USD");
        country.Offers("ur").ShouldBeFalse();
    }

    [TestCase("PAK")]
    [TestCase("P1")]
    [TestCase("")]
    public void ShouldRefuseAnInvalidCode(string code)
    {
        Should.Throw<ArgumentException>(() => Country.Create(code, "Pakistan", ["en"], "en", "Asia/Karachi", "PKR"));
    }

    [Test]
    public void ShouldRefuseALanguageWithoutTranslations()
    {
        Should.Throw<ArgumentException>(() => Country.Create("FR", "France", ["fr"], "fr", "Europe/Paris", "EUR"));
    }

    [Test]
    public void ShouldRefuseNoLanguages()
    {
        Should.Throw<ArgumentException>(() => Country.Create("PK", "Pakistan", [], "en", "Asia/Karachi", "PKR"));
    }

    [Test]
    public void ShouldRefuseADefaultLanguageTheCountryDoesNotOffer()
    {
        Should.Throw<ArgumentException>(() => Country.Create("PK", "Pakistan", ["en", "ur"], "ar", "Asia/Karachi", "PKR"));
    }

    [TestCase("Pakistan Standard Time")]
    [TestCase("Eastern Standard Time")]
    [TestCase("Asia/Nowhere")]
    [TestCase("")]
    public void ShouldRefuseATimeZoneThatIsNotIana(string timeZoneId)
    {
        Should.Throw<ArgumentException>(() => Country.Create("PK", "Pakistan", ["en"], "en", timeZoneId, "PKR"));
    }

    [TestCase("pkr")]
    [TestCase("PK")]
    [TestCase("RS.")]
    public void ShouldRefuseAnInvalidCurrency(string currency)
    {
        Should.Throw<ArgumentException>(() => Country.Create("PK", "Pakistan", ["en"], "en", "Asia/Karachi", currency));
    }

    [Test]
    public void ShouldKnowWhichLanguagesAreRightToLeft()
    {
        Language.Find("ar")!.IsRightToLeft.ShouldBeTrue();
        Language.Find("UR")!.IsRightToLeft.ShouldBeTrue();
        Language.Find("en")!.IsRightToLeft.ShouldBeFalse();
        Language.Find("fr").ShouldBeNull();
    }

    [Test]
    public void ShouldSetTheReferenceCodesFromTheLocationData()
    {
        var country = Country.Create("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");

        country.SetReferenceCodes("pak", "+92", " پاکستان ");

        country.Iso3.ShouldBe("PAK");
        country.PhoneCode.ShouldBe("92");
        country.NativeName.ShouldBe("پاکستان");
        country.Name.ShouldBe("Pakistan");
        country.Languages.ShouldBe(["en", "ur"]);
    }

    [Test]
    public void ShouldAcceptADashedPhoneCodeAndNoNativeName()
    {
        var country = Country.Create("US", "United States", ["en"], "en", "America/New_York", "USD");

        country.SetReferenceCodes("USA", "1-684", null);

        country.PhoneCode.ShouldBe("1-684");
        country.NativeName.ShouldBeNull();
    }

    [TestCase("PK", "92")]
    [TestCase("PAKI", "92")]
    [TestCase("P4K", "92")]
    [TestCase("PAK", "9a")]
    [TestCase("PAK", "12345")]
    public void ShouldRefuseInvalidReferenceCodes(string iso3, string phoneCode)
    {
        var country = Country.Create("PK", "Pakistan", ["en"], "en", "Asia/Karachi", "PKR");

        Should.Throw<ArgumentException>(() => country.SetReferenceCodes(iso3, phoneCode, null));
    }
}
