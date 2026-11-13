using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Subscriptions.Commands.SuspendSubscription;

/// <summary>
/// Suspends a school's subscription: it and every module it gives stop working (requests to them are refused with 403), and the data is kept. Master tenant administrators only
/// (SEC-AUTHZ-07), so the school id may come from the route (SEC-AUTHZ-04). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record SuspendSubscriptionCommand(Guid SchoolId) : IRequest;

public class SuspendSubscriptionCommandValidator : AbstractValidator<SuspendSubscriptionCommand>
{
    public SuspendSubscriptionCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.SchoolId)
            .MustAsync(async (schoolId, cancellationToken) => !await (
                    from s in context.Subscriptions
                    join school in context.Schools on s.SchoolId equals school.Id
                    where school.PublicId == schoolId && s.Status != SubscriptionStatus.Cancelled && (s.Status == SubscriptionStatus.Suspended)
                    select s.Id).AnyAsync(cancellationToken))
            .WithMessage(_ => ValidationMessages.SubscriptionCannotBeSuspended);
    }
}

public class SuspendSubscriptionCommandHandler : IRequestHandler<SuspendSubscriptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly IModuleEntitlementStore _entitlements;

    public SuspendSubscriptionCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, IModuleEntitlementStore entitlements)
    {
        _context = context;
        _auditLog = auditLog;
        _entitlements = entitlements;
    }

    public async Task Handle(SuspendSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await _context.Schools.ResolveIdAsync(request.SchoolId, cancellationToken);
        var subscription = await SubscriptionChanges.LoadAsync(_context, schoolId, request.SchoolId, cancellationToken);

        subscription.Suspend();
        await _context.SaveChangesAsync(cancellationToken);

        _entitlements.Invalidate(schoolId);
        await SubscriptionChanges.WriteAuditAsync(_auditLog, subscription);
    }
}
