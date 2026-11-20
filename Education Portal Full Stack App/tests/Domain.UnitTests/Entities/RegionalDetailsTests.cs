using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class CountryRegionalSettingsTests
{
    private static Country Pakistan() => Country.Create("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");

    [Test]
    public void ShouldStartWithTwoDecimalsMondayAndASaturdaySundayWeekend()
    {
        var country = Pakistan();

        country.CurrencyDecimals.ShouldBe(2);
        country.PhonePattern.ShouldBeNull();
        country.WeekStartDay.ShouldBe(DayOfWeek.Monday);
        country.WeekendDays.ShouldBe([DayOfWeek.Saturday, DayOfWeek.Sunday]);
    }

    [Test]
    public void ShouldSetTheSettingsAndOrderTheWeekendFromTheWeekStart()
    {
        var country = Pakistan();

        country.SetRegionalSettings(0, @" ^03\d{9}$ ", DayOfWeek.Sunday, [DayOfWeek.Saturday, DayOfWeek.Friday, DayOfWeek.Friday]);

        country.CurrencyDecimals.ShouldBe(0);
        country.PhonePattern.ShouldBe(@"^03\d{9}$");
        country.WeekStartDay.ShouldBe(DayOfWeek.Sunday);
        country.WeekendDays.ShouldBe([DayOfWeek.Friday, DayOfWeek.Saturday]);
    }

    [Test]
    public void ShouldAllowNoWeekendAndClearThePattern()
    {
        var country = Pakistan();
        country.SetRegionalSettings(2, "^1$", DayOfWeek.Monday, [DayOfWeek.Sunday]);

        country.SetRegionalSettings(2, "  ", DayOfWeek.Monday, []);

        country.PhonePattern.ShouldBeNull();
        country.WeekendDays.ShouldBeEmpty();
    }

    [TestCase(-1)]
    [TestCase(Country.MaxCurrencyDecimals + 1)]
    public void ShouldRefuseCurrencyDecimalsOutOfRange(int decimals) =>
        Should.Throw<ArgumentOutOfRangeException>(() => Pakistan().SetRegionalSettings(decimals, null, DayOfWeek.Monday, []));

    [TestCase("[unclosed")]
    [TestCase("(?<")]
    public void ShouldRefuseAPatternThatDoesNotCompile(string pattern) =>
        Should.Throw<ArgumentException>(() => Pakistan().SetRegionalSettings(2, pattern, DayOfWeek.Monday, []));

    [Test]
    public void ShouldRefuseAPatternOverTheLimit()
    {
        Country.IsValidPhonePattern(new string('a', Country.PhonePatternMaxLength)).ShouldBeTrue();
        Country.IsValidPhonePattern(new string('a', Country.PhonePatternMaxLength + 1)).ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseMoreThanThreeWeekendDays() =>
        Should.Throw<ArgumentOutOfRangeException>(() => Pakistan().SetRegionalSettings(2, null, DayOfWeek.Monday,
            [DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday]));

    [Test]
    public void ShouldRefuseAnUnknownDay() =>
        Should.Throw<ArgumentOutOfRangeException>(() => Pakistan().SetRegionalSettings(2, null, (DayOfWeek)7, []));
}

public class SchoolDetailsTests
{
    private static SchoolDetails Valid(string? phone = "+92 21 1234567", string? email = "office@school.example", string? website = "https://school.example", int month = 4, int? cityId = 5) =>
        SchoolDetails.Create(" مدرسہ ", " REG-1 ", " Sindh board ", " NTN-9 ", " 1 Main road ", cityId, phone, email, website, month);

    [Test]
    public void ShouldTrimTheValues()
    {
        var details = Valid();

        details.LocalName.ShouldBe("مدرسہ");
        details.RegistrationNumber.ShouldBe("REG-1");
        details.Address.ShouldBe("1 Main road");
        details.AcademicYearStartMonth.ShouldBe(4);
    }

    [Test]
    public void ShouldTurnBlanksIntoNull()
    {
        var details = SchoolDetails.Create(" ", null, "", null, null, null, " ", "", null, 8);

        details.LocalName.ShouldBeNull();
        details.Phone.ShouldBeNull();
        details.Email.ShouldBeNull();
    }

    [TestCase(0)]
    [TestCase(13)]
    public void ShouldRefuseAMonthOutOfRange(int month) =>
        Should.Throw<ArgumentOutOfRangeException>(() => Valid(month: month));

    [TestCase(1)]
    [TestCase(12)]
    public void ShouldAllowTheFirstAndLastMonth(int month) => Valid(month: month).AcademicYearStartMonth.ShouldBe(month);

    [Test]
    public void ShouldRefuseBadContactDetails()
    {
        Should.Throw<ArgumentException>(() => Valid(phone: "123"));
        Should.Throw<ArgumentException>(() => Valid(email: "not an email"));
        Should.Throw<ArgumentException>(() => Valid(website: "ftp://school.example"));
        Should.Throw<ArgumentOutOfRangeException>(() => Valid(cityId: 0));
    }

    [Test]
    public void ShouldReplaceTheSchoolsDetails()
    {
        var school = School.Create("Demo", null);

        school.UpdateDetails(Valid());

        school.LocalName.ShouldBe("مدرسہ");
        school.CityId.ShouldBe(5);
        school.AcademicYearStartMonth.ShouldBe(4);
    }
}

public class CampusProfileTests
{
    [Test]
    public void ShouldStartCoEducationalWithoutALocalName()
    {
        var campus = Campus.Create(1, "North", "0002");

        campus.GenderPolicy.ShouldBe(SchoolGenderPolicy.CoEducation);
        campus.LocalName.ShouldBeNull();
    }

    [Test]
    public void ShouldSetTheLocalNameAndGenderPolicy()
    {
        var campus = Campus.Create(1, "North", "0002");

        campus.SetProfile(" شمالی ", SchoolGenderPolicy.GirlsOnly);

        campus.LocalName.ShouldBe("شمالی");
        campus.GenderPolicy.ShouldBe(SchoolGenderPolicy.GirlsOnly);
    }

    [Test]
    public void ShouldRefuseAnUnknownPolicyAndALongName()
    {
        var campus = Campus.Create(1, "North", "0002");

        Should.Throw<ArgumentOutOfRangeException>(() => campus.SetProfile(null, (SchoolGenderPolicy)0));
        Should.Throw<ArgumentOutOfRangeException>(() => campus.SetProfile(new string('a', Campus.LocalNameMaxLength + 1), SchoolGenderPolicy.CoEducation));
    }
}
