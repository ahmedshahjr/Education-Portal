using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.SchoolPublicPage.Common;

/// <summary>
/// Maps the campuses of one school from their internal id to their public id, so DTOs carry only public ids (ENG-ID-02).
/// The campuses are filtered on the school explicitly, so the master tenant sees only the chosen school's campuses.
/// </summary>
public static class CampusPublicIds
{
    public static async Task<Dictionary<int, Guid>> ForSchoolAsync(IApplicationDbContext context, int schoolId, CancellationToken cancellationToken)
    {
        return await context.Campuses
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId)
            .ToDictionaryAsync(c => c.Id, c => c.PublicId, cancellationToken);
    }
}
