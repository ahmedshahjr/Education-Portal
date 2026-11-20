using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Academics.Common;

/// <summary>
/// Who may read a report card (AGENTS.md section 2). An administrator and the student, or a parent linked to the student,
/// see every course. A teacher sees only the lines of the courses they teach in the class, never another teacher's course.
/// Anyone else is refused.
/// </summary>
internal sealed record ReportCardView(bool SeesAllCourses, int? TeacherPersonId);

internal static class ReportCardAccess
{
    public static async Task<ReportCardView> ResolveAsync(
        IApplicationDbContext context,
        IIdentityService identityService,
        IUser user,
        int studentPersonId,
        int classOfferingId,
        CancellationToken cancellationToken)
    {
        if (user.Roles?.Contains(Roles.Administrator) ?? false)
        {
            return new ReportCardView(SeesAllCourses: true, TeacherPersonId: null);
        }

        var personId = await identityService.FindPersonIdOfAccountAsync(user.Id ?? string.Empty);
        if (personId is not { } person)
        {
            throw new ForbiddenAccessException();
        }

        if (person == studentPersonId)
        {
            return new ReportCardView(SeesAllCourses: true, TeacherPersonId: null);
        }

        var isLinkedParent = await context.ParentStudents.AnyAsync(
            link => link.ParentUserDetailsId == person && link.StudentUserDetailsId == studentPersonId, cancellationToken);
        if (isLinkedParent)
        {
            return new ReportCardView(SeesAllCourses: true, TeacherPersonId: null);
        }

        var teachesInClass = await context.Courses.AnyAsync(
            c => c.ClassOfferingId == classOfferingId && c.TeacherUserDetailsId == person, cancellationToken);
        if (teachesInClass)
        {
            return new ReportCardView(SeesAllCourses: false, TeacherPersonId: person);
        }

        throw new ForbiddenAccessException();
    }
}
