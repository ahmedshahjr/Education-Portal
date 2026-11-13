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

    public async Task<bool> IsEnabledAsync(int schoolId, string moduleCode, CancellationToken cancellationToken)
    {
        var enabled = await GetEnabledCodesAsync(schoolId, cancellationToken);
        return enabled.Contains(moduleCode, StringComparer.Ordinal);
    }

    public async Task<IReadOnlyList<string>> GetEnabledCodesAsync(int schoolId, CancellationToken cancellationToken)
    {
        // A handful of modules, so one small query per call (ENG-EF-02: no-tracking projection).
        var modules = await ModuleStates.ForSchool(_context, schoolId)
            .ToListAsync(cancellationToken);

        return modules
            .Where(m => PortalModule.IsEnabled(m.IsActive, m.IsCore, m.SchoolChoice))
            .Select(m => m.Code)
            .ToList();
    }
}
