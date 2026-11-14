using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Subscriptions.Commands.ChangeSubscriptionPlan;

/// <summary>
/// Moves a school's subscription to another plan, keeping its add-on modules. The price becomes the new plan's price plus
/// the add-ons it does not include, from the subscription's price book; modules of the old plan that the new one lacks end
/// now (their data is kept). It takes effect at once with no credit or charge for the current period. Master tenant
/// administrators only (SEC-AUTHZ-07), so the school id may come from the route (SEC-AUTHZ-04). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record ChangeSubscriptionPlanCommand : IRequest
{
    /// <summary>The school's public id, taken from the route (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }

    public string PlanCode { get; init; } = string.Empty;
}

public class ChangeSubscriptionPlanCommandValidator : AbstractValidator<ChangeSubscriptionPlanCommand>
{
    public ChangeSubscriptionPlanCommandValidator()
    {
        RuleFor(c => c.SchoolId).NotEmpty();
        RuleFor(c => c.PlanCode).NotEmpty().MaximumLength(Plan.CodeMaxLength);
    }
}

public class ChangeSubscriptionPlanCommandHandler : IRequestHandler<ChangeSubscriptionPlanCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;
    private readonly IModuleEntitlementStore _entitlements;

    public ChangeSubscriptionPlanCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, TimeProvider timeProvider, IModuleEntitlementStore entitlements)
    {
        _context = context;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
        _entitlements = entitlements;
    }

    public async Task Handle(ChangeSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await _context.Schools.ResolveIdAsync(request.SchoolId, cancellationToken);
        var subscription = await SubscriptionChanges.LoadAsync(_context, schoolId, request.SchoolId, cancellationToken);
        var (_, addOns) = await SubscriptionChanges.CurrentChoiceAsync(_context, subscription, cancellationToken);

        await SubscriptionChanges.ApplyAsync(_context, subscription, request.PlanCode, addOns, _timeProvider, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _entitlements.Invalidate(schoolId);
        await SubscriptionChanges.WriteAuditAsync(_auditLog, subscription);
    }
}
