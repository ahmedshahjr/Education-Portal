using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Subscriptions.Commands.ExtendSubscription;

/// <summary>
/// Moves the end of a school's current period (and of its trial) later by 1 to 366 days without a payment, so every module
/// stays valid until then. Master tenant administrators only (SEC-AUTHZ-07), so the school id may come from the route
/// (SEC-AUTHZ-04). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record ExtendSubscriptionCommand : IRequest
{
    /// <summary>The school's public id, taken from the route (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }

    public int Days { get; init; }
}

public class ExtendSubscriptionCommandValidator : AbstractValidator<ExtendSubscriptionCommand>
{
    public ExtendSubscriptionCommandValidator()
    {
        RuleFor(c => c.SchoolId).NotEmpty();
        RuleFor(c => c.Days).InclusiveBetween(1, Subscription.MaxExtensionDays);
    }
}

public class ExtendSubscriptionCommandHandler : IRequestHandler<ExtendSubscriptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly IModuleEntitlementStore _entitlements;

    public ExtendSubscriptionCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, IModuleEntitlementStore entitlements)
    {
        _context = context;
        _auditLog = auditLog;
        _entitlements = entitlements;
    }

    public async Task Handle(ExtendSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await _context.Schools.ResolveIdAsync(request.SchoolId, cancellationToken);
        var subscription = await SubscriptionChanges.LoadAsync(_context, schoolId, request.SchoolId, cancellationToken);

        subscription.Extend(request.Days);
        await _context.SaveChangesAsync(cancellationToken);

        _entitlements.Invalidate(schoolId);
        await SubscriptionChanges.WriteAuditAsync(_auditLog, subscription);
    }
}
