using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// Answers whether one of a user's school roles gives a right in a module (SEC-AUTHZ-02). The user's roles are the
/// accounts' <c>AccountRoles</c> of the school. Fails closed: no role, or no role that gives the right, is a refusal.
/// </summary>
public interface IRoleRights
{
    Task<bool> HasRightAsync(string userId, int schoolId, ModuleCode moduleCode, AccessRight right, CancellationToken cancellationToken);
}
