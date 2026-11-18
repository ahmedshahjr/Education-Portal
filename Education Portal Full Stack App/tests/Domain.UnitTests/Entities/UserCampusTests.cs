using EducationPortal.Domain.Entities;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class UserCampusTests
{
    [Test]
    public void ShouldLinkAPersonToACampusOfTheSchool()
    {
        var link = UserCampus.Create(schoolId: 1, userDetailsId: 2, campusId: 3);

        link.SchoolId.ShouldBe(1);
        link.UserDetailsId.ShouldBe(2);
        link.CampusId.ShouldBe(3);
    }

    [TestCase(0, 1, 1)]
    [TestCase(1, 0, 1)]
    [TestCase(1, 1, 0)]
    [TestCase(-1, 1, 1)]
    public void ShouldRefuseAnIdThatIsNotASavedRow(int schoolId, int userDetailsId, int campusId)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => UserCampus.Create(schoolId, userDetailsId, campusId));
    }
}
