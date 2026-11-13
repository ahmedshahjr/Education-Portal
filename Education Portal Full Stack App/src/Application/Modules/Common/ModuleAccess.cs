using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Modules.Common;

/// <inheritdoc cref="IModuleAccess"/>
public class ModuleAccess : IModuleAccess
{
    private readonly IApplicationDbContext _context;
    private readonly IModuleEntitlementStore _store;
    private readonly TimeProvider _timeProvider;

    public ModuleAccess(IApplicationDbContext context, IModuleEntitlementStore store, TimeProvider timeProvider)
    {
        _context = context;
        _store = store;
        _timeProvider = timeProvider;
    }

    public async Task<bool> HasModuleAsync(int schoolId, string moduleCode, int? branchId, CancellationToken cancellationToken)
    {
        var entitlements = await GetAsync(schoolId, cancellationToken);
        return entitlements.Has(moduleCode, branchId, _timeProvider.GetUtcNow());
    }

    public async Task<IReadOnlyList<string>> GetEnabledCodesAsync(int schoolId, CancellationToken cancellationToken)
    {
        var entitlements = await GetAsync(schoolId, cancellationToken);
        return entitlements.EnabledCodes(_timeProvider.GetUtcNow());
    }

    private Task<ModuleEntitlements> GetAsync(int schoolId, CancellationToken cancellationToken) =>
        _store.GetAsync(schoolId, ct => ModuleEntitlementLoader.LoadAsync(_context, schoolId, ct), cancellationToken);
}
