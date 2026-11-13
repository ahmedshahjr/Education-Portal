using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Schools.Commands.SetSchoolTaxRate;

/// <summary>
/// Sets a school's own tax rate (percent) for its invoices, which replaces its country's price-book rate, or clears it
/// with null so the country's rate applies again. Invoices already issued keep the rate they were issued with. Master
/// tenant administrators only (SEC-AUTHZ-07), so the school id may come from the route (SEC-AUTHZ-04). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record SetSchoolTaxRateCommand : IRequest
{
    /// <summary>The school's public id, taken from the route (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }

    /// <summary>The rate in percent from 0 to 100 with at most two decimals, or null to use the country's.</summary>
    public decimal? TaxRate { get; init; }
}

public class SetSchoolTaxRateCommandValidator : AbstractValidator<SetSchoolTaxRateCommand>
{
    public SetSchoolTaxRateCommandValidator()
    {
        RuleFor(c => c.SchoolId).NotEmpty();
        RuleFor(c => c.TaxRate)
            .Must(rate => PriceBook.IsValidTaxRate(rate!.Value))
            .When(c => c.TaxRate is not null)
            .WithMessage(_ => ValidationMessages.TaxRateRange);
    }
}

public class SetSchoolTaxRateCommandHandler : IRequestHandler<SetSchoolTaxRateCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public SetSchoolTaxRateCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(SetSchoolTaxRateCommand request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools.FirstOrDefaultAsync(s => s.PublicId == request.SchoolId, cancellationToken);
        Guard.Against.NotFound(request.SchoolId, school);

        school.SetTaxRateOverride(request.TaxRate);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SchoolTaxRateChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(School),
            targetId: school.Id.ToString(CultureInfo.InvariantCulture));
    }
}
