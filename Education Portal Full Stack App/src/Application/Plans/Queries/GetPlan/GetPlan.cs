using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Plans.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Plans.Queries.GetPlan;

/// <summary>Returns one plan with its paid modules. Master tenant administrators only (SEC-AUTHZ-07).</summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetPlanQuery(Guid Id) : IRequest<PlanDto>;

public class GetPlanQueryHandler : IRequestHandler<GetPlanQuery, PlanDto>
{
    private readonly IApplicationDbContext _context;

    public GetPlanQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlanDto> Handle(GetPlanQuery request, CancellationToken cancellationToken)
    {
        var plan = (await PlanReader.LoadAsync(_context, request.Id, cancellationToken)).SingleOrDefault();
        Guard.Against.NotFound(request.Id, plan);

        return plan;
    }
}
