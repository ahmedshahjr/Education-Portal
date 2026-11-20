using EducationPortal.Domain.Academics;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Academics;

/// <summary>The grading rules: a scale covers 0 to 100 once, a course's percent is its weighted average, and GPA is a mean.</summary>
public class GradeScaleTests
{
    private static readonly GradeBand[] Standard =
    [
        new(0m, 59.99m, "F", 0m),
        new(60m, 79.99m, "B", 3m),
        new(80m, 100m, "A", 4m),
    ];

    [Test]
    public void ShouldAcceptAScaleThatCoversZeroToHundredOnce()
    {
        Should.NotThrow(() => GradeScale.Validate(Standard));
    }

    [Test]
    public void ShouldRefuseAnEmptyScale()
    {
        Should.Throw<ArgumentException>(() => GradeScale.Validate([]));
    }

    [Test]
    public void ShouldRefuseAScaleThatDoesNotStartAtZero()
    {
        var bands = new[] { new GradeBand(1m, 100m, "A", 4m) };

        Should.Throw<ArgumentException>(() => GradeScale.Validate(bands));
    }

    [Test]
    public void ShouldRefuseAScaleThatDoesNotEndAtHundred()
    {
        var bands = new[] { new GradeBand(0m, 99.99m, "A", 4m) };

        Should.Throw<ArgumentException>(() => GradeScale.Validate(bands));
    }

    [Test]
    public void ShouldRefuseAGapBetweenBands()
    {
        var bands = new[] { new GradeBand(0m, 59m, "F", 0m), new GradeBand(60m, 100m, "A", 4m) };

        Should.Throw<ArgumentException>(() => GradeScale.Validate(bands));
    }

    [Test]
    public void ShouldRefuseOverlappingBands()
    {
        var bands = new[] { new GradeBand(0m, 60m, "F", 0m), new GradeBand(60m, 100m, "A", 4m) };

        Should.Throw<ArgumentException>(() => GradeScale.Validate(bands));
    }

    [Test]
    public void ShouldRefuseABandWhoseMinimumIsAboveItsMaximum()
    {
        var bands = new[] { new GradeBand(0m, 100m, "A", 4m), new GradeBand(100m, 50m, "X", 0m) };

        Should.Throw<ArgumentException>(() => GradeScale.Validate(bands));
    }

    [TestCase("")]
    [TestCase("ABCDEF")]
    public void ShouldRefuseALetterThatIsBlankOrTooLong(string letter)
    {
        var bands = new[] { new GradeBand(0m, 100m, letter, 4m) };

        Should.Throw<ArgumentException>(() => GradeScale.Validate(bands));
    }

    [Test]
    public void ShouldRefuseNegativeGradePoints()
    {
        var bands = new[] { new GradeBand(0m, 100m, "A", -1m) };

        Should.Throw<ArgumentOutOfRangeException>(() => GradeScale.Validate(bands));
    }

    [TestCase(0, "F")]
    [TestCase(59.99, "F")]
    [TestCase(60, "B")]
    [TestCase(79.99, "B")]
    [TestCase(80, "A")]
    [TestCase(100, "A")]
    public void ShouldPutAPercentInItsBand(decimal percent, string letter)
    {
        GradeScale.BandFor(Standard, percent).Letter.ShouldBe(letter);
    }

    [Test]
    public void ShouldRoundAPercentBeforeLookingForItsBand()
    {
        // 59.995 rounds up to 60.00, so it is in the B band, not in the gap below it.
        GradeScale.BandFor(Standard, 59.995m).Letter.ShouldBe("B");
    }

    [TestCase(-0.01)]
    [TestCase(100.01)]
    public void ShouldRefuseAPercentOutsideZeroToHundred(decimal percent)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => GradeScale.BandFor(Standard, percent));
    }

    [Test]
    public void ShouldWeightTheScoresOfACourse()
    {
        // 18/20 at weight 30 and 40/50 at weight 70: (0.9*30 + 0.8*70) / 100 = 83.00
        var percent = GradeScale.CoursePercent(
        [
            new AssessmentScore(18m, 20m, 30m),
            new AssessmentScore(40m, 50m, 70m),
        ]);

        percent.ShouldBe(83.00m);
    }

    [Test]
    public void ShouldHaveNoPercentWithoutAssessments()
    {
        GradeScale.CoursePercent([]).ShouldBeNull();
    }

    [Test]
    public void ShouldHaveNoPercentWhenTheWeightsAddUpToZero()
    {
        GradeScale.CoursePercent([new AssessmentScore(10m, 10m, 0m)]).ShouldBeNull();
    }

    [Test]
    public void ShouldRoundACoursePercentHalfUp()
    {
        // 1/3 of 100 is 33.333..., rounded to 33.33; 2/3 is 66.666..., rounded to 66.67.
        GradeScale.CoursePercent([new AssessmentScore(1m, 3m, 1m)]).ShouldBe(33.33m);
        GradeScale.CoursePercent([new AssessmentScore(2m, 3m, 1m)]).ShouldBe(66.67m);
    }

    [TestCase(11, 10)]
    [TestCase(-1, 10)]
    public void ShouldRefuseAScoreOutsideItsAssessment(decimal value, decimal maximum)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => GradeScale.CoursePercent([new AssessmentScore(value, maximum, 100m)]));
    }

    [Test]
    public void ShouldRefuseAMaximumScoreOfZero()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => GradeScale.CoursePercent([new AssessmentScore(0m, 0m, 100m)]));
    }

    [Test]
    public void ShouldAverageTheGradePointsForTheGpa()
    {
        GradeScale.Gpa([4m, 3m, 3m]).ShouldBe(3.33m);
    }

    [Test]
    public void ShouldHaveNoGpaWithoutGradedCourses()
    {
        GradeScale.Gpa([]).ShouldBeNull();
    }

    [Test]
    public void ShouldRoundTheGpaHalfUp()
    {
        // (3 + 3 + 3 + 3 + 3 + 3 + 3 + 4) / 8 = 3.125, which rounds half up to 3.13.
        GradeScale.Gpa([3m, 3m, 3m, 3m, 3m, 3m, 3m, 4m]).ShouldBe(3.13m);
    }
}
