using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AddOnRequests.Commands.ApproveAddOnRequest;

/// <summary>
/// Approves a pending add-on request: the module (and what it requires) is added to the school's subscription in the same
/// save, exactly as the master tenant adding it by hand. Master tenant administrators only (SEC-AUTHZ-07); audited
/// (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record ApproveAddOnRequestCommand(Guid Id) : IRequest;

public class ApproveAddOnRequestCommandValidator : AbstractValidator<ApproveAddOnRequestCommand>
{
    public ApproveAddOnRequestCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.Id)
            .MustAsync(async (id, cancellationToken) => !await context.AddOnRequests.AnyAsync(r => r.PublicId == id && r.Status != AddOnRequestStatus.Pending, cancellationToken))
            .WithMessage(_ => ValidationMessages.AddOnRequestNotPending);
    }
}

public class ApproveAddOnRequestCommandHandler : IRequestHandler<ApproveAddOnRequestCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;
    private readonly IModuleEntitlementStore _entitlements;

    public ApproveAddOnRequestCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, TimeProvider timeProvider, IModuleEntitlementStore entitlements)
    {
        _context = context;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
        _entitlements = entitlements;
    }

    public async Task Handle(ApproveAddOnRequestCommand request, CancellationToken cancellationToken)
    {
        var addOnRequest = await _context.AddOnRequests.FirstOrDefaultAsync(r => r.PublicId == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, addOnRequest);

        var code = await _context.Modules.Where(m => m.Id == addOnRequest.ModuleId).Select(m => m.Code).FirstAsync(cancellationToken);
        var subscription = await SubscriptionChanges.LoadAsync(_context, addOnRequest.SchoolId, request.Id, cancellationToken);
        var (planCode, addOns) = await SubscriptionChanges.CurrentChoiceAsync(_context, subscription, cancellationToken);

        if (!addOns.Contains(code))
        {
            addOns.Add(code);
        }

        await SubscriptionChanges.ApplyAsync(_context, subscription, planCode, addOns, _timeProvider, cancellationToken);
        addOnRequest.Approve(_timeProvider.GetUtcNow());

        // One save: the request's decision and the subscription's new modules stand or fall together.
        await _context.SaveChangesAsync(cancellationToken);

        _entitlements.Invalidate(addOnRequest.SchoolId);
        await _auditLog.WriteAsync(
            SecurityAuditEventType.AddOnRequestDecided,
            SecurityAuditOutcome.Success,
            targetType: nameof(AddOnRequest),
            targetId: addOnRequest.Id.ToString(CultureInfo.InvariantCulture));
    }
}
