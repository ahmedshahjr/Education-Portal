using EducationPortal.Application.Campuses.Common;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Campuses;

public class CampusDetailsInputValidatorTests
{
    private readonly CampusDetailsInputValidator _validator = new();

    private static CampusDetailsInput Valid() => new()
    {
        Address = "12 Main Road",
        StateId = Guid.NewGuid(),
        CityId = Guid.NewGuid(),
        PostalCode = "75500",
        Phones = ["+92 21 1234567"],
        Website = "https://karachi.example.com",
        SchoolNumber = "SN-1",
        ShortName = "KHI",
        PrincipalId = Guid.NewGuid(),
        Latitude = 24.8607m,
        Longitude = 67.0011m,
    };

    [Test]
    public void ShouldAcceptAFullyFilledCampus() =>
        _validator.Validate(Valid()).IsValid.ShouldBeTrue();

    [Test]
    public void ShouldAcceptACampusWithoutAWebsiteOrPrincipal() =>
        _validator.Validate(Valid() with { Website = null, PrincipalId = null }).IsValid.ShouldBeTrue();

    [TestCase(nameof(CampusDetailsInput.Address))]
    [TestCase(nameof(CampusDetailsInput.PostalCode))]
    [TestCase(nameof(CampusDetailsInput.SchoolNumber))]
    [TestCase(nameof(CampusDetailsInput.ShortName))]
    public void ShouldRefuseAMissingRequiredText(string property)
    {
        var input = property switch
        {
            nameof(CampusDetailsInput.Address) => Valid() with { Address = " " },
            nameof(CampusDetailsInput.PostalCode) => Valid() with { PostalCode = "" },
            nameof(CampusDetailsInput.SchoolNumber) => Valid() with { SchoolNumber = "" },
            _ => Valid() with { ShortName = "" },
        };

        _validator.Validate(input).Errors.ShouldContain(e => e.PropertyName == property);
    }

    [TestCase(nameof(CampusDetailsInput.StateId))]
    [TestCase(nameof(CampusDetailsInput.CityId))]
    public void ShouldRefuseAMissingStateOrCity(string property)
    {
        var input = property == nameof(CampusDetailsInput.StateId) ? Valid() with { StateId = null } : Valid() with { CityId = null };

        _validator.Validate(input).Errors.ShouldContain(e => e.PropertyName == property);
    }

    [Test]
    public void ShouldRefuseACampusWithoutAPhoneNumber()
    {
        _validator.Validate(Valid() with { Phones = [] }).Errors.ShouldContain(e => e.PropertyName == nameof(CampusDetailsInput.Phones));
        _validator.Validate(Valid() with { Phones = ["  "] }).IsValid.ShouldBeFalse();
    }

    [TestCase(5, true)]
    [TestCase(6, false)]
    public void ShouldAcceptAtMostFivePhoneNumbers(int count, bool valid)
    {
        var phones = Enumerable.Range(0, count).Select(i => $"+92 21 000000{i}").ToList();

        _validator.Validate(Valid() with { Phones = phones }).IsValid.ShouldBe(valid);
    }

    [TestCase("1234567", true)]
    [TestCase("123456", false)]
    [TestCase("+1234567890123456", false)]
    public void ShouldCheckEachPhoneNumber(string phone, bool valid) =>
        _validator.Validate(Valid() with { Phones = [phone] }).IsValid.ShouldBe(valid);

    [TestCase("https://school.example.com", true)]
    [TestCase("ftp://school.example.com", false)]
    public void ShouldCheckTheWebsite(string website, bool valid) =>
        _validator.Validate(Valid() with { Website = website }).IsValid.ShouldBe(valid);

    [TestCase(25, true)]
    [TestCase(26, false)]
    public void ShouldLimitTheShortName(int length, bool valid) =>
        _validator.Validate(Valid() with { ShortName = new string('s', length) }).IsValid.ShouldBe(valid);

    [TestCase(50, true)]
    [TestCase(51, false)]
    public void ShouldLimitTheSchoolNumber(int length, bool valid) =>
        _validator.Validate(Valid() with { SchoolNumber = new string('n', length) }).IsValid.ShouldBe(valid);

    [Test]
    public void ShouldRefuseACampusWithoutItsPlaceOnTheMap()
    {
        _validator.Validate(Valid() with { Latitude = null }).Errors.ShouldContain(e => e.PropertyName == nameof(CampusDetailsInput.Latitude));
        _validator.Validate(Valid() with { Longitude = null }).Errors.ShouldContain(e => e.PropertyName == nameof(CampusDetailsInput.Longitude));
    }

    [TestCase(90, true)]
    [TestCase(-90, true)]
    [TestCase(90.000001, false)]
    [TestCase(-91, false)]
    public void ShouldCheckTheLatitudeLimits(double latitude, bool valid) =>
        _validator.Validate(Valid() with { Latitude = (decimal)latitude }).IsValid.ShouldBe(valid);

    [TestCase(180, true)]
    [TestCase(-180, true)]
    [TestCase(180.000001, false)]
    [TestCase(-181, false)]
    public void ShouldCheckTheLongitudeLimits(double longitude, bool valid) =>
        _validator.Validate(Valid() with { Longitude = (decimal)longitude }).IsValid.ShouldBe(valid);
}
