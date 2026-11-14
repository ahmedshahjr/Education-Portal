using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class SchoolCampusCodeTests
{
    [Test]
    public void ShouldShowTheCodeAloneWhilePrefixIsOff()
    {
        var school = School.Create("School", "school.example.com");

        school.CampusDisplayCode("0002").ShouldBe("0002");
    }

    [Test]
    public void ShouldShowThePrefixAndTheCodeWhenThePrefixIsOn()
    {
        var school = School.Create("School", "school.example.com");

        school.ConfigureCampusCodes(prefixEnabled: true, prefix: " khi ");

        school.CampusCodePrefixEnabled.ShouldBeTrue();
        school.CampusCodePrefix.ShouldBe("KHI");
        school.CampusDisplayCode("0002").ShouldBe("KHI0002");
    }

    [Test]
    public void ShouldKeepThePrefixWhileItIsSwitchedOff()
    {
        var school = School.Create("School", "school.example.com");
        school.ConfigureCampusCodes(prefixEnabled: true, prefix: "KHI");

        school.ConfigureCampusCodes(prefixEnabled: false, prefix: "KHI");

        school.CampusDisplayCode("0002").ShouldBe("0002");
        school.CampusCodePrefix.ShouldBe("KHI");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ShouldRequireAPrefixWhenItIsOn(string? prefix) =>
        Should.Throw<ArgumentException>(() => School.Create("School", "school.example.com").ConfigureCampusCodes(prefixEnabled: true, prefix: prefix));

    [TestCase("ABCDEFG")]
    [TestCase("AB-C")]
    [TestCase("AB C")]
    public void ShouldRefuseAPrefixThatIsNotSixOrFewerLettersOrDigits(string prefix) =>
        Should.Throw<ArgumentException>(() => School.Create("School", "school.example.com").ConfigureCampusCodes(prefixEnabled: true, prefix: prefix));

    [Test]
    public void ShouldAcceptAPrefixOfExactlyTheMaximumLength() =>
        School.Create("School", "school.example.com")
            .ConfigureCampusCodes(prefixEnabled: true, prefix: new string('A', School.CampusCodePrefixMaxLength));
}
