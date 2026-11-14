using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.PriceBooks.Commands.UpdatePriceBook;

/// <summary>
/// Changes a price book's currency, tax rate and active switch. The prices are numbers in the book's currency, so changing the
/// currency does not convert them: the master tenant re-enters the prices. Master tenant administrators only
/// (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record UpdatePriceBookCommand : IRequest
{
    /// <summary>The price book's public id, taken from the route.</summary>
    public Guid Id { get; init; }

    public string CurrencyCode { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    /// <summary>The tax rate in percent that invoices add; leave it out to keep the current one.</summary>
    public decimal? TaxRate { get; init; }
}

public class UpdatePriceBookCommandValidator : AbstractValidator<UpdatePriceBookCommand>
{
    public UpdatePriceBookCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.CurrencyCode).Must(PriceBook.IsValidCurrencyCode);
        RuleFor(c => c.TaxRate).Must(rate => PriceBook.IsValidTaxRate(rate!.Value)).When(c => c.TaxRate is not null).WithMessage(_ => ValidationMessages.TaxRateRange);
    }
}

public class UpdatePriceBookCommandHandler : IRequestHandler<UpdatePriceBookCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public UpdatePriceBookCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(UpdatePriceBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _context.PriceBooks.FirstOrDefaultAsync(b => b.PublicId == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, book);

        book.SetCurrency(request.CurrencyCode);

        if (request.TaxRate is { } taxRate)
        {
            book.SetTaxRate(taxRate);
        }

        if (request.IsActive)
        {
            book.Activate();
        }
        else
        {
            book.Deactivate();
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.PriceBookChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(PriceBook),
            targetId: book.Id.ToString(CultureInfo.InvariantCulture));
    }
}
