using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Subscriptions.Commands.RemoveSubscriptionModule;

/// <summary>
/// Removes an add-on module from a school's subscription: it ends now and its data is kept. A module the plan includes, or that another module of the subscription requires, cannot be removed. It takes effect at once with no credit for the current period. Master tenant administrators only (SEC-AUTHZ-07), so the school id may come from the route (SEC-AUTHZ-04).
/// Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record RemoveSubscriptionModuleCommand : IRequest
{
    /// <summary>The school's public id, taken from the route (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }

    public string ModuleCode { get; init; } = string.Empty;
}

public class RemoveSubscriptionModuleCommandValidator : AbstractValidator<RemoveSubscriptionModuleCommand>
{
    public RemoveSubscriptionModuleCommandValidator()
    {
        RuleFor(c => c.SchoolId).NotEmpty();
        RuleFor(c => c.ModuleCode).NotEmpty().MaximumLength(PortalModule.CodeMaxLength);
    }
}

public class RemoveSubscriptionModuleCommandHandler : IRequestHandler<RemoveSubscriptionModuleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;
    private readonly IModuleEntitlementStore _entitlements;

    public RemoveSubscriptionModuleCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, TimeProvider timeProvider, IModuleEntitlementStore entitlements)
    {
        _context = context;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
        _entitlements = entitlements;
    }

    public async Task Handle(RemoveSubscriptionModuleCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await _context.Schools.ResolveIdAsync(request.SchoolId, cancellationToken);
        var subscription = await SubscriptionChanges.LoadAsync(_context, schoolId, request.SchoolId, cancellationToken);
        var (planCode, addOns) = await SubscriptionChanges.CurrentChoiceAsync(_context, subscription, cancellationToken);

        if (!addOns.Contains(request.ModuleCode))
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new FluentValidation.Results.ValidationFailure(nameof(request.ModuleCode), ValidationMessages.ModuleNotInSubscription)]);
        }

        addOns.Remove(request.ModuleCode);
        var granted = await SubscriptionChanges.ApplyAsync(_context, subscription, planCode, addOns, _timeProvider, cancellationToken);

        if (granted.Contains(request.ModuleCode))
        {
            // Another module or the plan still needs it, so removing it would change nothing.
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new FluentValidation.Results.ValidationFailure(nameof(request.ModuleCode), ValidationMessages.ModuleStillIncluded)]);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _entitlements.Invalidate(schoolId);
        await SubscriptionChanges.WriteAuditAsync(_auditLog, subscription);
    }
}
