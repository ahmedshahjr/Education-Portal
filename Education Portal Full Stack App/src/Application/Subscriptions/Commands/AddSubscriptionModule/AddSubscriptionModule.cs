using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Subscriptions.Commands.AddSubscriptionModule;

/// <summary>
/// Adds a paid add-on module (and the modules it requires) to a school's subscription. The price grows by the module's price in the subscription's price book. It takes effect at once with no charge for the current period. Master tenant administrators only (SEC-AUTHZ-07), so the school id may come from the route (SEC-AUTHZ-04).
/// Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record AddSubscriptionModuleCommand : IRequest
{
    /// <summary>The school's public id, taken from the route (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }

    public string ModuleCode { get; init; } = string.Empty;
}

public class AddSubscriptionModuleCommandValidator : AbstractValidator<AddSubscriptionModuleCommand>
{
    public AddSubscriptionModuleCommandValidator()
    {
        RuleFor(c => c.SchoolId).NotEmpty();
        RuleFor(c => c.ModuleCode).NotEmpty().MaximumLength(PortalModule.CodeMaxLength);
    }
}

public class AddSubscriptionModuleCommandHandler : IRequestHandler<AddSubscriptionModuleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;
    private readonly IModuleEntitlementStore _entitlements;

    public AddSubscriptionModuleCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, TimeProvider timeProvider, IModuleEntitlementStore entitlements)
    {
        _context = context;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
        _entitlements = entitlements;
    }

    public async Task Handle(AddSubscriptionModuleCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await _context.Schools.ResolveIdAsync(request.SchoolId, cancellationToken);
        var subscription = await SubscriptionChanges.LoadAsync(_context, schoolId, request.SchoolId, cancellationToken);
        var (planCode, addOns) = await SubscriptionChanges.CurrentChoiceAsync(_context, subscription, cancellationToken);

        var held = subscription.Modules
            .Where(m => m.IsSchoolWide && m.Status != TenantModuleStatus.Expired)
            .Join(_context.Modules, m => m.ModuleId, m => m.Id, (_, m) => m.Code)
            .ToHashSet();
        if (held.Contains(request.ModuleCode))
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new FluentValidation.Results.ValidationFailure(nameof(request.ModuleCode), ValidationMessages.ModuleAlreadyInSubscription)]);
        }

        addOns.Add(request.ModuleCode);
        var granted = await SubscriptionChanges.ApplyAsync(_context, subscription, planCode, addOns, _timeProvider, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _entitlements.Invalidate(schoolId);
        await SubscriptionChanges.WriteAuditAsync(_auditLog, subscription);
    }
}
