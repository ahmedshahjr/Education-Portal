using EducationPortal.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.ValueObjects;

public class HexColorTests
{
    [Test]
    public void ShouldStoreTheColourInLowerCase()
    {
        HexColor.From("#1F2A3D").Value.ShouldBe("#1f2a3d");
    }

    [TestCase("")]
    [TestCase("1f2a3d")]
    [TestCase("#1f2a3")]
    [TestCase("#1f2a3dff")]
    [TestCase("#gggggg")]
    [TestCase("#123456;color:red")]
    [TestCase("red")]
    public void ShouldRefuseAnythingButSixHexDigits(string value)
    {
        HexColor.IsValid(value).ShouldBeFalse();
        Should.Throw<ArgumentException>(() => HexColor.From(value));
    }

    [Test]
    public void ShouldCompareByValue()
    {
        HexColor.From("#ABCDEF").ShouldBe(HexColor.From("#abcdef"));
    }
}
