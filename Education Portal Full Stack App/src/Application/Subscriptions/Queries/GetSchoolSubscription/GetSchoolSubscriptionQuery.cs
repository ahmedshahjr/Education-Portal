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

        var subscription = await (
                from s in _context.Subscriptions.AsNoTracking()
                join p in _context.Plans on s.PlanId equals p.Id into plans
                from p in plans.DefaultIfEmpty()
                join b in _context.PriceBooks on s.PriceBookId equals b.Id
                where s.SchoolId == schoolId && s.Status != SubscriptionStatus.Cancelled
                select new { s.Id, s.PublicId, PlanCode = p.Code, b.CountryCode, s.CurrencyCode, s.BillingCycle, s.Amount, s.Status, s.StartDate, s.EndDate, s.TrialEndDate })
            .FirstOrDefaultAsync(cancellationToken);
        Guard.Against.NotFound(request.SchoolId, subscription);

        var modules = await (
                from t in _context.TenantModules.AsNoTracking()
                join m in _context.Modules on t.ModuleId equals m.Id
                where t.SubscriptionId == subscription.Id
                orderby m.SortOrder, m.Code
                select new TenantModuleDto(m.Code, t.Status, t.ValidFrom, t.ValidTo))
            .ToListAsync(cancellationToken);

        return new SubscriptionDto(
            subscription.PublicId,
            subscription.PlanCode,
            subscription.CountryCode,
            subscription.CurrencyCode,
            subscription.BillingCycle,
            subscription.Amount,
            subscription.Status,
            subscription.StartDate,
            subscription.EndDate,
            subscription.TrialEndDate,
            modules);
    }
}
