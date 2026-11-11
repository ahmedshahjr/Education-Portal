using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Modules.Common;

/// <inheritdoc cref="IModuleAccess"/>
public class ModuleAccess : IModuleAccess
{
    private readonly IApplicationDbContext _context;

    public ModuleAccess(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsEnabledAsync(int schoolId, string moduleKey, CancellationToken cancellationToken)
    {
        var enabled = await GetEnabledKeysAsync(schoolId, cancellationToken);
        return enabled.Contains(moduleKey, StringComparer.Ordinal);
    }

    public async Task<IReadOnlyList<string>> GetEnabledKeysAsync(int schoolId, CancellationToken cancellationToken)
    {
        // A handful of modules, so one small query per call (ENG-EF-02: no-tracking projection).
        var modules = await ModuleStates.ForSchool(_context, schoolId)
            .ToListAsync(cancellationToken);

        return modules
            .Where(m => PortalModule.IsEnabled(m.IsCore, m.IsDefault, m.SchoolChoice))
            .Select(m => m.Key)
            .ToList();
    }
}
