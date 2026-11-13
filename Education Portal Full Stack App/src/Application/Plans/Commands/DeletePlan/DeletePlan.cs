using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Plans.Commands.DeletePlan;

/// <summary>
/// Deletes a plan. It is a soft delete: the row and its module list stay for audit and for anything that points to it
/// by id, the plan disappears from every list, and its code can be used again. Its prices are removed from every price
/// book, so it can no longer be sold. Master tenant administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record DeletePlanCommand(Guid Id) : IRequest;

public class DeletePlanCommandHandler : IRequestHandler<DeletePlanCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public DeletePlanCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeletePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans.FirstOrDefaultAsync(p => p.PublicId == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, plan);

        var prices = await _context.PriceBookItems.Where(i => i.PlanId == plan.Id).ToListAsync(cancellationToken);
        _context.PriceBookItems.RemoveRange(prices);

        plan.Delete(_timeProvider.GetUtcNow(), _user.Id);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.PlanDeleted,
            SecurityAuditOutcome.Success,
            targetType: nameof(Plan),
            targetId: plan.Id.ToString(CultureInfo.InvariantCulture));
    }
}
