using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// Which campuses of the current school the signed-in user may act on (AGENTS.md section 2, "Campuses per user"). The
/// school's administrators and the master tenant may act on every campus; everyone else only on the campuses their person
/// is assigned to (<c>UserCampuses</c>). Fails closed: an account that is not a person's, or a person with no assignment,
/// may act on no campus. Rights per module still come from the user's roles (<see cref="IRoleRights"/>); this only limits
/// where they apply.
/// </summary>
public interface ICampusAccess
{
    /// <summary>
    /// The internal ids of the campuses the user may act on, or <c>null</c> when the user may act on every campus of the
    /// school.
    /// </summary>
    Task<IReadOnlyCollection<int>?> GetAllowedCampusIdsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Throws <c>ForbiddenAccessException</c> unless the user may act on the campus. The campus is required: a row of the
    /// whole school (no campus) is decided by the role rights alone and is not passed here.
    /// </summary>
    Task EnsureCampusAsync(int campusId, CancellationToken cancellationToken);

    /// <summary>
    /// The campuses where the user may use the right in the module: their assigned campuses intersected with the campuses
    /// their roles give the right on (ADR-022). <c>null</c> means every campus.
    /// </summary>
    Task<IReadOnlyCollection<int>?> GetAllowedCampusIdsAsync(ModuleCode module, AccessRight right, CancellationToken cancellationToken);

    /// <summary>
    /// Throws <c>ForbiddenAccessException</c> unless the user is assigned to the campus and a role gives the right there.
    /// The campus is required; a whole-school row is not passed here.
    /// </summary>
    Task EnsureCampusAsync(int campusId, ModuleCode module, AccessRight right, CancellationToken cancellationToken);
}
