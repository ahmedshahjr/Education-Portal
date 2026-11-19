using EducationPortal.Domain.Academics;
using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Academics;

/// <summary>A grading scale keeps a trimmed name of at most the stored length, and its bands are validated before it is created.</summary>
public class GradingScaleTests
{
    private static readonly GradeBand[] ValidBands =
    [
        new(0m, 59.99m, "F", 0m),
        new(60m, 100m, "A", 4m),
    ];

    [Test]
    public void ShouldTrimTheNameAndKeepEveryBand()
    {
        var scale = GradingScale.Create(1, "  Primary  ", ValidBands);

        scale.Name.ShouldBe("Primary");
        scale.Bands.Count.ShouldBe(2);
    }

    [Test]
    public void ShouldAcceptANameExactlyAtTheLimit()
    {
        var scale = GradingScale.Create(1, new string('N', GradingScale.NameMaxLength), ValidBands);

        scale.Name.Length.ShouldBe(GradingScale.NameMaxLength);
    }

    [Test]
    public void ShouldRefuseANameOneCharacterOverTheLimit()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            GradingScale.Create(1, new string('N', GradingScale.NameMaxLength + 1), ValidBands));
    }

    [Test]
    public void ShouldRefuseBandsThatLeaveAGap()
    {
        Should.Throw<ArgumentException>(() =>
            GradingScale.Create(1, "Primary", [new GradeBand(0m, 50m, "F", 0m), new GradeBand(60m, 100m, "A", 4m)]));
    }
}
