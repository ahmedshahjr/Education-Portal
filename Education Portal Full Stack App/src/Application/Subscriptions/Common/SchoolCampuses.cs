using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Subscriptions.Common;

/// <summary>Finds the campuses a subscription grants modules on.</summary>
public static class SchoolCampuses
{
    /// <summary>
    /// The school's main campus, its registration campus. Every school has exactly one (the database enforces it), so
    /// this always finds it. The school is named in the query, so it is right whichever tenant asks.
    /// </summary>
    public static Task<Campus> MainAsync(IApplicationDbContext context, int schoolId, CancellationToken cancellationToken) =>
        context.Campuses.SingleAsync(b => b.SchoolId == schoolId && b.Kind == CampusKind.Main, cancellationToken);
}
