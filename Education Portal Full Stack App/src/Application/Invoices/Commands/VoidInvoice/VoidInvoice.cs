using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Invoices.Commands.VoidInvoice;

/// <summary>
/// Voids an open invoice (a mistake, or a deal that fell through). The row stays for audit and the period can be
/// invoiced again. A paid invoice cannot be voided. Master tenant administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record VoidInvoiceCommand(Guid Id) : IRequest;

public class VoidInvoiceCommandValidator : AbstractValidator<VoidInvoiceCommand>
{
    public VoidInvoiceCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.Id)
            .MustAsync(async (id, cancellationToken) => !await context.Invoices.AnyAsync(i => i.PublicId == id && i.Status != InvoiceStatus.Open, cancellationToken))
            .WithMessage(_ => ValidationMessages.InvoiceNotOpen);
    }
}

public class VoidInvoiceCommandHandler : IRequestHandler<VoidInvoiceCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public VoidInvoiceCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(VoidInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.PublicId == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, invoice);

        invoice.Void();
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.InvoiceVoided,
            SecurityAuditOutcome.Success,
            targetType: nameof(Invoice),
            targetId: invoice.Id.ToString(CultureInfo.InvariantCulture));
    }
}
