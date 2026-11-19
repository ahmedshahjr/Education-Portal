using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Constants;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Academics;

/// <summary>
/// A teacher grades only the courses they teach (AGENTS.md section 2). The signed-in account must belong to the course's
/// teacher; administrators are not limited by it; an account with no person record is refused.
/// </summary>
public class CourseOwnershipTests
{
    private const string TeacherAccount = "teacher-account";
    private const int TeacherPersonId = 7;

    [Test]
    public async Task ShouldAllowTheTeacherOfTheCourse()
    {
        var identity = IdentityWithPerson(TeacherAccount, TeacherPersonId);

        await CourseOwnership.EnsureCanGradeAsync(identity, new FakeUser(TeacherAccount, []), TeacherPersonId, CancellationToken.None);
    }

    [Test]
    public async Task ShouldRefuseAnotherTeacher()
    {
        var identity = IdentityWithPerson("other-account", 99);

        await Should.ThrowAsync<ForbiddenAccessException>(() =>
            CourseOwnership.EnsureCanGradeAsync(identity, new FakeUser("other-account", []), TeacherPersonId, CancellationToken.None));
    }

    [Test]
    public async Task ShouldRefuseAnAccountWithNoPersonRecord()
    {
        var identity = new Mock<IIdentityService>();
        identity.Setup(i => i.FindPersonIdOfAccountAsync("lonely-account")).ReturnsAsync((int?)null);

        await Should.ThrowAsync<ForbiddenAccessException>(() =>
            CourseOwnership.EnsureCanGradeAsync(identity.Object, new FakeUser("lonely-account", []), TeacherPersonId, CancellationToken.None));
    }

    [Test]
    public async Task ShouldLetAnAdministratorGradeAnyCourse()
    {
        var identity = new Mock<IIdentityService>();

        await CourseOwnership.EnsureCanGradeAsync(identity.Object, new FakeUser("admin-account", [Roles.Administrator]), TeacherPersonId, CancellationToken.None);

        identity.Verify(i => i.FindPersonIdOfAccountAsync(It.IsAny<string>()), Times.Never);
    }

    private static IIdentityService IdentityWithPerson(string account, int personId)
    {
        var identity = new Mock<IIdentityService>();
        identity.Setup(i => i.FindPersonIdOfAccountAsync(account)).ReturnsAsync(personId);
        return identity.Object;
    }

    private sealed record FakeUser(string? Id, List<string>? Roles, bool MustChangePassword = false) : IUser;
}
