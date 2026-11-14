using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.PriceBooks.Commands.RemovePrice;

/// <summary>
/// Removes the price of a plan or module for one billing cycle from a price book, so it can no longer be sold in that
/// country on that cycle. Answers 404 when there is no such price. Master tenant administrators only (SEC-AUTHZ-07);
/// audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record RemovePriceCommand(Guid PriceBookId, string? PlanCode, string? ModuleCode, BillingCycle BillingCycle) : IRequest;

public class RemovePriceCommandValidator : AbstractValidator<RemovePriceCommand>
{
    public RemovePriceCommandValidator()
    {
        RuleFor(c => c.PriceBookId).NotEmpty();
        RuleFor(c => c.BillingCycle).IsInEnum();
        RuleFor(c => c)
            .Must(c => string.IsNullOrWhiteSpace(c.PlanCode) != string.IsNullOrWhiteSpace(c.ModuleCode))
            .WithMessage(_ => ValidationMessages.PriceTargetRequired)
            .OverridePropertyName(nameof(RemovePriceCommand.PlanCode));
    }
}

public class RemovePriceCommandHandler : IRequestHandler<RemovePriceCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public RemovePriceCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(RemovePriceCommand request, CancellationToken cancellationToken)
    {
        var book = await _context.PriceBooks.FirstOrDefaultAsync(b => b.PublicId == request.PriceBookId, cancellationToken);
        Guard.Against.NotFound(request.PriceBookId, book);

        var planCode = request.PlanCode;
        var moduleCode = request.ModuleCode;
        var item = await (
                from i in _context.PriceBookItems
                join p in _context.Plans on i.PlanId equals p.Id into plans
                from p in plans.DefaultIfEmpty()
                join m in _context.Modules on i.ModuleId equals m.Id into modules
                from m in modules.DefaultIfEmpty()
                where i.PriceBookId == book.Id
                    && i.BillingCycle == request.BillingCycle
                    && (planCode != null ? p.Code == planCode : m.Code == moduleCode)
                select i)
            .FirstOrDefaultAsync(cancellationToken);
        Guard.Against.NotFound(planCode ?? moduleCode ?? string.Empty, item);

        var itemId = item.Id;
        _context.PriceBookItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.PriceChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(PriceBookItem),
            targetId: itemId.ToString(CultureInfo.InvariantCulture));
    }
}
