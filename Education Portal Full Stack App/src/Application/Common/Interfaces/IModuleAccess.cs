namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// Answers which modules a school may use (AGENTS.md section 2, SEC-AUTHZ-01). An inactive module is off for everyone;
/// otherwise the master tenant's choice for the school wins, and without one the module's type decides.
/// </summary>
public interface IModuleAccess
{
    /// <summary>True when the module is switched on for the school. An unknown key is never on (fail closed).</summary>
    Task<bool> IsEnabledAsync(int schoolId, string moduleKey, CancellationToken cancellationToken);

    /// <summary>The keys of the modules switched on for the school.</summary>
    Task<IReadOnlyList<string>> GetEnabledKeysAsync(int schoolId, CancellationToken cancellationToken);
}
