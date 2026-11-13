using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Modules.Common;

/// <summary>One module granted to a school by its subscription, for a period and, optionally, one branch.</summary>
public sealed record ModuleGrant(string Code, int? BranchId, DateTimeOffset ValidFrom, DateTimeOffset? ValidTo);

/// <summary>
/// What one school may use: the active core modules and the grants of active paid modules. Time is checked when asked,
/// so a cached copy never keeps a module on past its <c>ValidTo</c>.
/// </summary>
public sealed class ModuleEntitlements
{
    private readonly IReadOnlyList<string> _coreCodes;
    private readonly IReadOnlyList<ModuleGrant> _grants;
    private readonly IReadOnlyList<string> _order;

    /// <param name="order">Every active module's code in catalogue order.</param>
    /// <param name="coreCodes">The active core modules.</param>
    /// <param name="grants">Grants of active paid modules whose tenant module is active.</param>
    public ModuleEntitlements(IReadOnlyList<string> order, IReadOnlyList<string> coreCodes, IReadOnlyList<ModuleGrant> grants)
    {
        _order = order;
        _coreCodes = coreCodes;
        _grants = grants;
    }

    /// <summary>No module at all: what an unknown school has.</summary>
    public static ModuleEntitlements None { get; } = new([], [], []);

    /// <summary>True when the module is on at <paramref name="now"/> for the school, or for the branch when one is given.</summary>
    public bool Has(string moduleCode, int? branchId, DateTimeOffset now)
    {
        if (_coreCodes.Contains(moduleCode, StringComparer.Ordinal))
        {
            return true;
        }

        return _grants.Any(g =>
            g.Code == moduleCode
            && IsInPeriod(g, now)
            && (g.BranchId is null || g.BranchId == branchId));
    }

    /// <summary>The modules on at <paramref name="now"/> for the whole school, in catalogue order.</summary>
    public IReadOnlyList<string> EnabledCodes(DateTimeOffset now) =>
        _order.Where(code => _coreCodes.Contains(code, StringComparer.Ordinal)
                             || _grants.Any(g => g.Code == code && g.BranchId is null && IsInPeriod(g, now)))
            .ToList();

    private static bool IsInPeriod(ModuleGrant grant, DateTimeOffset now) =>
        grant.ValidFrom <= now && (grant.ValidTo is null || grant.ValidTo > now);
}

internal static class ModuleEntitlementLoader
{
    /// <summary>
    /// Reads a school's entitlements. The school is named in the query, so the result is the same whichever tenant asks
    /// (the master tenant reads other schools' rows through the query filter's master bypass).
    /// </summary>
    public static async Task<ModuleEntitlements> LoadAsync(IApplicationDbContext context, int schoolId, CancellationToken cancellationToken)
    {
        var modules = await context.Modules
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Code)
            .Select(m => new { m.Id, m.Code, m.IsCore })
            .ToListAsync(cancellationToken);

        var grants = await (
                from t in context.TenantModules.AsNoTracking()
                join m in context.Modules on t.ModuleId equals m.Id
                where t.SchoolId == schoolId && t.Status == TenantModuleStatus.Active && m.IsActive && !m.IsCore
                select new ModuleGrant(m.Code, t.BranchId, t.ValidFrom, t.ValidTo))
            .ToListAsync(cancellationToken);

        return new ModuleEntitlements(
            modules.Select(m => m.Code).ToList(),
            modules.Where(m => m.IsCore).Select(m => m.Code).ToList(),
            grants);
    }
}
