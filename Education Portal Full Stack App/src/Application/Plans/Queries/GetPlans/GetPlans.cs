using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Plans.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Plans.Queries.GetPlans;

/// <summary>Lists the plans by name, with their paid modules. Master tenant administrators only (SEC-AUTHZ-07).</summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetPlansQuery : IRequest<IReadOnlyList<PlanDto>>;

public class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, IReadOnlyList<PlanDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPlansQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PlanDto>> Handle(GetPlansQuery request, CancellationToken cancellationToken) =>
        await PlanReader.LoadAsync(_context, id: null, cancellationToken);
}
