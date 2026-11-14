using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Common;

public class EnumNamesTests
{
    [TestCase("Logo", BrandingImageKind.Logo)]
    [TestCase("favicon", BrandingImageKind.Favicon)]
    [TestCase("  Logo ", BrandingImageKind.Logo)]
    public void ShouldParseAMemberName(string value, BrandingImageKind expected)
    {
        EnumNames.TryParse<BrandingImageKind>(value, out var kind).ShouldBeTrue();
        kind.ShouldBe(expected);
    }

    // ENG-ENUM-01: numbers, undefined names, combinations and blanks are refused, unlike Enum.TryParse.
    [TestCase("1")]
    [TestCase("99")]
    [TestCase("-1")]
    [TestCase("+2")]
    [TestCase("Logo,Favicon")]
    [TestCase("Banner")]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void ShouldRefuseAnythingButAMemberName(string? value)
    {
        EnumNames.TryParse<BrandingImageKind>(value, out var kind).ShouldBeFalse();
        kind.ShouldBe(default);
    }
}
