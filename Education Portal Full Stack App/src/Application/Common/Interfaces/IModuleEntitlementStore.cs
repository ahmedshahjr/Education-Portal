using EducationPortal.Application.Modules.Common;

namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// Keeps each school's module entitlements for a short time, so the module check on every request does not query the
/// database each time. Whatever changes a school's modules calls <see cref="Invalidate"/>; whatever changes the
/// catalogue calls <see cref="InvalidateAll"/>. Entries also expire on their own, which bounds how stale another server
/// instance's copy can be.
/// </summary>
public interface IModuleEntitlementStore
{
    /// <summary>The school's entitlements: the cached copy, or <paramref name="load"/> when there is none.</summary>
    Task<ModuleEntitlements> GetAsync(int schoolId, Func<CancellationToken, Task<ModuleEntitlements>> load, CancellationToken cancellationToken);

    /// <summary>Forgets one school's entitlements; the next check reads them again.</summary>
    void Invalidate(int schoolId);

    /// <summary>Forgets every school's entitlements, after a change to the module catalogue.</summary>
    void InvalidateAll();
}
