using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// Answers whether one of a user's school roles gives a right in a module (SEC-AUTHZ-02). The user's roles are the
/// accounts' <c>AccountRoles</c> of the school. Fails closed: no role, or no role that gives the right, is a refusal.
/// Rights are per campus in ADR-022 (docs/decisions/ADR-022-Role-Rights-Per-Campus.md); how to change them is in
/// docs/sop/rights-tenants-history.md.
/// </summary>
public interface IRoleRights
{
    /// <summary>True when a role of the user gives the right on any campus. The request pipeline asks this; the handler narrows to the campus.</summary>
    Task<bool> HasRightAsync(string userId, int schoolId, ModuleCode moduleCode, AccessRight right, CancellationToken cancellationToken);

    /// <summary>
    /// The internal ids of the campuses where the user's roles give the right, or <c>null</c> when a role gives it on every
    /// campus of the school. Roles combine: the result is the union over the user's roles. Empty means nowhere.
    /// </summary>
    Task<IReadOnlyCollection<int>?> GetRightCampusIdsAsync(string userId, int schoolId, ModuleCode moduleCode, AccessRight right, CancellationToken cancellationToken);
}
