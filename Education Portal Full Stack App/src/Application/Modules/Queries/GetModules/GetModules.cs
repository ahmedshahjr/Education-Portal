using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Queries.GetModules;

/// <summary>Lists every module and whether it is core, default or custom. Master tenant administrators only (SEC-AUTHZ-07).</summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetModulesQuery : IRequest<IReadOnlyList<ModuleDto>>;

public class GetModulesQueryHandler : IRequestHandler<GetModulesQuery, IReadOnlyList<ModuleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetModulesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ModuleDto>> Handle(GetModulesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Modules
            .AsNoTracking()
            .OrderBy(m => m.Id)
            .Select(m => new ModuleDto(m.Key, m.Name, m.Description, m.IsCore, m.IsDefault))
            .ToListAsync(cancellationToken);
    }
}
