using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.PriceBooks.Commands.CreatePriceBook;

/// <summary>
/// Adds the price book of a country, empty and active, and returns its id. A country has one price book. Master tenant
/// administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record CreatePriceBookCommand : IRequest<Guid>
{
    /// <summary>ISO 3166-1 alpha-2 code, for example <c>PK</c>. It cannot be changed later.</summary>
    public string CountryCode { get; init; } = string.Empty;

    /// <summary>ISO 4217 code of the currency every price is in, for example <c>PKR</c>.</summary>
    public string CurrencyCode { get; init; } = string.Empty;
}

public class CreatePriceBookCommandValidator : AbstractValidator<CreatePriceBookCommand>
{
    public CreatePriceBookCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.CountryCode)
            .Must(code => Country.IsValidCode(code))
            .MustAsync(async (code, cancellationToken) => !await context.PriceBooks.AnyAsync(b => b.CountryCode == code.ToUpper(), cancellationToken))
            .WithMessage(_ => ValidationMessages.PriceBookCountryTaken);

        RuleFor(c => c.CurrencyCode).Must(PriceBook.IsValidCurrencyCode);
    }
}

public class CreatePriceBookCommandHandler : IRequestHandler<CreatePriceBookCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public CreatePriceBookCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<Guid> Handle(CreatePriceBookCommand request, CancellationToken cancellationToken)
    {
        var book = PriceBook.Create(request.CountryCode, request.CurrencyCode);
        _context.PriceBooks.Add(book);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.PriceBookCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(PriceBook),
            targetId: book.Id.ToString(CultureInfo.InvariantCulture));

        return book.PublicId;
    }
}
