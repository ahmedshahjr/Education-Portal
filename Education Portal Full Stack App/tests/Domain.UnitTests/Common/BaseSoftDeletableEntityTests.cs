using EducationPortal.Domain.Common;
using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Common;

public class BaseSoftDeletableEntityTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 10, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public void ShouldStartActiveAndNotDeleted()
    {
        var school = School.Create("Greenfield", "greenfield.example");

        school.IsActive.ShouldBeTrue();
        school.IsDeleted.ShouldBeFalse();
        school.DeletedAt.ShouldBeNull();
    }

    [Test]
    public void ShouldDeactivateAndReactivate()
    {
        var school = School.Create("Greenfield", "greenfield.example");

        school.Deactivate();
        school.IsActive.ShouldBeFalse();

        school.Activate();
        school.IsActive.ShouldBeTrue();
    }

    [Test]
    public void ShouldSoftDeleteAndRecordWhoAndWhen()
    {
        var school = School.Create("Greenfield", "greenfield.example");

        school.Delete(Now, "admin@localhost");

        school.IsDeleted.ShouldBeTrue();
        school.IsActive.ShouldBeFalse();
        school.DeletedAt.ShouldBe(Now);
        school.DeletedBy.ShouldBe("admin@localhost");
    }

    [Test]
    public void ShouldKeepTheFirstDeletionWhenDeletedTwice()
    {
        var school = School.Create("Greenfield", "greenfield.example");
        school.Delete(Now, "first");

        school.Delete(Now.AddDays(1), "second");

        school.DeletedBy.ShouldBe("first");
        school.DeletedAt.ShouldBe(Now);
    }

    [Test]
    public void ShouldRefuseChangesToADeletedEntity()
    {
        var school = School.Create("Greenfield", "greenfield.example");
        school.Delete(Now, "admin");

        Should.Throw<InvalidOperationException>(() => school.Activate());
        Should.Throw<InvalidOperationException>(() => school.Deactivate());
    }

    [Test]
    public void ShouldNeverDeactivateOrDeleteTheMasterTenant()
    {
        var master = School.CreateMaster("Education Portal", "localhost");

        Should.Throw<InvalidOperationException>(() => master.Deactivate());
        Should.Throw<InvalidOperationException>(() => master.Delete(Now, "admin"));
        master.IsActive.ShouldBeTrue();
        master.IsDeleted.ShouldBeFalse();
    }
}
