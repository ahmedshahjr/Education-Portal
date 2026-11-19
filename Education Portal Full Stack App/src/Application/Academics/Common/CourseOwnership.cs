using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Academics.Common;

/// <summary>
/// The rule that a teacher grades only the courses they teach (AGENTS.md section 2). Administrators (the school's owners) are
/// not limited by it. The signed-in person is the one the account belongs to.
/// </summary>
public static class CourseOwnership
{
    public static bool IsAdministrator(IUser user) => user.Roles?.Contains(Roles.Administrator) ?? false;

    public static async Task EnsureCanGradeAsync(IIdentityService identityService, IUser user, int teacherUserDetailsId, CancellationToken cancellationToken)
    {
        if (IsAdministrator(user))
        {
            return;
        }

        var personId = await identityService.FindPersonIdOfAccountAsync(user.Id ?? string.Empty);
        if (personId != teacherUserDetailsId)
        {
            throw new ForbiddenAccessException();
        }
    }
}
