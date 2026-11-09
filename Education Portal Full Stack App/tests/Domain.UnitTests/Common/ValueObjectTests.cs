using EducationPortal.Domain.Common;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Common;

public class ValueObjectTests
{
    private sealed class Score(int points, int maxPoints) : ValueObject
    {
        public int Points { get; } = points;

        public int MaxPoints { get; } = maxPoints;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Points;
            yield return MaxPoints;
        }
    }

    [Test]
    public void ShouldBeEqualWhenAllComponentsMatch()
    {
        var left = new Score(78, 100);
        var right = new Score(78, 100);

        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    [Test]
    public void ShouldNotBeEqualWhenAnyComponentDiffers()
    {
        var left = new Score(78, 100);
        var right = new Score(78, 80);

        left.Equals(right).ShouldBeFalse();
        (left != right).ShouldBeTrue();
    }

    [Test]
    public void ShouldNotBeEqualToNull()
    {
        var score = new Score(78, 100);

        score.Equals(null).ShouldBeFalse();
    }
}
