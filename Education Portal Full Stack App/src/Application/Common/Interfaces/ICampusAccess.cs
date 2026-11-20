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
    /// Throws <c>ForbiddenAccessException</c> unless the user may act on the campus. <c>null</c> stands for a row of the
    /// whole school (no campus), which the user's role rights alone decide, so it always passes here.
    /// </summary>
    Task EnsureCampusAsync(int? campusId, CancellationToken cancellationToken);
}
