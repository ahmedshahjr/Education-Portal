using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Communication;

/// <summary>A notification copies its event's text, belongs to one recipient, and keeps the first time it was read.</summary>
public class NotificationTests
{
    private static readonly DateTimeOffset Start = new(2026, 12, 1, 9, 0, 0, TimeSpan.Zero);

    private static SchoolEvent Event() => SchoolEvent.Create(1, "Sports day", "Bring water", Start, Start.AddHours(2), EventAudience.All);

    [Test]
    public void ShouldCopyTheEventTextAndBelongToTheSchoolOfTheEvent()
    {
        var notification = Notification.For(Event(), "account-1");

        notification.Title.ShouldBe("Sports day");
        notification.Body.ShouldBe("Bring water");
        notification.SchoolId.ShouldBe(1);
        notification.RecipientAccountId.ShouldBe("account-1");
        notification.ReadAt.ShouldBeNull();
    }

    [Test]
    public void ShouldKeepTheFirstTimeItWasRead()
    {
        var notification = Notification.For(Event(), "account-1");
        var first = new DateTimeOffset(2026, 12, 2, 8, 0, 0, TimeSpan.Zero);

        notification.MarkRead(first);
        notification.MarkRead(first.AddDays(1));

        notification.ReadAt.ShouldBe(first);
    }

    [Test]
    public void ShouldRefuseAnEmptyRecipient()
    {
        Should.Throw<ArgumentException>(() => Notification.For(Event(), " "));
    }

    [Test]
    public void ShouldRefuseARecipientLongerThanAnIdentityId()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Notification.For(Event(), new string('a', Notification.RecipientMaxLength + 1)));
    }
}
