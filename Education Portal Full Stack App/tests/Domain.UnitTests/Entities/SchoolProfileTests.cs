using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class SchoolProfileTests
{
    [Test]
    public void ShouldChangeTheNameAndTheDomainOfASchool()
    {
        var school = School.Create("Old name", "old.example.com");

        school.UpdateProfile("  New name  ", " New.Example.COM. ");

        school.Name.ShouldBe("New name");
        school.Domain.ShouldBe("new.example.com");
    }

    [Test]
    public void ShouldRenameTheMasterTenantButNotChangeItsDomain()
    {
        var master = School.CreateMaster("Education Portal", "portal.example.com");

        master.UpdateProfile("Platform", "PORTAL.example.com");

        master.Name.ShouldBe("Platform");
        master.Domain.ShouldBe("portal.example.com");
        Should.Throw<InvalidOperationException>(() => master.UpdateProfile("Platform", "other.example.com"));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void ShouldRefuseABlankName(string name) =>
        Should.Throw<ArgumentException>(() => School.Create("School", "school.example.com").UpdateProfile(name, "school.example.com"));

    [Test]
    public void ShouldRefuseANameLongerThanTheMaximum() =>
        Should.Throw<ArgumentOutOfRangeException>(() =>
            School.Create("School", "school.example.com").UpdateProfile(new string('a', School.NameMaxLength + 1), "school.example.com"));

    [TestCase("https://school.example.com")]
    [TestCase("school.example.com:8080")]
    [TestCase("school example.com")]
    public void ShouldRefuseADomainThatIsNotAHostName(string domain) =>
        Should.Throw<ArgumentException>(() => School.Create("School", "school.example.com").UpdateProfile("School", domain));
}
