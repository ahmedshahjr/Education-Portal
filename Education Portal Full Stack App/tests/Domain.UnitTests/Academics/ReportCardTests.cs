using EducationPortal.Domain.Academics;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Academics;

/// <summary>
/// A report card: a line per course with its percent, letter and points, and the GPA over the graded courses only. A course
/// with no recorded score is not graded yet and does not count in the GPA.
/// </summary>
public class ReportCardTests
{
    private static readonly GradeBand[] Bands =
    [
        new(0m, 59.99m, "F", 0m),
        new(60m, 79.99m, "B", 3m),
        new(80m, 100m, "A", 4m),
    ];

    [Test]
    public void ShouldGradeEachCourseAndAverageTheGradedOnesOnly()
    {
        var card = ReportCard.Build(
        [
            new CourseResult("Maths", [new AssessmentScore(90, 100, 100)]),
            new CourseResult("Art", [new AssessmentScore(65, 100, 100)]),
            new CourseResult("Music", []),
        ],
        Bands);

        card.Lines.Count.ShouldBe(3);
        card.Lines[0].ShouldBe(new ReportCardLine("Maths", 90m, "A", 4m));
        card.Lines[1].ShouldBe(new ReportCardLine("Art", 65m, "B", 3m));
        card.Lines[2].ShouldBe(new ReportCardLine("Music", null, null, null));
        card.Gpa.ShouldBe(3.5m);
    }

    [Test]
    public void ShouldLeaveTheGpaEmptyWhenNoCourseIsGraded()
    {
        var card = ReportCard.Build([new CourseResult("Maths", [])], Bands);

        card.Gpa.ShouldBeNull();
        card.Lines[0].Percent.ShouldBeNull();
    }

    [Test]
    public void ShouldRoundTheGpaHalfUpToTwoDecimals()
    {
        // (4 + 3 + 3) / 3 = 3.333... -> 3.33
        var card = ReportCard.Build(
        [
            new CourseResult("A", [new AssessmentScore(90, 100, 1)]),
            new CourseResult("B", [new AssessmentScore(65, 100, 1)]),
            new CourseResult("C", [new AssessmentScore(70, 100, 1)]),
        ],
        Bands);

        card.Gpa.ShouldBe(3.33m);
    }

    [Test]
    public void ShouldPlaceAPercentOnTheBandBoundary()
    {
        // 80 is the first percent of band A, 79.99 the last of band B.
        var atBoundary = ReportCard.Build([new CourseResult("Maths", [new AssessmentScore(80, 100, 1)])], Bands);
        var justBelow = ReportCard.Build([new CourseResult("Maths", [new AssessmentScore(79.99m, 100, 1)])], Bands);

        atBoundary.Lines[0].Letter.ShouldBe("A");
        justBelow.Lines[0].Letter.ShouldBe("B");
    }
}
