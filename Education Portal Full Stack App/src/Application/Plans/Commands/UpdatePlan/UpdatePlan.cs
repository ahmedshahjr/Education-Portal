using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Plans.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Plans.Commands.UpdatePlan;

/// <summary>
/// Changes a plan's name, description, active switch and modules (the list replaces the plan's modules). An inactive
/// plan cannot be chosen for new subscriptions. The code never changes. Master tenant administrators only
/// (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record UpdatePlanCommand : PlanFields, IRequest
{
    /// <summary>The plan's public id, taken from the route.</summary>
    public Guid Id { get; init; }

    public bool IsActive { get; init; }
}

public class UpdatePlanCommandValidator : PlanFieldsValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator(IApplicationDbContext context) : base(context)
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public UpdatePlanCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans.FirstOrDefaultAsync(p => p.PublicId == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, plan);

        plan.Update(request.Name, request.Description);

        if (request.IsActive)
        {
            plan.Activate();
        }
        else
        {
            plan.Deactivate();
        }

        await PlanModuleSync.SetModulesAsync(_context, plan, request.Modules, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.PlanChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Plan),
            targetId: plan.Id.ToString(CultureInfo.InvariantCulture));
    }
}
