using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Modules.Common;

/// <summary>
/// One grant of a module to a school, for a period, on one campus. A grant on the school's main campus
/// (<paramref name="IsSchoolWide"/>) covers the whole school. A grant comes from the school's subscription, or is the
/// default grant of a module that is on by default (given when the school registers).
/// </summary>
public sealed record ModuleGrant(string Code, int CampusId, bool IsSchoolWide, DateTimeOffset ValidFrom, DateTimeOffset? ValidTo);

/// <summary>
/// What one school may use: the grants of the active modules. Every module, core or paid, is on only through a grant that
/// is in its period. Time is checked when asked, so a cached copy never keeps a module on past its <c>ValidTo</c>.
/// </summary>
public sealed class ModuleEntitlements
{
    private readonly IReadOnlyList<ModuleGrant> _grants;
    private readonly IReadOnlyList<string> _order;

    /// <param name="order">Every active module's code in catalogue order.</param>
    /// <param name="grants">Grants of active modules whose tenant module is active.</param>
    public ModuleEntitlements(IReadOnlyList<string> order, IReadOnlyList<ModuleGrant> grants)
    {
        _order = order;
        _grants = grants;
    }

    /// <summary>No module at all: what an unknown school has.</summary>
    public static ModuleEntitlements None { get; } = new([], []);

    /// <summary>
    /// True when the module is on at <paramref name="now"/> for the school, or for the campus when one is given. Without a
    /// campus only a whole-school grant counts, so a module given to one campus only is not on (fail closed).
    /// </summary>
    public bool Has(string moduleCode, int? campusId, DateTimeOffset now) =>
        _grants.Any(g =>
            g.Code == moduleCode
            && IsInPeriod(g, now)
            && (g.IsSchoolWide || g.CampusId == campusId));

    /// <summary>The modules on at <paramref name="now"/> for the whole school, in catalogue order.</summary>
    public IReadOnlyList<string> EnabledCodes(DateTimeOffset now) =>
        _order.Where(code => _grants.Any(g => g.Code == code && g.IsSchoolWide && IsInPeriod(g, now)))
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
        var order = await context.Modules
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Code)
            .Select(m => m.Code)
            .ToListAsync(cancellationToken);

        var grants = await (
                from t in context.TenantModules.AsNoTracking()
                join m in context.Modules on t.ModuleId equals m.Id
                where t.SchoolId == schoolId && t.Status == TenantModuleStatus.Active && m.IsActive
                select new ModuleGrant(m.Code, t.CampusId, t.IsSchoolWide, t.ValidFrom, t.ValidTo))
            .ToListAsync(cancellationToken);

        return new ModuleEntitlements(order, grants);
    }
}
