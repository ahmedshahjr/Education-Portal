using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class CampusTests
{
    [Test]
    public void ShouldCreateEverySchoolWithExactlyOneMainCampusWithCodeZeroZeroZeroOne()
    {
        var school = School.Create("Demo School", "demo.example.com");

        var main = school.Campuses.ShouldHaveSingleItem();
        main.Kind.ShouldBe(CampusKind.Main);
        main.Name.ShouldBe(Campus.MainName);
        main.Code.ShouldBe("0001");
    }

    [Test]
    public void ShouldGiveTheMasterTenantItsMainCampusToo()
    {
        var master = School.CreateMaster("Education Portal", "portal.example.com");

        master.Campuses.ShouldHaveSingleItem().Kind.ShouldBe(CampusKind.Main);
    }

    [TestCase(" 0042 ", "0042")]
    [TestCase("9999", "9999")]
    public void ShouldTrimAFourDigitCode(string code, string expected) =>
        Campus.NormalizeCode(code).ShouldBe(expected);

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("123")]
    [TestCase("12345")]
    [TestCase("12A4")]
    [TestCase("KHI1")]
    [TestCase("12-4")]
    public void ShouldRefuseACodeThatIsNotFourDigits(string code) =>
        Should.Throw<ArgumentException>(() => Campus.NormalizeCode(code));

    [Test]
    public void ShouldKeepTheCodeWhenACampusIsRenamed()
    {
        var campus = Campus.Create(3, "North", "0007");

        campus.Rename("  North wing  ");

        campus.Name.ShouldBe("North wing");
        campus.Code.ShouldBe("0007");
    }

    [Test]
    public void ShouldAcceptANameOfExactlyTheMaximumLengthAndRefuseOneOverIt()
    {
        var campus = Campus.Create(3, "North", "0007");

        campus.Rename(new string('a', Campus.NameMaxLength));
        campus.Name.Length.ShouldBe(Campus.NameMaxLength);
        Should.Throw<ArgumentOutOfRangeException>(() => campus.Rename(new string('a', Campus.NameMaxLength + 1)));
    }

    [Test]
    public void ShouldCreateANonMainCampusKeepingItsCode()
    {
        var campus = Campus.Create(3, "North", "0007");

        campus.Kind.ShouldBe(CampusKind.Other);
        campus.Code.ShouldBe("0007");
    }

    [Test]
    public void ShouldNeverDeactivateTheMainCampus()
    {
        var main = School.Create("Demo School", "demo.example.com").Campuses.Single();

        Should.Throw<InvalidOperationException>(() => main.Deactivate());
    }
}
