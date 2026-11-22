using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Countries.Commands.UpdateCountryRegionalSettings;

/// <summary>
/// Changes a country's regional settings: digits after the decimal point in its currency, the pattern a local phone number
/// must match, the first day of the week and the weekend days. Master tenant administrators only (SEC-AUTHZ-07); audited
/// (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record UpdateCountryRegionalSettingsCommand
{
    /// <summary>The country's public id, taken from the route.</summary>
    public Guid Id { get; init; }

    public int CurrencyDecimals { get; init; } = 2;

    /// <summary>A regular expression, or blank for none.</summary>
    public string? PhonePattern { get; init; }

    public DayOfWeek WeekStartDay { get; init; } = DayOfWeek.Monday;

    public IReadOnlyList<DayOfWeek> WeekendDays { get; init; } = [];
}

public class UpdateCountryRegionalSettingsCommandValidator : AbstractValidator<UpdateCountryRegionalSettingsCommand>
{
    public UpdateCountryRegionalSettingsCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.CurrencyDecimals).InclusiveBetween(0, Country.MaxCurrencyDecimals);
        RuleFor(c => c.PhonePattern)
            .Must(pattern => string.IsNullOrWhiteSpace(pattern) || Country.IsValidPhonePattern(pattern.Trim()))
            .WithMessage(_ => ValidationMessages.PhonePatternInvalid);
        RuleFor(c => c.WeekStartDay).IsInEnum();
        RuleFor(c => c.WeekendDays)
            .NotNull()
            .Must(days => days.Count <= Country.MaxWeekendDays && days.Distinct().Count() == days.Count && days.All(Enum.IsDefined))
            .WithMessage(_ => ValidationMessages.WeekendDaysInvalid);
    }
}

public class UpdateCountryRegionalSettingsCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public UpdateCountryRegionalSettingsCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(UpdateCountryRegionalSettingsCommand request, CancellationToken cancellationToken)
    {
        var country = await _context.Countries.FirstOrDefaultAsync(c => c.PublicId == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, country);

        country.SetRegionalSettings(request.CurrencyDecimals, request.PhonePattern, request.WeekStartDay, request.WeekendDays);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.CountryChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Country),
            targetId: country.Id.ToString(CultureInfo.InvariantCulture));
    }
}
