using EducationPortal.Application.Branches.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Branches.Queries.GetBranches;

/// <summary>
/// Lists the current school's campuses, the main campus first. The school is the one the request's host resolves to
/// (SEC-AUTHZ-04), so a school sees only its own campuses. Needs the Branches module (AGENTS.md section 2).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Branch)]
public record GetBranchesQuery : IRequest<IReadOnlyList<BranchDto>>;

public class GetBranchesQueryHandler : IRequestHandler<GetBranchesQuery, IReadOnlyList<BranchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetBranchesQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<BranchDto>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var school = await _context.Schools
            .AsNoTracking()
            .Where(s => s.Id == schoolId)
            .Select(s => new { s.CampusCodePrefixEnabled, s.CampusCodePrefix })
            .SingleAsync(cancellationToken);

        var campuses = await _context.Branches
            .AsNoTracking()
            .Where(b => b.SchoolId == schoolId)
            .OrderBy(b => b.Kind)
            .ThenBy(b => b.Name)
            .Select(b => new { b.PublicId, b.Name, b.Code, b.Kind })
            .ToListAsync(cancellationToken);

        var prefix = school.CampusCodePrefixEnabled ? school.CampusCodePrefix ?? string.Empty : string.Empty;

        return campuses
            .Select(c => new BranchDto(c.PublicId, c.Name, c.Code, prefix + c.Code, c.Kind))
            .ToList();
    }
}
