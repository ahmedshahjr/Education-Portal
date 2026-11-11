using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Common;

/// <summary>ENG-ID-01, ENG-ID-02: every entity gets its public id when it is created, before it is saved.</summary>
public class BaseEntityTests
{
    [Test]
    public void ShouldGiveANewEntityAVersion7PublicId()
    {
        var school = School.Create("School One", "school1.com");

        school.PublicId.ShouldNotBe(Guid.Empty);
        school.PublicId.Version.ShouldBe(7);
        school.Id.ShouldBe(0L);
    }

    [Test]
    public void ShouldGiveEveryEntityItsOwnPublicId()
    {
        var first = School.Create("School One", "school1.com");
        var second = School.Create("School Two", "school2.com");

        first.PublicId.ShouldNotBe(second.PublicId);
    }
}
