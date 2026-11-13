using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Subscriptions.Queries.GetSchoolSubscription;

/// <summary>
/// Returns a school's current subscription (the one that is not cancelled) with its modules; 404 when it has none.
/// Master tenant administrators only (SEC-AUTHZ-07), so the school id may come from the route (SEC-AUTHZ-04).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetSchoolSubscriptionQuery(Guid SchoolId) : IRequest<SubscriptionDto>;

public class GetSchoolSubscriptionQueryHandler : IRequestHandler<GetSchoolSubscriptionQuery, SubscriptionDto>
{
    private readonly IApplicationDbContext _context;

    public GetSchoolSubscriptionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionDto> Handle(GetSchoolSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var schoolId = await _context.Schools.ResolveIdAsync(request.SchoolId, cancellationToken);

        var subscription = await SubscriptionReader.ReadAsync(_context, schoolId, cancellationToken);
        Guard.Against.NotFound(request.SchoolId, subscription);

        return subscription;
    }
}
