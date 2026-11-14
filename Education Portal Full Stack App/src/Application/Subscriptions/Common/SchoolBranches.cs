using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Subscriptions.Common;

/// <summary>Finds the branches a subscription grants modules on.</summary>
public static class SchoolBranches
{
    /// <summary>
    /// The school's main branch, its registration branch. Every school has exactly one (the database enforces it), so
    /// this always finds it. The school is named in the query, so it is right whichever tenant asks.
    /// </summary>
    public static Task<Branch> MainAsync(IApplicationDbContext context, int schoolId, CancellationToken cancellationToken) =>
        context.Branches.SingleAsync(b => b.SchoolId == schoolId && b.Kind == BranchKind.Main, cancellationToken);
}
