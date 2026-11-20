using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Enums;

public class DataRegionForCountryTests
{
    [TestCase("PK", DataRegion.SouthAsia)]
    [TestCase("SA", DataRegion.MiddleEast)]
    [TestCase("AE", DataRegion.MiddleEast)]
    [TestCase("US", DataRegion.NorthAmerica)]
    public void ShouldGiveTheRegionOfAMappedCountry(string code, DataRegion expected) =>
        DataRegionForCountry.For(code).ShouldBe(expected);

    [TestCase("pk", DataRegion.SouthAsia)]
    [TestCase("us", DataRegion.NorthAmerica)]
    public void ShouldIgnoreTheCaseOfTheCode(string code, DataRegion expected) =>
        DataRegionForCountry.For(code).ShouldBe(expected);

    [TestCase("JP")]
    [TestCase("ZZ")]
    [TestCase("")]
    [TestCase(null)]
    public void ShouldLeaveTheRegionUndecidedForAnUnmappedCountry(string? code) =>
        DataRegionForCountry.For(code).ShouldBeNull();
}
