using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Countries.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Countries.Commands.CreateCountry;

/// <summary>Adds a country that schools can be assigned to. Master tenant administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).</summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record CreateCountryCommand : CountryFields, IRequest<Guid>
{
    /// <summary>The ISO 3166-1 alpha-2 code, for example <c>PK</c>. It cannot be changed later.</summary>
    public string Code { get; init; } = string.Empty;
}

public class CreateCountryCommandValidator : CountryFieldsValidator<CreateCountryCommand>
{
    public CreateCountryCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.Code)
            .Must(Country.IsValidCode)
            .WithMessage("The code must be two letters (ISO 3166-1 alpha-2), for example PK.")
            .MustAsync(async (code, cancellationToken) =>
                !await context.Countries.AnyAsync(c => c.Code == code.ToUpperInvariant(), cancellationToken))
            .WithMessage("A country with this code already exists.");
    }
}

public class CreateCountryCommandHandler : IRequestHandler<CreateCountryCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public CreateCountryCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task<Guid> Handle(CreateCountryCommand request, CancellationToken cancellationToken)
    {
        var country = Country.Create(request.Code, request.Name, request.Languages, request.DefaultLanguage, request.TimeZoneId, request.CurrencyCode);

        _context.Countries.Add(country);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.CountryChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Country),
            targetId: country.Id.ToString(CultureInfo.InvariantCulture));

        return country.PublicId;
    }
}
