using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class BranchTests
{
    [Test]
    public void ShouldCreateEverySchoolWithExactlyOneMainCampusWithCodeZeroZeroZeroOne()
    {
        var school = School.Create("Demo School", "demo.example.com");

        var main = school.Branches.ShouldHaveSingleItem();
        main.Kind.ShouldBe(BranchKind.Main);
        main.Name.ShouldBe(Branch.MainName);
        main.Code.ShouldBe("0001");
    }

    [Test]
    public void ShouldGiveTheMasterTenantItsMainCampusToo()
    {
        var master = School.CreateMaster("Education Portal", "portal.example.com");

        master.Branches.ShouldHaveSingleItem().Kind.ShouldBe(BranchKind.Main);
    }

    [TestCase(" 0042 ", "0042")]
    [TestCase("9999", "9999")]
    public void ShouldTrimAFourDigitCode(string code, string expected) =>
        Branch.NormalizeCode(code).ShouldBe(expected);

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("123")]
    [TestCase("12345")]
    [TestCase("12A4")]
    [TestCase("KHI1")]
    [TestCase("12-4")]
    public void ShouldRefuseACodeThatIsNotFourDigits(string code) =>
        Should.Throw<ArgumentException>(() => Branch.NormalizeCode(code));

    [Test]
    public void ShouldKeepTheCodeWhenACampusIsRenamed()
    {
        var campus = Branch.Create(3, "North", "0007");

        campus.Rename("  North wing  ");

        campus.Name.ShouldBe("North wing");
        campus.Code.ShouldBe("0007");
    }

    [Test]
    public void ShouldAcceptANameOfExactlyTheMaximumLengthAndRefuseOneOverIt()
    {
        var campus = Branch.Create(3, "North", "0007");

        campus.Rename(new string('a', Branch.NameMaxLength));
        campus.Name.Length.ShouldBe(Branch.NameMaxLength);
        Should.Throw<ArgumentOutOfRangeException>(() => campus.Rename(new string('a', Branch.NameMaxLength + 1)));
    }

    [Test]
    public void ShouldCreateANonMainCampusKeepingItsCode()
    {
        var campus = Branch.Create(3, "North", "0007");

        campus.Kind.ShouldBe(BranchKind.Other);
        campus.Code.ShouldBe("0007");
    }

    [Test]
    public void ShouldNeverDeactivateTheMainCampus()
    {
        var main = School.Create("Demo School", "demo.example.com").Branches.Single();

        Should.Throw<InvalidOperationException>(() => main.Deactivate());
    }
}
