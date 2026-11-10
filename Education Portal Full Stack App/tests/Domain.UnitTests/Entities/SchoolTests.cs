using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class SchoolTests
{
    [Test]
    public void ShouldCreateTheMasterTenant()
    {
        var master = School.CreateMaster("Education Portal", "Portal.Example.com.");

        master.IsMaster.ShouldBeTrue();
        master.Domain.ShouldBe("portal.example.com");
        master.ThemePreset.ShouldBe(ThemePreset.Cyan);
    }

    [Test]
    public void ShouldCreateAnOrdinarySchool()
    {
        School.Create("School One", "school1.com").IsMaster.ShouldBeFalse();
    }

    [TestCase("school1.com:443")]
    [TestCase("https://school1.com")]
    [TestCase("school1.com/path")]
    [TestCase("-school1.com")]
    [TestCase("school one.com")]
    [TestCase("school1..com")]
    public void ShouldRefuseADomainThatIsNotAHostName(string domain)
    {
        Should.Throw<ArgumentException>(() => School.Create("School One", domain));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void ShouldRequireAName(string name)
    {
        Should.Throw<ArgumentException>(() => School.Create(name, "school1.com"));
    }

    [Test]
    public void ShouldUpdateBranding()
    {
        var school = School.Create("School One", "school1.com");

        school.UpdateBranding(ThemePreset.Coral, HexColor.From("#112233"));

        school.ThemePreset.ShouldBe(ThemePreset.Coral);
        school.SidebarColor!.Value.ShouldBe("#112233");
    }

    [Test]
    public void ShouldRefuseAnUnknownThemePreset()
    {
        var school = School.Create("School One", "school1.com");

        Should.Throw<ArgumentOutOfRangeException>(() => school.UpdateBranding((ThemePreset)99, null));
    }

    [Test]
    public void ShouldAssignAndClearACountry()
    {
        var school = School.Create("School One", "school1.com");

        school.AssignCountry(3);
        school.CountryId.ShouldBe(3);

        school.AssignCountry(null);
        school.CountryId.ShouldBeNull();

        Should.Throw<ArgumentOutOfRangeException>(() => school.AssignCountry(0));
    }
}
