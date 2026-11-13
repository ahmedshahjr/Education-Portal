using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Plans.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Plans.Commands.CreatePlan;

/// <summary>
/// Adds a plan with its paid modules and returns its id. It starts active and unpriced; prices are added per price
/// book. Master tenant administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record CreatePlanCommand : PlanFields, IRequest<Guid>
{
    /// <summary>The stable code, for example <c>STARTER</c>. It cannot be changed later.</summary>
    public string Code { get; init; } = string.Empty;
}

public class CreatePlanCommandValidator : PlanFieldsValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator(IApplicationDbContext context) : base(context)
    {
        RuleFor(c => c.Code)
            .Must(Plan.IsValidCode)
            .WithMessage(_ => ValidationMessages.PlanCodeFormat)
            .MustAsync(async (code, cancellationToken) => !await context.Plans.AnyAsync(p => p.Code == code, cancellationToken))
            .WithMessage(_ => ValidationMessages.PlanCodeTaken);
    }
}

public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public CreatePlanCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<Guid> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = Plan.Create(request.Code, request.Name, request.Description);
        _context.Plans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);

        await PlanModuleSync.SetModulesAsync(_context, plan, request.Modules, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.PlanCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(Plan),
            targetId: plan.Id.ToString(CultureInfo.InvariantCulture));

        return plan.PublicId;
    }
}
