using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>A parent is linked to a student of the same school, and a person is never their own parent.</summary>
public class ParentStudentTests
{
    [Test]
    public void ShouldLinkAParentToAStudent()
    {
        var link = ParentStudent.Create(schoolId: 1, parentUserDetailsId: 2, studentUserDetailsId: 3);

        link.SchoolId.ShouldBe(1);
        link.ParentUserDetailsId.ShouldBe(2);
        link.StudentUserDetailsId.ShouldBe(3);
    }

    [Test]
    public void ShouldRefuseAPersonBeingTheirOwnParent()
    {
        Should.Throw<ArgumentException>(() => ParentStudent.Create(1, 5, 5));
    }

    [TestCase(0, 2, 3)]
    [TestCase(1, 0, 3)]
    [TestCase(1, 2, 0)]
    [TestCase(-1, 2, 3)]
    public void ShouldRefuseAnIdThatIsNotASavedRow(int schoolId, int parentId, int studentId)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => ParentStudent.Create(schoolId, parentId, studentId));
    }
}
