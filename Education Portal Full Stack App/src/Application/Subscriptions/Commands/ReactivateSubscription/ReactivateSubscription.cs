using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Subscriptions.Commands.ReactivateSubscription;

/// <summary>
/// Brings a suspended or past-due subscription back to Active with its modules. Master tenant administrators only
/// (SEC-AUTHZ-07), so the school id may come from the route (SEC-AUTHZ-04). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record ReactivateSubscriptionCommand(Guid SchoolId) : IRequest;

public class ReactivateSubscriptionCommandValidator : AbstractValidator<ReactivateSubscriptionCommand>
{
    public ReactivateSubscriptionCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.SchoolId)
            .MustAsync(async (schoolId, cancellationToken) => !await (
                    from s in context.Subscriptions
                    join school in context.Schools on s.SchoolId equals school.Id
                    where school.PublicId == schoolId && s.Status != SubscriptionStatus.Cancelled && (s.Status != SubscriptionStatus.Suspended && s.Status != SubscriptionStatus.PastDue)
                    select s.Id).AnyAsync(cancellationToken))
            .WithMessage(_ => ValidationMessages.SubscriptionCannotBeReactivated);
    }
}

public class ReactivateSubscriptionCommandHandler : IRequestHandler<ReactivateSubscriptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly IModuleEntitlementStore _entitlements;

    public ReactivateSubscriptionCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, IModuleEntitlementStore entitlements)
    {
        _context = context;
        _auditLog = auditLog;
        _entitlements = entitlements;
    }

    public async Task Handle(ReactivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await _context.Schools.ResolveIdAsync(request.SchoolId, cancellationToken);
        var subscription = await SubscriptionChanges.LoadAsync(_context, schoolId, request.SchoolId, cancellationToken);

        subscription.Reactivate();
        await _context.SaveChangesAsync(cancellationToken);

        _entitlements.Invalidate(schoolId);
        await SubscriptionChanges.WriteAuditAsync(_auditLog, subscription);
    }
}
