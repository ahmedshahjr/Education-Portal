using EducationPortal.Application.SchoolPublicPage.Commands.UpdateSchoolProfile;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.SchoolPublicPage;

public class UpdateSchoolProfileCommandValidatorTests
{
    private readonly UpdateSchoolProfileCommandValidator _validator = new();

    private static UpdateSchoolProfileCommand Valid() => new()
    {
        Tagline = "Learning with care",
        About = "A school for boys and girls.",
        EstablishedYear = 1990,
        GenderPolicy = SchoolGenderPolicy.CoEducation,
        VisitingHours = "Monday to Friday",
        VirtualTourUrl = "https://tour.example.com",
    };

    [Test]
    public void ShouldAcceptAFullPage() => _validator.Validate(Valid()).IsValid.ShouldBeTrue();

    [Test]
    public void ShouldAcceptAPageWithoutOptionalFields() =>
        _validator.Validate(Valid() with { EstablishedYear = null, VisitingHours = null, VirtualTourUrl = null }).IsValid.ShouldBeTrue();

    [TestCase("")]
    [TestCase("   ")]
    public void ShouldRefuseABlankTagline(string tagline) =>
        _validator.Validate(Valid() with { Tagline = tagline }).IsValid.ShouldBeFalse();

    [Test]
    public void ShouldAcceptATaglineAtTheLimitAndRefuseOneOverIt()
    {
        _validator.Validate(Valid() with { Tagline = new string('t', SchoolProfile.TaglineMaxLength) }).IsValid.ShouldBeTrue();
        _validator.Validate(Valid() with { Tagline = new string('t', SchoolProfile.TaglineMaxLength + 1) }).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldAcceptAnAboutTextAtTheLimitAndRefuseOneOverIt()
    {
        _validator.Validate(Valid() with { About = new string('a', SchoolProfile.AboutMaxLength) }).IsValid.ShouldBeTrue();
        _validator.Validate(Valid() with { About = new string('a', SchoolProfile.AboutMaxLength + 1) }).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseABlankAboutText() =>
        _validator.Validate(Valid() with { About = " " }).IsValid.ShouldBeFalse();

    [TestCase(SchoolProfile.MinimumEstablishedYear, true)]
    [TestCase(SchoolProfile.MinimumEstablishedYear - 1, false)]
    public void ShouldCheckTheLowestEstablishedYear(int year, bool valid) =>
        _validator.Validate(Valid() with { EstablishedYear = year }).IsValid.ShouldBe(valid);

    [Test]
    public void ShouldAcceptThisYearAndRefuseNextYear()
    {
        var thisYear = DateTimeOffset.UtcNow.Year;
        _validator.Validate(Valid() with { EstablishedYear = thisYear }).IsValid.ShouldBeTrue();
        _validator.Validate(Valid() with { EstablishedYear = thisYear + 1 }).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseAnUndefinedGenderPolicy() =>
        _validator.Validate(Valid() with { GenderPolicy = (SchoolGenderPolicy)99 }).IsValid.ShouldBeFalse();

    [Test]
    public void ShouldAcceptVisitingHoursAtTheLimitAndRefuseOneOverIt()
    {
        _validator.Validate(Valid() with { VisitingHours = new string('h', SchoolProfile.VisitingHoursMaxLength) }).IsValid.ShouldBeTrue();
        _validator.Validate(Valid() with { VisitingHours = new string('h', SchoolProfile.VisitingHoursMaxLength + 1) }).IsValid.ShouldBeFalse();
    }

    [TestCase("ftp://tour.example.com", false)]
    [TestCase("javascript:alert(1)", false)]
    [TestCase("not a url", false)]
    [TestCase("https://tour.example.com/school", true)]
    public void ShouldAcceptOnlyAbsoluteWebAddressesForTheTour(string url, bool valid) =>
        _validator.Validate(Valid() with { VirtualTourUrl = url }).IsValid.ShouldBe(valid);
}
