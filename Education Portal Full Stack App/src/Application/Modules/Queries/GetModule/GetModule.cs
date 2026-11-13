using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Queries.GetModule;

/// <summary>Returns one module of the catalogue by code, with what it requires. Master tenant administrators only (SEC-AUTHZ-07).</summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetModuleQuery(string Code) : IRequest<ModuleDto>;

public class GetModuleQueryHandler : IRequestHandler<GetModuleQuery, ModuleDto>
{
    private readonly IApplicationDbContext _context;

    public GetModuleQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ModuleDto> Handle(GetModuleQuery request, CancellationToken cancellationToken)
    {
        var module = (await ModuleCatalogReader.LoadAsync(_context, request.Code, cancellationToken)).SingleOrDefault();
        Guard.Against.NotFound(request.Code, module);

        return module;
    }
}
