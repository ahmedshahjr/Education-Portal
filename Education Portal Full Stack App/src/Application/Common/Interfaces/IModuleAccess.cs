namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// Answers which modules a school may use (AGENTS.md section 2, SEC-AUTHZ-01). An inactive module is off for everyone;
/// a core module is on for every school; a paid module is on only when the master switched it on for the school.
/// </summary>
public interface IModuleAccess
{
    /// <summary>True when the module is on for the school. An unknown code is never on (fail closed).</summary>
    Task<bool> IsEnabledAsync(int schoolId, string moduleCode, CancellationToken cancellationToken);

    /// <summary>The codes of the modules on for the school, in catalogue order.</summary>
    Task<IReadOnlyList<string>> GetEnabledCodesAsync(int schoolId, CancellationToken cancellationToken);
}
