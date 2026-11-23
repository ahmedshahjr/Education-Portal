using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Subscriptions.Queries.GetTrialNotice;

/// <summary>Whether the school of the request's host is on its free trial, and how many whole days are left.</summary>
/// <param name="IsTrial"><c>true</c> only while the school's subscription is on its trial.</param>
/// <param name="DaysRemaining">The whole days left, rounded up, never below 0. 0 when <paramref name="IsTrial"/> is false.</param>
public sealed record TrialNoticeDto(bool IsTrial, int DaysRemaining);

/// <summary>
/// The trial notice shown above the school's sign-in page, so it is open without sign-in. Like branding it is read for the
/// host's school (the school comes from the host, never from the caller) and holds the least that is needed: a flag and a
/// day count, no plan, price or dates. The master tenant has no trial. A school without a domain signs in on the master
/// tenant's domain, so it shows no notice there; it sees the in-app banner after sign-in.
/// </summary>
public record GetTrialNoticeQuery;

public class GetTrialNoticeQueryHandler
{
    private static readonly TrialNoticeDto NoTrial = new(false, 0);

    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;
    private readonly TimeProvider _timeProvider;

    public GetTrialNoticeQueryHandler(IApplicationDbContext context, ICurrentTenant tenant, TimeProvider timeProvider)
    {
        _context = context;
        _tenant = tenant;
        _timeProvider = timeProvider;
    }

    public async Task<TrialNoticeDto> Handle(GetTrialNoticeQuery request, CancellationToken cancellationToken)
    {
        if (_tenant.IsMaster)
        {
            return NoTrial;
        }

        var schoolId = _tenant.SchoolId;

        var trialEnd = await _context.Subscriptions
            .AsNoTracking()
            .Where(s => s.SchoolId == schoolId && s.Status == SubscriptionStatus.Trial)
            .Select(s => s.TrialEndDate)
            .FirstOrDefaultAsync(cancellationToken);

        return trialEnd is { } end ? new TrialNoticeDto(true, TrialDays.Remaining(end, _timeProvider.GetUtcNow())) : NoTrial;
    }
}
