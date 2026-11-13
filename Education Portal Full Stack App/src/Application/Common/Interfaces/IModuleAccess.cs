namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// Answers which modules a school may use (AGENTS.md section 2, SEC-AUTHZ-01). A module is on for a school when it is
/// active in the catalogue and either core (core modules are in every subscription) or granted by an active, in-period
/// <c>TenantModule</c> of the school's subscription. A grant limited to one branch counts only for that branch. The
/// master tenant is never limited by modules; callers handle it before asking.
/// </summary>
public interface IModuleAccess
{
    /// <summary>
    /// True when the module is on for the school (and for the branch, when one is given). Without a branch, a module
    /// granted only to a branch is not on (fail closed). An unknown code is never on.
    /// </summary>
    Task<bool> HasModuleAsync(int schoolId, string moduleCode, int? branchId, CancellationToken cancellationToken);

    /// <summary>The codes of the modules on for the whole school, in catalogue order.</summary>
    Task<IReadOnlyList<string>> GetEnabledCodesAsync(int schoolId, CancellationToken cancellationToken);
}
