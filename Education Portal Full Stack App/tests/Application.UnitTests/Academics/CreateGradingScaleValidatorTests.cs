using EducationPortal.Application.Academics.Commands.CreateGradingScale;
using EducationPortal.Application.Academics.Common;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Academics;

/// <summary>A grading scale's bands must cover 0 to 100 once: no gap, no overlap (the boundaries are tested).</summary>
public class CreateGradingScaleValidatorTests
{
    private static CreateGradingScaleCommand Scale(params GradeBandDto[] bands) => new() { Name = "Primary", Bands = bands };

    [Test]
    public void ShouldAcceptBandsThatCoverZeroToOneHundredOnce()
    {
        var result = new CreateGradingScaleCommandValidator().Validate(Scale(
            new GradeBandDto(0m, 59.99m, "F", 0m),
            new GradeBandDto(60m, 100m, "A", 4m)));

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseAGapOfOneHundredthBetweenBands()
    {
        var result = new CreateGradingScaleCommandValidator().Validate(Scale(
            new GradeBandDto(0m, 59.99m, "F", 0m),
            new GradeBandDto(60.01m, 100m, "A", 4m)));

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseBandsThatOverlap()
    {
        var result = new CreateGradingScaleCommandValidator().Validate(Scale(
            new GradeBandDto(0m, 60m, "F", 0m),
            new GradeBandDto(60m, 100m, "A", 4m)));

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseAScaleWithNoBands()
    {
        var result = new CreateGradingScaleCommandValidator().Validate(Scale());

        result.IsValid.ShouldBeFalse();
    }
}
