using EducationPortal.Application.DevelopmentData.Commands.GenerateDevelopmentData;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.DevelopmentData;

public class GenerateDevelopmentDataCommandValidatorTests
{
    private readonly GenerateDevelopmentDataCommandValidator _validator = new();

    private static GenerateDevelopmentDataCommand Valid() => new()
    {
        Schools = 1,
        CampusesPerSchool = 1,
        PeoplePerCampus = 1,
        CountryCode = "PK",
    };

    [Test]
    public void ShouldAcceptAValidRequest()
    {
        _validator.Validate(Valid()).IsValid.ShouldBeTrue();
    }

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(GenerateDevelopmentDataCommandValidator.MaximumSchools, true)]
    [TestCase(GenerateDevelopmentDataCommandValidator.MaximumSchools + 1, false)]
    public void ShouldLimitSchools(int schools, bool valid)
    {
        _validator.Validate(Valid() with { Schools = schools }).IsValid.ShouldBe(valid);
    }

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(GenerateDevelopmentDataCommandValidator.MaximumCampusesPerSchool, true)]
    [TestCase(GenerateDevelopmentDataCommandValidator.MaximumCampusesPerSchool + 1, false)]
    public void ShouldLimitCampusesPerSchool(int campuses, bool valid)
    {
        _validator.Validate(Valid() with { CampusesPerSchool = campuses }).IsValid.ShouldBe(valid);
    }

    [TestCase(-1, false)]
    [TestCase(0, true)]
    [TestCase(GenerateDevelopmentDataCommandValidator.MaximumPeoplePerCampus, true)]
    [TestCase(GenerateDevelopmentDataCommandValidator.MaximumPeoplePerCampus + 1, false)]
    public void ShouldLimitPeoplePerCampus(int people, bool valid)
    {
        _validator.Validate(Valid() with { PeoplePerCampus = people }).IsValid.ShouldBe(valid);
    }

    [Test]
    public void ShouldAcceptExactlyTheMaximumNumberOfPeople()
    {
        // 500 schools x 50 campuses x 40 people = 1,000,000.
        _validator.Validate(new GenerateDevelopmentDataCommand
        {
            Schools = 500, CampusesPerSchool = 50, PeoplePerCampus = 40, CountryCode = "PK",
        }).IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRejectMoreThanTheMaximumNumberOfPeopleInOneCall()
    {
        // 500 x 50 x 41 = 1,025,000: each value is in range, the product is not.
        _validator.Validate(new GenerateDevelopmentDataCommand
        {
            Schools = 500, CampusesPerSchool = 50, PeoplePerCampus = 41, CountryCode = "PK",
        }).IsValid.ShouldBeFalse();
    }

    [TestCase("")]
    [TestCase("P")]
    [TestCase("PKR")]
    public void ShouldRequireATwoLetterCountryCode(string code)
    {
        _validator.Validate(Valid() with { CountryCode = code }).Errors.ShouldContain(e => e.PropertyName == nameof(GenerateDevelopmentDataCommand.CountryCode));
    }
}
