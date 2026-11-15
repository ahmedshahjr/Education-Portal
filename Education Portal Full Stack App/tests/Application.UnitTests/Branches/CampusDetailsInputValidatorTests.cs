using EducationPortal.Application.Branches.Common;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Branches;

public class CampusDetailsInputValidatorTests
{
    private readonly CampusDetailsInputValidator _validator = new();

    [Test]
    public void ShouldAcceptAFullyFilledCampus()
    {
        var result = _validator.Validate(new CampusDetailsInput
        {
            Address = "12 Main Road",
            StateId = Guid.NewGuid(),
            CityId = Guid.NewGuid(),
            PostalCode = "75500",
            Phone = "+92 21 1234567",
            Website = "https://karachi.example.com",
            SchoolNumber = "SN-1",
            ShortName = "KHI",
            PrincipalId = Guid.NewGuid(),
        });

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldAcceptAnEmptyCampus()
    {
        _validator.Validate(new CampusDetailsInput()).IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseACityWithoutAState()
    {
        var result = _validator.Validate(new CampusDetailsInput { CityId = Guid.NewGuid() });

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CampusDetailsInput.CityId));
    }

    [TestCase("123456", false)]
    [TestCase("1234567", true)]
    [TestCase("+123456789012345", true)]
    [TestCase("+1234567890123456", false)]
    public void ShouldCheckThePhoneDigits(string phone, bool valid)
    {
        var result = _validator.Validate(new CampusDetailsInput { Phone = phone });

        result.IsValid.ShouldBe(valid);
    }

    [TestCase("ABCDE12345", true)]
    [TestCase("ABCDE123456", false)]
    [TestCase("12#45", false)]
    public void ShouldCheckThePostalCode(string code, bool valid)
    {
        var result = _validator.Validate(new CampusDetailsInput { PostalCode = code });

        result.IsValid.ShouldBe(valid);
    }

    [TestCase("https://school.example.com", true)]
    [TestCase("ftp://school.example.com", false)]
    [TestCase("school.example.com", false)]
    public void ShouldCheckTheWebsite(string website, bool valid)
    {
        var result = _validator.Validate(new CampusDetailsInput { Website = website });

        result.IsValid.ShouldBe(valid);
    }

    [TestCase(25, true)]
    [TestCase(26, false)]
    public void ShouldLimitTheShortName(int length, bool valid)
    {
        var result = _validator.Validate(new CampusDetailsInput { ShortName = new string('s', length) });

        result.IsValid.ShouldBe(valid);
    }

    [TestCase(50, true)]
    [TestCase(51, false)]
    public void ShouldLimitTheSchoolNumber(int length, bool valid)
    {
        var result = _validator.Validate(new CampusDetailsInput { SchoolNumber = new string('n', length) });

        result.IsValid.ShouldBe(valid);
    }
}
