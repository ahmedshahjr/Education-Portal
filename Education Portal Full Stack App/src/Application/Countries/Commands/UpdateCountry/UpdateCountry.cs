using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Countries.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Countries.Commands.UpdateCountry;

/// <summary>
/// Changes a country's languages, time zone and currency. Every school in the country follows at its next request.
/// Master tenant administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record UpdateCountryCommand : CountryFields, IRequest
{
    public int Id { get; init; }
}

public class UpdateCountryCommandValidator : CountryFieldsValidator<UpdateCountryCommand>
{
    public UpdateCountryCommandValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0);
    }
}

public class UpdateCountryCommandHandler : IRequestHandler<UpdateCountryCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public UpdateCountryCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(UpdateCountryCommand request, CancellationToken cancellationToken)
    {
        var country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, country);

        country.Update(request.Name, request.Languages, request.DefaultLanguage, request.TimeZoneId, request.CurrencyCode);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.CountryChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Country),
            targetId: country.Id.ToString(CultureInfo.InvariantCulture));
    }
}
