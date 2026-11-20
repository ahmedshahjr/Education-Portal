using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Communication;

/// <summary>An event keeps UTC times, a trimmed title and description within their stored lengths, and ends after it starts.</summary>
public class SchoolEventTests
{
    private static readonly DateTimeOffset Start = new(2026, 12, 1, 9, 0, 0, TimeSpan.FromHours(3));

    [Test]
    public void ShouldStoreTheTimesInUtcAndTrimTheText()
    {
        var schoolEvent = SchoolEvent.Create(1, "  Sports day ", "  Bring water  ", Start, Start.AddHours(2), EventAudience.Students);

        schoolEvent.Title.ShouldBe("Sports day");
        schoolEvent.Description.ShouldBe("Bring water");
        schoolEvent.StartsAt.Offset.ShouldBe(TimeSpan.Zero);
        schoolEvent.StartsAt.ShouldBe(Start.ToUniversalTime());
    }

    [Test]
    public void ShouldAcceptATitleAtTheLimitAndRefuseOneOverIt()
    {
        SchoolEvent.Create(1, new string('T', SchoolEvent.TitleMaxLength), null, Start, Start.AddHours(1), EventAudience.All)
            .Title.Length.ShouldBe(SchoolEvent.TitleMaxLength);

        Should.Throw<ArgumentOutOfRangeException>(() =>
            SchoolEvent.Create(1, new string('T', SchoolEvent.TitleMaxLength + 1), null, Start, Start.AddHours(1), EventAudience.All));
    }

    [Test]
    public void ShouldRefuseADescriptionOneCharacterOverTheLimit()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            SchoolEvent.Create(1, "Sports day", new string('D', SchoolEvent.DescriptionMaxLength + 1), Start, Start.AddHours(1), EventAudience.All));
    }

    [Test]
    public void ShouldRefuseAnEventThatEndsWhenItStarts()
    {
        Should.Throw<ArgumentException>(() => SchoolEvent.Create(1, "Sports day", null, Start, Start, EventAudience.All));
    }

    [Test]
    public void ShouldRefuseAnAudienceThatIsNotDefined()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            SchoolEvent.Create(1, "Sports day", null, Start, Start.AddHours(1), (EventAudience)99));
    }
}
