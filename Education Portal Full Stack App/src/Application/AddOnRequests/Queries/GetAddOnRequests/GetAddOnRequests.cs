using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AddOnRequests.Queries.GetAddOnRequests;

/// <summary>
/// Lists add-on requests of every school, newest first, optionally only those with one status (pending ones are what the
/// master tenant acts on). Master tenant administrators only (SEC-AUTHZ-07).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetAddOnRequestsQuery(AddOnRequestStatus? Status = null) : IRequest<IReadOnlyList<AddOnRequestDto>>;

public class GetAddOnRequestsQueryValidator : AbstractValidator<GetAddOnRequestsQuery>
{
    public GetAddOnRequestsQueryValidator()
    {
        RuleFor(q => q.Status).IsInEnum();
    }
}

public class GetAddOnRequestsQueryHandler : IRequestHandler<GetAddOnRequestsQuery, IReadOnlyList<AddOnRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAddOnRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AddOnRequestDto>> Handle(GetAddOnRequestsQuery request, CancellationToken cancellationToken)
    {
        return await AddOnRequestReader.Query(_context, status: request.Status)
            .Take(500)
            .ToListAsync(cancellationToken);
    }
}
