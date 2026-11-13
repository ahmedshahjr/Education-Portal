using EducationPortal.Application.AddOnRequests;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Subscriptions.Queries.GetMySubscription;

/// <summary>A paid module the school could add, with its price for the subscription's billing cycle.</summary>
public sealed record AvailableAddOnDto(string Code, string Name, string Description, decimal? Price, bool Requested);

/// <summary>What a school's administrator sees under "My subscription".</summary>
/// <param name="Subscription">The current subscription, or <c>null</c> when the school has none.</param>
public sealed record MySubscriptionDto(
    SubscriptionDto? Subscription,
    IReadOnlyList<AvailableAddOnDto> AvailableAddOns,
    IReadOnlyList<AddOnRequestDto> Requests);

/// <summary>
/// The current school's subscription, the paid modules it could add with their prices, and its add-on requests. The school
/// comes from the host (SEC-AUTHZ-04); a school sees only its own rows (SEC-AUTHZ-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.School)]
public record GetMySubscriptionQuery : IRequest<MySubscriptionDto>;

public class GetMySubscriptionQueryHandler : IRequestHandler<GetMySubscriptionQuery, MySubscriptionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetMySubscriptionQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<MySubscriptionDto> Handle(GetMySubscriptionQuery request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var subscription = await SubscriptionReader.ReadAsync(_context, schoolId, cancellationToken);

        var requests = await AddOnRequestReader.Query(_context, schoolId)
            .Take(100)
            .ToListAsync(cancellationToken);

        var addOns = new List<AvailableAddOnDto>();
        if (subscription is not null)
        {
            var held = subscription.Modules.Where(m => m.Status != TenantModuleStatus.Expired).Select(m => m.Code).ToHashSet();
            var pending = requests.Where(r => r.Status == AddOnRequestStatus.Pending).Select(r => r.ModuleCode).ToHashSet();

            var priceBookId = await _context.Subscriptions.Where(s => s.PublicId == subscription.Id).Select(s => s.PriceBookId).FirstAsync(cancellationToken);
            var prices = await (
                    from i in _context.PriceBookItems.AsNoTracking()
                    join m in _context.Modules on i.ModuleId equals m.Id
                    where i.PriceBookId == priceBookId && i.BillingCycle == subscription.BillingCycle
                    select new { m.Code, i.Price })
                .ToDictionaryAsync(p => p.Code, p => p.Price, cancellationToken);

            var modules = await _context.Modules.AsNoTracking()
                .Where(m => m.IsActive && !m.IsCore)
                .OrderBy(m => m.SortOrder).ThenBy(m => m.Code)
                .Select(m => new { m.Code, m.Name, m.Description })
                .ToListAsync(cancellationToken);

            addOns = modules
                .Where(m => !held.Contains(m.Code))
                .Select(m => new AvailableAddOnDto(m.Code, m.Name, m.Description, prices.TryGetValue(m.Code, out var price) ? price : null, pending.Contains(m.Code)))
                .ToList();
        }

        return new MySubscriptionDto(subscription, addOns, requests);
    }
}
