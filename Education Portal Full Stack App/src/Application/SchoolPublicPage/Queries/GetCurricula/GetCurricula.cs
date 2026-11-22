using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Queries.GetCurricula;

/// <summary>Lists the curricula a school shows on its public page. Needs the School module.</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record GetCurriculaQuery
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }
}

public class GetCurriculaQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetCurriculaQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<CurriculumKind>> Handle(GetCurriculaQuery request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        return await _context.SchoolCurricula
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId)
            .OrderBy(c => c.Kind)
            .Select(c => c.Kind)
            .ToListAsync(cancellationToken);
    }
}
