using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class BranchTests
{
    [Test]
    public void ShouldCreateEverySchoolWithExactlyOneMainBranch()
    {
        var school = School.Create("Demo School", "demo.example.com");

        var main = school.Branches.ShouldHaveSingleItem();
        main.Kind.ShouldBe(BranchKind.Main);
        main.Name.ShouldBe(Branch.MainName);
        main.Code.ShouldBe(Branch.MainCode);
    }

    [Test]
    public void ShouldGiveTheMasterTenantItsMainBranchToo()
    {
        var master = School.CreateMaster("Education Portal", "portal.example.com");

        master.Branches.ShouldHaveSingleItem().Kind.ShouldBe(BranchKind.Main);
    }

    [Test]
    public void ShouldTrimAndUpperCaseACode()
    {
        Branch.NormalizeCode("  north-1 ").ShouldBe("NORTH-1");
    }

    [Test]
    public void ShouldAcceptACodeOfExactlyTheMaximumLength()
    {
        var code = new string('A', Branch.CodeMaxLength);

        Branch.NormalizeCode(code).ShouldBe(code);
    }

    [Test]
    public void ShouldRejectAnEmptyCode()
    {
        Should.Throw<ArgumentException>(() => Branch.NormalizeCode(" "));
    }

    [Test]
    public void ShouldRejectACodeLongerThanTheMaximum()
    {
        Should.Throw<ArgumentException>(() => Branch.NormalizeCode(new string('A', Branch.CodeMaxLength + 1)));
    }

    [TestCase("NORTH WING")]
    [TestCase("NORTH_1")]
    [TestCase("NORTH.1")]
    [TestCase("NÖRD")]
    public void ShouldRejectCharactersOtherThanLettersDigitsAndHyphens(string code)
    {
        Should.Throw<ArgumentException>(() => Branch.NormalizeCode(code));
    }

    [Test]
    public void ShouldCreateANonMainBranchWithItsCode()
    {
        var branch = Branch.Create(3, "North", "north");

        branch.Kind.ShouldBe(BranchKind.Other);
        branch.Code.ShouldBe("NORTH");
    }
}
