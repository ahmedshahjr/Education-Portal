using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Users.Dtos;

namespace EducationPortal.Application.Parents.Common;

/// <summary>The students linked to a parent, by name. Both sides are filtered by the school (SEC-AUTHZ-03).</summary>
internal static class ParentChildren
{
    public static Task<List<UserDetailsDto>> ListAsync(IApplicationDbContext context, int parentId, CancellationToken cancellationToken) =>
        context.ParentStudents
            .AsNoTracking()
            .Where(l => l.ParentUserDetailsId == parentId)
            .Join(context.UserDetails.AsNoTracking(), l => l.StudentUserDetailsId, u => u.Id, (l, u) => u)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new UserDetailsDto(u.PublicId, u.FirstName, u.MiddleName, u.LastName, u.UserType, u.Phone))
            .ToListAsync(cancellationToken);
}
